using System;
using System.Collections.Generic;
using System.Linq;
using TechTalk.SpecFlow.TestProjectGenerator.Data;

namespace TechTalk.SpecFlow.TestProjectGenerator
{
    public class TargetFrameworkSplitter
    {
        private readonly IReadOnlyCollection<TargetFramework> _availableTargetFrameworks;

        public TargetFrameworkSplitter()
        {
            _availableTargetFrameworks = Enum.GetValues(typeof(TargetFramework)).Cast<TargetFramework>().ToArray();
        }

        public IEnumerable<TargetFramework> GetAllTargetFrameworkValues(TargetFramework targetFramework)
        {
            return GetAllTargetFrameworkValues(Enumerable.Empty<TargetFramework>(), targetFramework);
        }

        internal IEnumerable<TargetFramework> GetAllTargetFrameworkValues(IEnumerable<TargetFramework> currentlyCollected, TargetFramework remainingTargetFrameworks)
        {
            if (remainingTargetFrameworks is 0)
            {
                return currentlyCollected;
            }

            var singleTargetFramework = _availableTargetFrameworks.FirstOrDefault(tf => (remainingTargetFrameworks & tf) == tf);
            if (singleTargetFramework is 0) // invalid target framework - no match found
            {
                throw new InvalidOperationException($"Target framework {remainingTargetFrameworks} is not supported");
            }

            var nextRemainingTargetFrameworks = remainingTargetFrameworks & ~singleTargetFramework;

            return GetAllTargetFrameworkValues(currentlyCollected.Append(singleTargetFramework), nextRemainingTargetFrameworks);
        }
    }
}
