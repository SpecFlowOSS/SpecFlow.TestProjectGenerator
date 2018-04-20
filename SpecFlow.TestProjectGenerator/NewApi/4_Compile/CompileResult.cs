namespace SpecFlow.TestProjectGenerator.NewApi._4_Compile
{
    public class CompileResult
    {
        public CompileResult(bool successful, string output)
        {
            Successful = successful;
            Output = output;
        }

        public bool Successful { get; }
        public string Output { get; }
    }
}