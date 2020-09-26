using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UnitSharp.Http
{
    public abstract class AutoDataInitializingTestDataSource : Attribute, ITestDataSource
    {
        private readonly AutoDataAttribute _autoDataSource;
        private readonly ITestDataSource _dominantDataSource;

        protected AutoDataInitializingTestDataSource(
            AutoDataAttribute autoDataSource,
            ITestDataSource dominantDataSource)
        {
            _autoDataSource = autoDataSource;
            _dominantDataSource = dominantDataSource;
        }

        public IEnumerable<object[]> GetData(MethodInfo methodInfo)
        {
            foreach (object[] dominantData in _dominantDataSource.GetData(methodInfo))
            {
                object[] arguments = InitializeArguments(methodInfo);
                OverrideArguments(arguments, dominantData);
                yield return arguments;
            }
        }

        private object[] InitializeArguments(MethodInfo methodInfo)
        {
            object[] arguments = new object[methodInfo.GetParameters().Length];
            object[] autoData = _autoDataSource.GetData(methodInfo).Single();
            CopyArray(destination: arguments, source: autoData);
            return arguments;
        }

        private static void OverrideArguments(object[] arguments, object[] dominantData)
        {
            CopyArray(destination: arguments, source: dominantData);
        }

        private static void CopyArray(object[] destination, object[] source)
        {
            Array.Copy(source, sourceIndex: 0, destination, destinationIndex: 0, source.Length);
        }

        public string GetDisplayName(MethodInfo methodInfo, object[] data)
            => TestDataSource.GetDisplayName(methodInfo, data);
    }
}
