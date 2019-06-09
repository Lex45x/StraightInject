﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using StraightInject.Services;

namespace StraightInject.Core.ConstructorResolver
{
    public class ExpressionConstructorResolver<TComponent> : IConstructorResolver
    {
        private readonly Expression<Func<TComponent>> expression;

        public ExpressionConstructorResolver(Expression<Func<TComponent>> expression)
        {
            this.expression = expression;
        }

        public ConstructorInfo Resolve(Type component, Dictionary<Type, IService> dependencies)
        {
            if (!(expression.Body is NewExpression expressionBody))
            {
                throw new NotImplementedException("Expression body must be a constructor call");
            }

            var constructor = expressionBody.Constructor;
            foreach (var parameter in constructor.GetParameters())
            {
                if (!dependencies.ContainsKey(parameter.ParameterType))
                {
                    throw new NotImplementedException(
                        $"Can't resolve parameter {parameter.ParameterType.FullName} for selected constructor of {component}");
                }
            }

            return constructor;
        }
    }
}