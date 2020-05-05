using System.Collections.Generic;
using TechTalk.SpecFlow.TestProjectGenerator.Data;

namespace TechTalk.SpecFlow.TestProjectGenerator.FilesystemWriter
{
    public class NetCoreSdkInfoProvider
    {
        private static readonly NetCoreSdkInfo NetCore31 = new NetCoreSdkInfo("3.1.200");
        private static readonly NetCoreSdkInfo NetCore30 = new NetCoreSdkInfo("3.0.101");
        private static readonly NetCoreSdkInfo NetCore22 = new NetCoreSdkInfo("2.2.402");
        private static readonly NetCoreSdkInfo NetCore21 = new NetCoreSdkInfo("2.1.802");
        private static readonly NetCoreSdkInfo NetCore20 = new NetCoreSdkInfo("2.1.202");

        private readonly IReadOnlyDictionary<TargetFramework, NetCoreSdkInfo> _sdkMappings = new Dictionary<TargetFramework, NetCoreSdkInfo>
        {
            [TargetFramework.Netcoreapp31] = NetCore31,
            [TargetFramework.Netcoreapp30] = NetCore30,
            [TargetFramework.Netcoreapp22] = NetCore22,
            [TargetFramework.Netcoreapp21] = NetCore21,
            [TargetFramework.Netcoreapp20] = NetCore20,
            [TargetFramework.NetStandard20] = NetCore22
        };

        public NetCoreSdkInfo GetSdkFromTargetFramework(TargetFramework targetFramework)
        {
            if (_sdkMappings.TryGetValue(targetFramework, out var netCoreSdkInfo))
            {
                return netCoreSdkInfo;
            }

            return null;
        }
    }
}
