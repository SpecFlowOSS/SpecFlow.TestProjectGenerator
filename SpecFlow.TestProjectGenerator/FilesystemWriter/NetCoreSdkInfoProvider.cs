using System.Collections.Generic;
using TechTalk.SpecFlow.TestProjectGenerator.Data;

namespace TechTalk.SpecFlow.TestProjectGenerator.FilesystemWriter
{
    public class NetCoreSdkInfoProvider
    {
        private static readonly NetCoreSdkInfo NetCore30Preview = new NetCoreSdkInfo("3.0.100-preview");
        private static readonly NetCoreSdkInfo NetCore22 = new NetCoreSdkInfo("2.2.401");
        private static readonly NetCoreSdkInfo NetCore21 = new NetCoreSdkInfo("2.1.801");
        private static readonly NetCoreSdkInfo NetCore20 = new NetCoreSdkInfo("2.1.202");

        private readonly IReadOnlyDictionary<TargetFramework, NetCoreSdkInfo> _sdkMappings = new Dictionary<TargetFramework, NetCoreSdkInfo>
        {
            [TargetFramework.Netcoreapp30] = NetCore30Preview,
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
