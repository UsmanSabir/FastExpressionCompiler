﻿using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using BenchmarkDotNet.Attributes;

using LEC = FastExpressionCompiler.LightExpression.ExpressionCompiler;
using LE = FastExpressionCompiler.LightExpression.Expression;

namespace FastExpressionCompiler.Benchmarks
{
    [MemoryDiagnoser]
    public class ExprInfoVsExpr_CreateAndCompile_SimpleExpr
    {
        public class A { }
        public class B { }

        public class X
        {
            public A A { get; }
            public B B { get; }
            public X(A a, B b)
            {
                A = a;
                B = b;
            }
        }

        private static readonly ConstructorInfo _xCtor = typeof(X).GetTypeInfo().GetConstructors()[0];
        private static readonly ConstructorInfo _aCtor = typeof(A).GetTypeInfo().GetConstructors()[0];
        private static readonly ConstructorInfo _bCtor = typeof(B).GetTypeInfo().GetConstructors()[0];

        [Benchmark]
        public object CreateExpression_and_Compile()
        {
            var expr = Expression.Lambda(Expression.New(_xCtor, Expression.New(_aCtor), Expression.New(_bCtor)));
            return expr.Compile();
        }

        [Benchmark]
        public object CreateExpression_and_FastCompile()
        {
            var expr = Expression.Lambda(Expression.New(_xCtor, Expression.New(_aCtor), Expression.New(_bCtor)));
            return expr.TryCompile<Func<object>>();
        }

        [Benchmark(Baseline = true)]
        public object CreateExpressionInfo_and_FastCompile()
        {
            var expr = LE.Lambda(LE.New(_xCtor, LE.New(_aCtor), LE.New(_bCtor)));
            return LEC.TryCompile<Func<object>>(expr);
        }

        [Benchmark]
        public object ManualEmit()
        {
            var method = new DynamicMethod(string.Empty, typeof(object), Type.EmptyTypes,
                typeof(ExprInfoVsExpr_CreateAndCompile_SimpleExpr), skipVisibility: true);
            var il = method.GetILGenerator();

            il.Emit(OpCodes.Newobj, _aCtor);
            il.Emit(OpCodes.Newobj, _bCtor);
            il.Emit(OpCodes.Newobj, _xCtor);

            il.Emit(OpCodes.Ret);

            return method.CreateDelegate(typeof(Func<object>), null);
        }
        
        [Benchmark]
        public object ActivatorCreateInstance()
        {
            return Activator.CreateInstance(typeof(X),
                Activator.CreateInstance(typeof(A)),
                Activator.CreateInstance(typeof(B)));
        }
    }
}
