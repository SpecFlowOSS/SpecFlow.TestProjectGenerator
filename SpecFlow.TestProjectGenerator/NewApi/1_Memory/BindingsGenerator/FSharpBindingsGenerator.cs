using System;
using System.Collections.Generic;
using TechTalk.SpecFlow.TestProjectGenerator.NewApi.Driver;

namespace TechTalk.SpecFlow.TestProjectGenerator.NewApi._1_Memory.BindingsGenerator
{
    public class FSharpBindingsGenerator : BaseBindingsGenerator
    {
        private const string BindingsClassTemplate = @"
namespace Bindings
open TechTalk.SpecFlow

[<Binding>]
type {0}() =
    {1}";

        public override ProjectFile GenerateBindingClassFile(string content)
        {
            return new ProjectFile($"BindingsClass_{Guid.NewGuid():N}.fs", "Compile", content);
        }

        public override ProjectFile GenerateStepDefinition(string method)
        {
            string randomClassName = $"BindingsClass_{Guid.NewGuid():N}";
            return new ProjectFile($"{randomClassName}.fs", "Compile", string.Format(BindingsClassTemplate, randomClassName, method));
        }

        public override ProjectFile GenerateLoggerClass(string pathToLogFile)
        {
            throw new NotImplementedException();
        }

        protected override string GetBindingCode(string methodName, string methodImplementation, string attributeName, string regex, ParameterType parameterType, string argumentName)
        {
            throw new NotImplementedException();
        }

        protected override string GetLoggingStepDefinitionCode(string methodName, string attributeName, string regex, ParameterType parameterType, string argumentName)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }
    }
}
