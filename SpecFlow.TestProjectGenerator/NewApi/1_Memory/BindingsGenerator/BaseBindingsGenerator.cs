namespace SpecFlow.TestProjectGenerator.NewApi._1_Memory.BindingsGenerator
{
    public abstract class BaseBindingsGenerator
    {
        public abstract ProjectFile GenerateBindingClassFile(string name, string fileContent);

        public abstract ProjectFile GenerateStepDefinition(string method);
    }
}
