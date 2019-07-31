using System.Collections.Generic;
using System.Linq;
using TechTalk.SpecFlow.TestProjectGenerator.Data;

namespace TechTalk.SpecFlow.TestProjectGenerator
{
    public class TargetFrameworkMonikerStringBuilder
    {
        private readonly TargetFrameworkSplitter _targetFrameworkSplitter;
        private readonly IReadOnlyDictionary<TargetFramework, string> _targetFrameworkMonikerMappings = new Dictionary<TargetFramework, string>
        {
            [TargetFramework.Net35] = "net35",
            [TargetFramework.Net45] = "net45",
            [TargetFramework.Net452] = "net452",
            [TargetFramework.NetStandard20] = "netstandard2.0",
            [TargetFramework.Netcoreapp20] = "netcoreapp2.0",
            [TargetFramework.Netcoreapp21] = "netcoreapp2.1",
            [TargetFramework.Netcoreapp22] = "netcoreapp2.2",
            [TargetFramework.Netcoreapp30] = "netcoreapp3.0"
        };

        public TargetFrameworkMonikerStringBuilder(TargetFrameworkSplitter targetFrameworkSplitter)
        {
            _targetFrameworkSplitter = targetFrameworkSplitter;
        }

        public string BuildTargetFrameworkMoniker(TargetFramework targetFramework)
        {
            var allTargetFrameworkMonikers = GetAllTargetFrameworkMonikers(targetFramework);
            return string.Join(";", allTargetFrameworkMonikers);
        }

        public IEnumerable<string> GetAllTargetFrameworkMonikers(TargetFramework targetFramework)
        {
            var allTargetFrameworkValues = _targetFrameworkSplitter.GetAllTargetFrameworkValues(targetFramework);
            return GetAllTargetFrameworkMonikers(Enumerable.Empty<string>(), allTargetFrameworkValues);
        }

        internal IEnumerable<string> GetAllTargetFrameworkMonikers(IEnumerable<string> currentlyCollected, IEnumerable<TargetFramework> targetFrameworks)
        {
            if (!targetFrameworks.Any())
            {
                return currentlyCollected;
            }

            string targetFrameworkMoniker = _targetFrameworkMonikerMappings[targetFrameworks.First()];
            return GetAllTargetFrameworkMonikers(currentlyCollected.Append(targetFrameworkMoniker), targetFrameworks.Skip(1));
        }
    }
}
