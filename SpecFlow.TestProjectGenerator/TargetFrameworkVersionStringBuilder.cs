using System;
using System.Collections.Generic;
using System.Linq;
using TechTalk.SpecFlow.TestProjectGenerator.Data;

namespace TechTalk.SpecFlow.TestProjectGenerator
{
    public class TargetFrameworkVersionStringBuilder
    {
        private readonly TargetFrameworkSplitter _targetFrameworkSplitter;
        private readonly IReadOnlyDictionary<TargetFramework, string> _targetFrameworkMonikerMappings = new Dictionary<TargetFramework, string>
        {
            [TargetFramework.Net35] = "v3.5",
            [TargetFramework.Net45] = "v4.5",
            [TargetFramework.Net452] = "v4.5.2",
        };

        public TargetFrameworkVersionStringBuilder(TargetFrameworkSplitter targetFrameworkSplitter)
        {
            _targetFrameworkSplitter = targetFrameworkSplitter;
        }

        public string BuildTargetFrameworkVersion(TargetFramework targetFramework)
        {
            var targetFrameworks = _targetFrameworkSplitter.GetAllTargetFrameworkValues(targetFramework).ToArray();
            if (targetFrameworks.Length > 1)
            {
                throw new InvalidOperationException("Multiple target frameworks don't work with the old csproj format");
            }

            var allTargetFrameworkMonikers = GetAllTargetFrameworkMonikers(Enumerable.Empty<string>(), targetFrameworks).ToArray();

            if (allTargetFrameworkMonikers.Length == 0)
            {
                throw new InvalidOperationException("At least one valid target framework must be specified.");
            }

            return string.Join(";", allTargetFrameworkMonikers);
        }

        internal IEnumerable<string> GetAllTargetFrameworkMonikers(IEnumerable<string> currentlyCollected, IEnumerable<TargetFramework> targetFrameworks)
        {
            var currentTargetFramework = targetFrameworks.FirstOrDefault();
            if (currentTargetFramework is 0)
            {
                return currentlyCollected;
            }

            var singleTargetFramework = _targetFrameworkMonikerMappings.Keys.FirstOrDefault(tf => (currentTargetFramework & tf) == tf);

            if (singleTargetFramework is 0)
            {
                throw new InvalidOperationException("Only .NET Framework target frameworks are supported.");
            }

            string targetFrameworkMoniker = _targetFrameworkMonikerMappings[singleTargetFramework];
            return GetAllTargetFrameworkMonikers(currentlyCollected.Append(targetFrameworkMoniker), targetFrameworks.Skip(1));
        }
    }
}
