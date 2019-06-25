using System;
using System.Linq.Expressions;
using StraightInject.Core.ConstructorResolver;
using StraightInject.Services;

namespace StraightInject.Core.Services
{
    public static class ConstructableServiceExtensions
    {
        public static IConstructableService WithConstructor<TService>(this IConstructableService constructableService,
            Expression<Func<TService>> expression)
        {
            var expressionConstructorResolver = new ExpressionConstructorResolver<TService>(expression);

            constructableService.OverrideConstructorResolver(expressionConstructorResolver);

            return constructableService;
        }
    }
}