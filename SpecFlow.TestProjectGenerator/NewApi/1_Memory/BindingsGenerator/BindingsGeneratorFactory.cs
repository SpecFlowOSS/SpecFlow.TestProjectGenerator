using System;

namespace TechTalk.SpecFlow.TestProjectGenerator.NewApi._1_Memory.BindingsGenerator
{
    public class BindingsGeneratorFactory
    {
        public BaseBindingsGenerator FromLanguage(ProgrammingLanguage targetLanguage)
        {
            switch (targetLanguage)
            {
                case ProgrammingLanguage.CSharp: return new CSharpBindingsGenerator();
                case ProgrammingLanguage.FSharp: return new FSharpBindingsGenerator();
                case ProgrammingLanguage.VB: return new VbBindingsGenerator();
                default: throw new ArgumentException(
                        $"Target language generator not defined for {targetLanguage}.",
                        nameof(targetLanguage));
            }
        }
    }
}
