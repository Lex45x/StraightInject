using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace StraightInject.Core.ServiceConstructors
{
    internal class InstanceServiceCompiler : IServiceCompiler
    {
        public Action<ILGenerator> Construct(IService service, Dictionary<Type, Action<ILGenerator>> knownTypes, Dictionary<Type, IService> dependencies)
        {
            throw new NotImplementedException();
        }
    }
}