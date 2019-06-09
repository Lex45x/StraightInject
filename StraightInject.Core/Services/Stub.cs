using System;

namespace StraightInject.Core.Services
{
    public static class Stub
    {
        public static T Of<T>()
        {
            throw new NotImplementedException();
        }
    }
}