namespace TechTalk.SpecFlow.TestProjectGenerator.NewApi._1_Memory
{
    public class MSBuildTarget
    {
        public MSBuildTarget(string name, string implementation)
        {
            Name = name;
            Implementation = implementation;
        }

        public string Name { get; private set; }
        public string Implementation { get; private set; }
    }
}