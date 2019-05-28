﻿namespace StraightInject.Core.Tests.Services
{
    public interface IDependentService
    {
    }

    public class DependentService : IDependentService
    {
        private readonly IDependencyService dependency;

        public DependentService(IDependencyService dependency)
        {
            this.dependency = dependency;
        }
    }
}