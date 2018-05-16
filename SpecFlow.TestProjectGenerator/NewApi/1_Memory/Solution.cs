using System;
using System.Collections.Generic;

namespace TechTalk.SpecFlow.TestProjectGenerator.NewApi._1_Memory
{
    public class Solution
    {
        private readonly List<Project> _projects = new List<Project>();

        public Solution(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public IReadOnlyList<Project> Projects => _projects;

        public ProjectFile NugetConfig { get; set; }

        public void AddProject(Project project)
        {
            _projects.Add(project ?? throw new ArgumentNullException(nameof(project)));
        }
    }
}