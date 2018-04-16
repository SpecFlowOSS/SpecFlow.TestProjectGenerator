namespace SpecFlow.TestProjectGenerator.NewApi._1_Memory
{
    public abstract class BaseBindingsGenerator
    {
        public abstract ProjectFile GenerateBindingClass(string name, string content);

        public abstract ProjectFile GenerateBindingMethod(string method);
    }
}
