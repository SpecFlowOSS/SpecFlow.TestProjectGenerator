using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SpecFlow.TestProjectGenerator.NewApi._1_Memory
{
    public class Solution
    {
        private readonly List<Project> _projects = new List<Project>();

        public Solution(string name)
        {
            Name = name;
            Projects = new ReadOnlyCollection<Project>(_projects);
        }

        public string Name { get; set; }
        public IReadOnlyList<Project> Projects { get; } 
        public ProjectFile NugetConfig { get; set; } //TODO tests & add to solutionwriter

        public void AddProject(Project project)
        {
            _projects.Add(project ?? throw new ArgumentNullException(nameof(project)));
        }
    }
}