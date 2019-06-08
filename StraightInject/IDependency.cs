using System;

namespace StraightInject
{
    public interface IDependency
    {
    }

    public interface IDependency<TOriginal> : IDependency
    {
    }
}