using System;
using SpecFlow.TestProjectGenerator.Helpers;
using SpecFlow.TestProjectGenerator.NewApi.Driver;

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

        public override ProjectFile GenerateBindingClassFile(string name, string content)
        {
            return new ProjectFile(name, "Compile", content);
        }

        public override ProjectFile GenerateStepDefinition(string method)
        {
            string randomClassName = $"BindingsClass_{Guid.NewGuid():N}";
            string fileContent = string.Format(BindingsClassTemplate, randomClassName, method);
            return new ProjectFile($"{randomClassName}.cs", "Compile", fileContent);
        }

        protected override string GetBindingCode(string methodName, string methodImplementation, string attributeName, string regex, ParameterType parameterType, string argumentName)
        {
            string parameter = "";

            if (argumentName.IsNotNullOrWhiteSpace())
            {
                switch (parameterType)
                {
                    case ParameterType.Normal:
                        parameter = $"object {argumentName}";
                        break;
                    case ParameterType.Table:
                        parameter = $"Table {argumentName}";
                        break;
                    case ParameterType.DocString:
                        parameter = $"string {argumentName}";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(parameterType), parameterType, null);
                }
            }
            
            return $@"[{attributeName}(@""{regex}"")] public void {methodName}({parameter}) 
                                {{
                                    {methodImplementation}
                                }}";
        }
    }
}
