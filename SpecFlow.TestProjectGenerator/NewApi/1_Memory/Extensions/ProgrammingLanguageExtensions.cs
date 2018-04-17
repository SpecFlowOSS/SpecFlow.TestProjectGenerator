using System;

namespace SpecFlow.TestProjectGenerator.NewApi._1_Memory.Extensions
{
    public static class ProgrammingLanguageExtensions
    {

        public static string ToProjectFileExtension(this ProgrammingLanguage programmingLanguage)
            => programmingLanguage == ProgrammingLanguage.CSharp ? "csproj"
                : programmingLanguage == ProgrammingLanguage.FSharp ? "fsproj"
                : programmingLanguage == ProgrammingLanguage.VB ? "vbproj"
                : throw new NotSupportedException("There is no known project file extension for the programming language.");

        public static string ToCodeFileExtension(this ProgrammingLanguage programmingLanguage)
            => programmingLanguage == ProgrammingLanguage.CSharp ? "cs"
                : programmingLanguage == ProgrammingLanguage.FSharp ? "fs"
                : programmingLanguage == ProgrammingLanguage.VB ? "vb"
                : throw new NotSupportedException("There is no known file extension for the programming language.");
    }
}
