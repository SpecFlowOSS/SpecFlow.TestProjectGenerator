using TechTalk.SpecFlow.TestProjectGenerator.Inputs;

namespace TechTalk.SpecFlow.TestProjectGenerator.ProgramLanguageDrivers
{
    class VBNetProgramLanguageInputProjectDriver : ProgramLanguageInputProjectDriver
    {
        public override string GetBindingCode(string eventType, string code)
        {
            var staticKeyword = IsStaticEvent(eventType) ? "Shared" : "";
            return string.Format(@"<{0}> {1} Public Sub {0}() 
                                    Console.WriteLine(""BindingExecuted:{0}"")
                                    {2}
                                End Sub",
                                eventType,
                                staticKeyword,
                                code);
        }

        public override string GetProjectFileName(string projectName)
        {
            return $"{projectName}.vbproj";
        }

        public override BindingClassInput GetDefaultBindingClass()
        {
            return new BindingClassInput("DefaultBindings.vb");
        }
    }
}
