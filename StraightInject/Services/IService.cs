using System;

namespace StraightInject.Services
{
    public interface IService
    {
        Type ServiceType { get; }
    }
}