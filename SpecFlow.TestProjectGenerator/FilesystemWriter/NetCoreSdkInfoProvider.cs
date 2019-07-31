using System;
using System.Collections.Generic;

namespace TechTalk.SpecFlow.TestProjectGenerator.FilesystemWriter
{
    public class NetCoreSdkInfoProvider
    {
        private static readonly NetCoreSdkInfo NetCore30Preview = new NetCoreSdkInfo("3.0.100-preview");
        private static readonly NetCoreSdkInfo NetCore22 = new NetCoreSdkInfo("2.2.401");
        private static readonly NetCoreSdkInfo NetCore21 = new NetCoreSdkInfo("2.1.801");
        private static readonly NetCoreSdkInfo NetCore20 = new NetCoreSdkInfo("2.1.202");

        private readonly IReadOnlyDictionary<string, NetCoreSdkInfo> _sdkMappings = new Dictionary<string, NetCoreSdkInfo>
        {
            ["netcoreapp3.0"] = NetCore30Preview,
            ["netcoreapp2.2"] = NetCore22,
            ["netcoreapp2.1"] = NetCore21,
            ["netcoreapp2.0"] = NetCore20,
            ["netstandard2.0"] = NetCore22,
            ["net35"] = NetCore22,
            ["net45"] = NetCore22,
            ["net452"] = NetCore22,
            ["net471"] = NetCore22
        };

        public NetCoreSdkInfo GetSdkFromTargetFramework(string targetFramework)
        {
            if (_sdkMappings.TryGetValue(targetFramework, out var netCoreSdkInfo))
            {
                return netCoreSdkInfo;
            }

            throw new NotSupportedException($"Target framework {targetFramework} does not have an associated SDK");
        }
    }
}
