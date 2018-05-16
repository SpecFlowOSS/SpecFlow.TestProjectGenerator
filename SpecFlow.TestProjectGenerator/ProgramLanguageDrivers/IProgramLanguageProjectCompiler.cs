using Microsoft.Build.Evaluation;
using TechTalk.SpecFlow.TestProjectGenerator.Inputs;

namespace TechTalk.SpecFlow.TestProjectGenerator.ProgramLanguageDrivers
{
    public interface IProgramLanguageProjectCompiler
    {
        void AddBindingClass(InputProjectDriver inputProjectDriver, Project project, BindingClassInput bindingClassInput);
        string FileEnding { get; }
        string ProjectFileName { get; }
        void AdditionalAdjustments(Project project, InputProjectDriver inputProjectDriver);
    }
}