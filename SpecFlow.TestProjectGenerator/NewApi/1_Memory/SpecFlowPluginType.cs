using System;

namespace SpecFlow.TestProjectGenerator.NewApi._1_Memory
{
    [Flags]
    public enum SpecFlowPluginType
    {
        Generator = 0b01,
        Runtime = 0b10
    }
}
