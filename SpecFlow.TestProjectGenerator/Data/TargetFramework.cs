using System;

namespace TechTalk.SpecFlow.TestProjectGenerator.Data
{
    [Flags]
    public enum TargetFramework
    {
        Net45 = 0b00_00_00_01,
        NetStandard20 = 0b00_00_00_10,
        Netcoreapp20 = 0b00_00_01_00,
        Net452 = 0b00_00_10_00,
        Net35 = 0b00_01_00_00,
        Netcoreapp21 = 0b00_10_00_00,
        Netcoreapp22 = 0b01_00_00_00,
    }
}
