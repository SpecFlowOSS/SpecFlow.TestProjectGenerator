using System.Collections.Generic;

namespace SpecFlow.TestProjectGenerator.NewApi._1_Memory
{
    class Solution
    {
        public string Name { get; set; }
        public IReadOnlyList<Project> Projects { get; set; }
        public File NugetConfig { get; set; }
    }
}