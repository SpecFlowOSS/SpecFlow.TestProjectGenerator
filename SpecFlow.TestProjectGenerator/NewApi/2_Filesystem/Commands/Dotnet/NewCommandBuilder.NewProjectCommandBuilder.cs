using System;
using System.IO;

namespace SpecFlow.TestProjectGenerator.NewApi._2_Filesystem.Commands.Dotnet
{
    public partial class NewCommandBuilder
    {
        public class NewProjectCommandBuilder : BaseCommandBuilder
        {
            private readonly NewCommandBuilder _newCommandBuilder;
            private string _templateName = "classlib";
            private string _name = "ClassLib";
            private string _folder;
            private ProgrammingLanguage _language = ProgrammingLanguage.CSharp;

            public NewProjectCommandBuilder(NewCommandBuilder newCommandBuilder)
            {
                _newCommandBuilder = newCommandBuilder;
            }

            public NewProjectCommandBuilder UsingTemplate(string templateName)
            {
                _templateName = templateName;
                return this;
            }

            public NewProjectCommandBuilder WithName(string name)
            {
                _name = name;
                return this;
            }

            public NewProjectCommandBuilder InFolder(string folder)
            {
                _folder = folder;
                return this;
            }

            public NewProjectCommandBuilder WithLanguage(ProgrammingLanguage language)
            {
                _language = language;
                return this;
            }

            protected override string BuildArguments()
            {
                var arguments = AddArgument($"new {_templateName}", "-o", _folder);
                arguments = AddArgument(
                    arguments,
                    "-lang",
                    _language == ProgrammingLanguage.CSharp ? "\"C#\"" :
                    _language == ProgrammingLanguage.VB ? "VB" :
                    _language == ProgrammingLanguage.FSharp ? "\"F#\"" : string.Empty);

                return arguments;
            }
        }
    }

}
