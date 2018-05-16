using System.Text;
using Microsoft.Build.Evaluation;
using TechTalk.SpecFlow.TestProjectGenerator.Inputs;

namespace TechTalk.SpecFlow.TestProjectGenerator.ProgramLanguageDrivers
{
    class CSharpProgramLanguageProjectCompiler : ProgramLanguageProjectCompiler
    {
        private readonly CurrentVersionDriver _currentVersionDriver;

        public CSharpProgramLanguageProjectCompiler(ProjectCompilerHelper projectCompilerHelper, CurrentVersionDriver currentVersionDriver) : base(projectCompilerHelper)
        {
            _currentVersionDriver = currentVersionDriver;
        }

        public override string FileEnding => ".cs";
        public override string ProjectFileName
        {
            get
            {
                if (_currentVersionDriver.SpecFlowMajor < 2)
                {
                    return "TestProjectFile_Before20.csproj";
                }
                return "TestProjectFile.csproj";
            }
        }

        public override void AdditionalAdjustments(Project project, InputProjectDriver inputProjectDriver)
        {
            
        }

        protected override string BindingClassFileName => "BindingClass.cs";

        protected override string GetBindingsCode(BindingClassInput bindingClassInput)
        {
            StringBuilder result = new StringBuilder();

            int counter = 0;

            foreach (var stepBindingInput in bindingClassInput.StepBindings)
            {
                result.AppendFormat(@"[{2}(@""{3}"")]public void sb{0}() {{ 
                                        {1}
                                      }}", ++counter, stepBindingInput.CSharpCode, stepBindingInput.ScenarioBlock, stepBindingInput.Regex);
                result.AppendLine();
            }

            foreach (var otherBinding in bindingClassInput.OtherBindings)
            {
                result.AppendLine(otherBinding);
            }

            return result.ToString();
        }
    }
}