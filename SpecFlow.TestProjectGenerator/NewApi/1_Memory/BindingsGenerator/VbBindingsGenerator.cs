using System;
using System.Collections.Generic;
using System.Linq;
using SpecFlow.TestProjectGenerator.Helpers;
using SpecFlow.TestProjectGenerator.NewApi.Driver;
using SpecFlow.TestProjectGenerator.NewApi._1_Memory.Extensions;

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

            if (argumentName.IsNotNullOrWhiteSpace())
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

            return $@"<[{attributeName}](""{regex}"")> Public Sub {methodName}({parameter}) 
                                
                                    {methodImplementation}
                                End Sub";
        }

        protected override string GetHookBindingClass(
            string hookType,
            string name,
            string code = "",
            int? order = null,
            IList<string> hookTypeAttributeTags = null,
            IList<string> methodScopeAttributeTags = null,
            IList<string> classScopeAttributeTags = null)
        {
            string ToScopeTags(IList<string> scopeTags) => scopeTags.Any() ? $"{scopeTags.Select(t => $@"<[Scope](Tag=""{t}"")>").JoinToString("")}_" : null;

            bool isStatic = IsStaticEvent(hookType);
            
            string hookTypeTags = hookTypeAttributeTags?.Select(t => $@"""{t}""").JoinToString(", ");

            var hookAttributeConstructorProperties = new[]
            {
                hookTypeAttributeTags is null || !hookTypeAttributeTags.Any() ? null : $"tags:= New String() {{{hookTypeTags}}}",
                order is null ? null : $"Order:= {order}"
            }.Where(p => p.IsNotNullOrWhiteSpace());

            string hookTypeAttributeTagsString = string.Join(", ", hookAttributeConstructorProperties);
            string classScopeAttributes = ToScopeTags(classScopeAttributeTags);
            string methodScopeAttributes = ToScopeTags(methodScopeAttributeTags);

            string staticKeyword = isStatic ? "Static" : string.Empty;
            return $@"
Imports System
Imports System.Collections
Imports System.IO
Imports System.Linq
Imports System.Xml
Imports System.Xml.Linq
Imports TechTalk.SpecFlow

<[Binding]> _
{classScopeAttributes}
Public Class {Guid.NewGuid()}
    <[{hookType}({hookTypeAttributeTagsString})]>_
    {methodScopeAttributes}
    Public {staticKeyword} Sub {name}()
        Console.WriteLine(""-> hook: {name}"")
        {code}
    End Sub
End Class
";
        }
    }
}
