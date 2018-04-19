using SpecFlow.TestProjectGenerator.NewApi.Driver;

namespace SpecFlow.TestProjectGenerator.NewApi._1_Memory.BindingsGenerator
{
    public abstract class BaseBindingsGenerator
    {
        public abstract ProjectFile GenerateBindingClassFile(string name, string fileContent);

        public abstract ProjectFile GenerateStepDefinition(string method);

        

        public ProjectFile GenerateStepDefinition(string methodName, string methodImplementation, string attributeName, string regex, ParameterType parameterType = ParameterType.Normal, string argumentName = null)
        {
            var method = GetBindingCode(methodName, methodImplementation, attributeName, regex, parameterType, argumentName);

            return GenerateStepDefinition(method);
        }

        protected abstract string GetBindingCode(string methodName, string methodImplementation, string attributeName, string regex, ParameterType parameterType, string argumentName);

        protected bool IsStaticEvent(string eventType)
        {
            return eventType == "BeforeFeature" || eventType == "AfterFeature" || eventType == "BeforeTestRun" || eventType == "AfterTestRun";
        }
    }
}
