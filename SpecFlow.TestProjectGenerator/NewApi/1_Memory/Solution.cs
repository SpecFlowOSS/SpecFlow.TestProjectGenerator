using System.Collections.Generic;

namespace SpecFlow.TestProjectGenerator.NewApi._1_Memory
{
    public class Solution
    {
        private string v;

        public Solution(string v)
        {
            this.v = v;
        }

        public string Name { get; set; }
        public IReadOnlyList<Project> Projects { get; set; }
        public ProjectFile NugetConfig { get; set; }

        public void AddProject(Project project)
        {
            throw new System.NotImplementedException();
        }
    }
}