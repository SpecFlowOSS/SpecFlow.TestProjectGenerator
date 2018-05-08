using System;
using System.Collections.Generic;
using System.Linq;
using SpecFlow.TestProjectGenerator.Helpers;
using SpecFlow.TestProjectGenerator.NewApi.Driver;

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

        private const string HookBindingsClassTemplate = @"
Imports System;
Imports System.Collections;
Imports System.IO;
Imports System.Linq;
Imports System.Xml;
Imports System.Xml.Linq;
Imports TechTalk.SpecFlow;

<Binding> _
{0}
Public Class {1}
    <{2}({3})>_
    {4}
    Public {5} Sub {6}()
        Console.WriteLine(""-> hook: {6}"");
        {7}
    End Sub
End Class
";

        public override ProjectFile GenerateBindingClassFile(string content)
        {
            return new ProjectFile($"BindingsClass_{Guid.NewGuid():N}.vb", "Compile", content);
        }

        public override ProjectFile GenerateStepDefinition(string method)
        {
            string randomClassName = $"BindingsClass_{Guid.NewGuid():N}";
            return new ProjectFile($"{randomClassName}.vb", "Compile", string.Format(BindingsClassTemplate, randomClassName, method));
        }

        protected override string GetBindingCode(string methodName, string methodImplementation, string attributeName, string regex, ParameterType parameterType, string argumentName)
        {
            string parameter = "";

            if (argumentName.IsNullOrWhiteSpace())
            {
                switch (parameterType)
                {
                    case ParameterType.Normal:
                        parameter = $"{argumentName} As Object";
                        break;
                    case ParameterType.Table:
                        parameter = $"{argumentName} As Table";
                        break;
                    case ParameterType.DocString:
                        parameter = $"{argumentName} As String";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(parameterType), parameterType, null);
                }
            }

            return $@"<{attributeName}(@""{regex}"")> Public Sub {methodName}({parameter}) 
                                
                                    {methodImplementation}
                                End Sub";
        }

        protected override string GetHookBindingClass(string eventType, string name, string code = "", int? order = null, IEnumerable<string> tags = null, bool useScopeTagsOnHookMethods = false, bool useScopeTagsOnClass = false)
        {
            bool isStatic = IsStaticEvent(eventType);

            var tagsArray = tags as string[] ?? tags?.ToArray() ?? new string[0];
            string eventTypeTags = string.Join(", ", tagsArray.Select(t => $@"""{t}"""));

            var eventTypeParams = new[]
            {
                useScopeTagsOnHookMethods ? null : $"tags:= New String() {{{eventTypeTags}}}",
                order is null ? null : $"Order:= {order}"
            }.Where(p => p.IsNotNullOrWhiteSpace());

            string eventTypeParamsString = string.Join(", ", eventTypeParams);
            string scopeTags = $"[{string.Join(", ", tagsArray.Select(t => $@"Scope(Tag:=""{t}"")"))}]";

            return string.Format(
                HookBindingsClassTemplate,
                useScopeTagsOnClass ? scopeTags : "",
                Guid.NewGuid(),
                eventType,
                eventTypeParamsString,
                useScopeTagsOnHookMethods ? scopeTags : "",
                isStatic ? "Static" : string.Empty,
                name,
                code);
        }
    }
}
