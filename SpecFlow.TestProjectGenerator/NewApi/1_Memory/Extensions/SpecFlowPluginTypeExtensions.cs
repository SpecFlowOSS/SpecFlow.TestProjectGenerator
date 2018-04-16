using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecFlow.TestProjectGenerator.NewApi._1_Memory.Extensions
{
    public static class SpecFlowPluginTypeExtensions
    {
        public static string ToPluginTypeString(this SpecFlowPluginType pluginType)
        {
            switch (pluginType)
            {
                case SpecFlowPluginType.Generator | SpecFlowPluginType.Runtime:
                    return "GeneratorAndRuntime";
                case SpecFlowPluginType.Generator:
                    return "Generator";
                case SpecFlowPluginType.Runtime:
                    return "Runtime";
                default:
                    throw new ArgumentOutOfRangeException(nameof(pluginType));
            }
        }
    }
}
