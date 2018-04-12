namespace SpecFlow.TestProjectGenerator.NewApi._1_Memory
{
    public class SpecFlowPlugin
    {
        private string v;
        private string v1;
        private string v2;

        public SpecFlowPlugin(string v)
        {
            this.v = v;
        }

        public SpecFlowPlugin(string v1, string v2)
        {
            this.v1 = v1;
            this.v2 = v2;
        }

        public string Name { get; set; }
        public string Path { get; set; }
    }
}