using System;

namespace StraightInject.Services
{
    /// <summary>
    /// Disclaim a specific service with can act as a dependency
    /// </summary>
    public interface IService
    {
        Type ServiceType { get; }
    }
}