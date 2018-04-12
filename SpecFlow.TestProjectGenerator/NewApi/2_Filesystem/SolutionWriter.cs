using SpecFlow.TestProjectGenerator.NewApi._1_Memory;

namespace SpecFlow.TestProjectGenerator.NewApi._2_Filesystem
{
    public class SolutionWriter
    {
        public string WriteToFileSystem(Solution solution, string outputPath)
        {
            //vb | csharp
            //new | old format

            //folder
            //files
            //  feature
            //  code //ProgrammLanguageDrivers
            //  app.config
            //  package.config
            // project 
            // nuget.config
            // solution //always dotnet sln

            //see ProjectCompiler.Compile

            return null; //path to solution file
        }
    }
}
