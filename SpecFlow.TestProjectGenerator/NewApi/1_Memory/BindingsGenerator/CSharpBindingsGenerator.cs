using System;

namespace SpecFlow.TestProjectGenerator.NewApi._1_Memory.BindingsGenerator
{
    public class CSharpBindingsGenerator : BaseBindingsGenerator
    {
        private const string BindingsClassTemplate = @"
using TechTalk.SpecFlow;

[Binding]
public class {0}
{{
    {1}
}}";

        public override ProjectFile GenerateBindingClass(string name, string content)
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
