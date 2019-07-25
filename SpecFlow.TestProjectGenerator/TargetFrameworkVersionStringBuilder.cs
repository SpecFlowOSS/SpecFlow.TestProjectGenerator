using System;
using System.Collections.Generic;
using System.Linq;
using TechTalk.SpecFlow.TestProjectGenerator.Data;

namespace TechTalk.SpecFlow.TestProjectGenerator
{
    public class TargetFrameworkVersionStringBuilder
    {
        private readonly IReadOnlyDictionary<TargetFramework, string> _targetFrameworkMonikerMappings = new Dictionary<TargetFramework, string>
        {
            [TargetFramework.Net35] = "v3.5",
            [TargetFramework.Net45] = "v4.5",
            [TargetFramework.Net452] = "v4.5.2",
        };

        public string BuildTargetFrameworkVersion(TargetFramework targetFramework)
        {
            var allTargetFrameworkMonikers = GetAllTargetFrameworkMonikers(Enumerable.Empty<string>(), targetFramework).ToArray();
            if (allTargetFrameworkMonikers.Length > 1)
            {
                throw new InvalidOperationException("The old project format only supports one target framework version.");
            }

            if (allTargetFrameworkMonikers.Length == 0)
            {
                throw new InvalidOperationException("Only .NET Framework target frameworks are supported.");
            }

            return string.Join(";", allTargetFrameworkMonikers);
        }

        internal IEnumerable<string> GetAllTargetFrameworkMonikers(IEnumerable<string> currentlyCollected, TargetFramework targetFramework)
        {
            var singleTargetFramework = _targetFrameworkMonikerMappings.Keys.FirstOrDefault(tf => (targetFramework & tf) == tf);
            if (singleTargetFramework is 0)
            {
                return currentlyCollected;
            }

            string targetFrameworkMoniker = _targetFrameworkMonikerMappings[singleTargetFramework];
            return GetAllTargetFrameworkMonikers(currentlyCollected.Append(targetFrameworkMoniker), targetFramework & ~singleTargetFramework);
        }
    }
}
