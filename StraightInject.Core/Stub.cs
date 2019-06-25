using System;

namespace StraightInject.Core
{
    /// <summary>
    /// Static class that must be used for a constructor specification
    /// </summary>
    public static class Stub
    {
        /// <summary>
        /// Use as a stub for dependency instance: () => new Service(Stub.Of&lt;IDbContext&gt;())
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Of<T>()
        { 
            throw new NotImplementedException();
        }
    }
}