using System;

namespace SpecFlow.TestProjectGenerator.NewApi._1_Memory
{
    [Flags]
    public enum TargetFramework
    {
        Net45 = 0b01,
        NetStandard20 = 0b10,
        Netcoreapp20 = 0b01_00
    }
}
