using NUnit.Framework;
using StraightInject.Core.Extensions;
using StraightInject.Core.Tests.Services;

namespace StraightInject.Core.Tests.Modules
{
    public class ModuleExtensionsTests
    {
        private IDependencyMapper _mapper;

        [SetUp]
        public void Setup()
        {
            _mapper = DefaultDependencyComposer.Initialize();
        }

        [Test]
        public void Compile_WithModuleUsage_Success()
        {
            Assert.DoesNotThrow(() =>
            {
                _mapper.FromModule<TestModule>();
                var container = _mapper.Compile();
                var dependentService = container.Resolve<IDependentService>();
                Assert.IsInstanceOf<DependentService>(dependentService);
            });
        }
    }

    public class TestModule : IModule
    {
        public void Apply(IDependencyMapper dependencyMapper)
        {
            dependencyMapper.FromType<DependentService>().ToService<IDependentService>();
            dependencyMapper.FromType<DependencyService>().ToService<IDependencyService>();
        }
    }
}