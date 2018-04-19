using SpecFlow.TestProjectGenerator.Inputs;

namespace SpecFlow.TestProjectGenerator.ProgramLanguageDrivers
{
    public class CSharpProgramLanguageInputProjectDriver : ProgramLanguageInputProjectDriver
    {
        public override string GetBindingCode(string eventType, string code)
        {
            var staticKeyword = IsStaticEvent(eventType) ? "static" : "";
            return $@"[{eventType}] {staticKeyword} public void {eventType}() 
                                {{
                                    Console.WriteLine(""BindingExecuted:{eventType}"");
                                    {code}
                                }}";
        }

        public override string GetProjectFileName(string projectName)
        {
            return $"{projectName}.csproj";
        }

        public override BindingClassInput GetDefaultBindingClass()
        {
            return new BindingClassInput("DefaultBindings.cs");
        }
    }
}