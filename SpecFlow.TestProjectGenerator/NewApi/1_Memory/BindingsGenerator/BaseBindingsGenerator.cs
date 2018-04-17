namespace SpecFlow.TestProjectGenerator.NewApi._1_Memory.BindingsGenerator
{
    public abstract class BaseBindingsGenerator
    {
        public abstract ProjectFile GenerateBindingClass(string name, string content);

        public abstract ProjectFile GenerateStepDefinition(string method);
    }
}
