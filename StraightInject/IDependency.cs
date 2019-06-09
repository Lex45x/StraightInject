using System;

namespace StraightInject
{
    public interface IService
    {
    }

    public interface IService<TOriginal> : IService
    {
    }
}