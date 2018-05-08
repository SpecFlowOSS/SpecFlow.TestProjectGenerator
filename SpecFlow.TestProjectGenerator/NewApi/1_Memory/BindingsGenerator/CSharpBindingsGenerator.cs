using System;
using System.Collections.Generic;
using System.Linq;
using SpecFlow.TestProjectGenerator.Helpers;
using SpecFlow.TestProjectGenerator.NewApi.Driver;

namespace SpecFlow.TestProjectGenerator.NewApi._1_Memory.BindingsGenerator
{
    public class CSharpBindingsGenerator : BaseBindingsGenerator
    {
        private const string BindingsClassTemplate = @"
using System;
using System.IO;
using System.Xml;
using TechTalk.SpecFlow;

[Binding]
public class {0}
{{
    {1}
}}";

        private const string HookBindingsClassTemplate = @"
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using TechTalk.SpecFlow;

[Binding]
{0}
public class {1}
{{
    [{2}({3})]
    {4}
    public {5} void {6}()
    {{
        Console.WriteLine(""-> hook: {6}"");
        {7}
    }}   
}}
";

        public override ProjectFile GenerateBindingClassFile(string content)
        {
            return new ProjectFile($"BindingsClass_{Guid.NewGuid():N}.cs", "Compile", content);
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

        protected override string GetHookBindingClass(string eventType, string name, string code = "", int? order = null, IEnumerable<string> tags = null, bool useScopeTagsOnHookMethods = false, bool useScopeTagsOnClass = false)
        {
            bool isStatic = IsStaticEvent(eventType);

            var tagsArray = tags as string[] ?? tags?.ToArray() ?? new string[0];
            string eventTypeTags = string.Join(", ", tagsArray.Select(t => $@"""{t}"""));

            var eventTypeParams = new[]
            {
                eventTypeTags.Any() ? $"tags: new string[] {{{eventTypeTags}}}" : null,
                order is null ? null : $"Order = {order}"
            }.Where(p => p.IsNotNullOrWhiteSpace());

            string eventTypeParamsString = string.Join(", ", eventTypeParams);
            string scopeTags = $"[{string.Join(", ", tagsArray.Select(t => $@"Scope(Tag=""{t}"")"))}]";

            return string.Format(
                HookBindingsClassTemplate,
                useScopeTagsOnClass ? scopeTags : "",
                $"HooksClass_{Guid.NewGuid():N}",
                eventType,
                eventTypeParamsString,
                useScopeTagsOnHookMethods ? scopeTags : "",
                isStatic ? "static" : string.Empty,
                name,
                code);
        }
    }
}
