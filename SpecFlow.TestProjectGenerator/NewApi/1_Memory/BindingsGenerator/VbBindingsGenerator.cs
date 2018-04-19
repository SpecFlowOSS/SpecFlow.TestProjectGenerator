using System;

namespace SpecFlow.TestProjectGenerator.NewApi._1_Memory.BindingsGenerator
{
    public class VbBindingsGenerator : BaseBindingsGenerator
    {
        private const string BindingsClassTemplate = @"
Imports TechTalk.SpecFlow

<Binding> _
Public Class {0}
    {1}
End Class";

        public override ProjectFile GenerateBindingClassFile(string name, string content)
        {
            return new ProjectFile(name, "Compile", content);
        }

        public override ProjectFile GenerateStepDefinition(string method)
        {
            string randomClassName = $"BindingsClass_{Guid.NewGuid():N}";
            return new ProjectFile($"{randomClassName}.cs", "Compile", string.Format(BindingsClassTemplate, randomClassName, method));
        }
    }
}
