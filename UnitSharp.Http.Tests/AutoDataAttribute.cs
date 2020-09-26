using AutoFixture;
using AutoFixture.Kernel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace UnitSharp.Http
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class AutoDataAttribute : Attribute, ITestDataSource
    {
        private readonly Lazy<IFixture> _fixture;

        public AutoDataAttribute()
            : this(() => new Fixture { Behaviors = { new OmitOnRecursionBehavior() } })
        {
        }

        public AutoDataAttribute(ICustomization customization)
            : this(() => new Fixture
            {
                Behaviors =
                {
                    new OmitOnRecursionBehavior(),
                },
            }.Customize(customization))
        {
        }

        protected AutoDataAttribute(Func<IFixture> factory)
        {
            _fixture = new Lazy<IFixture>(factory, LazyThreadSafetyMode.PublicationOnly);
        }

        public IEnumerable<object[]> GetData(MethodInfo methodInfo)
        {
            var arguments = new List<object>();
            foreach (ParameterInfo parameter in methodInfo.GetParameters())
            {
                CustomizeFixture(parameter);
                arguments.Add(Resolve(parameter));
            }

            yield return arguments.ToArray();
        }

        private void CustomizeFixture(ParameterInfo parameter)
        {
            foreach (Attribute attribute in parameter.GetCustomAttributes())
            {
                switch (attribute)
                {
                    case IParameterCustomizationSource source:
                        IFixture fixture = _fixture.Value;
                        ICustomization customization = source.GetCustomization(parameter);
                        fixture.Customize(customization);
                        break;
                }
            }
        }

        private object Resolve(ParameterInfo parameter)
        {
            return new SpecimenContext(_fixture.Value).Resolve(parameter);
        }

        public string GetDisplayName(MethodInfo methodInfo, object[] data)
            => TestDataSource.GetDisplayName(methodInfo, data);
    }
}
