using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UnitSharp.Http
{
    internal static class TestDataSource
    {
        public static string GetDisplayName(MethodInfo methodInfo, object[] data)
        {
            ParameterInfo[] parameters = methodInfo.GetParameters();
            IEnumerable<string> values =
                from t in parameters.Zip(data, (p, a) => (parameter: p, argument: a))
                select DumpArgument(t.parameter, t.argument);
            return string.Join(", ", values);
        }

        private static string DumpArgument(ParameterInfo parameter, object argument)
        {
            return $"{parameter.Name}: {argument?.ToString() ?? "null"}";
        }
    }
}
