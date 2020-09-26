using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace UnitSharp.Http
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class InlineAutoDataAttribute : AutoDataInitializingTestDataSource
    {
        public InlineAutoDataAttribute(object firstData, params object[] moreData)
            : base(autoDataSource: new AutoDataAttribute(),
                   dominantDataSource: new DataRowAttribute(firstData, moreData))
        {
        }

        protected InlineAutoDataAttribute(
            AutoDataAttribute autoDataSource,
            object firstData,
            params object[] moreData)
            : base(autoDataSource,
                   dominantDataSource: new DataRowAttribute(firstData, moreData))
        {
        }
    }
}
