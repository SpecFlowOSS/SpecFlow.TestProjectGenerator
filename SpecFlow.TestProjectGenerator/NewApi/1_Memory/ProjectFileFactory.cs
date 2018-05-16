using System.IO;
using System.Text;

namespace TechTalk.SpecFlow.TestProjectGenerator.NewApi._1_Memory
{
    public class ProjectFileFactory
    {
        public ProjectFile FromStream(Stream source, string projFileName, string projFileAction)
            => FromStream(source, projFileName, projFileAction, Encoding.Default);

        public ProjectFile FromStream(Stream source, string projFileName, string projFileAction, Encoding encoding)
        {
            if (source.CanSeek)
            {
                source.Seek(0, SeekOrigin.Begin);
            }

            using (var sr = new StreamReader(source, encoding, true, 512, leaveOpen: true))
            {
                return new ProjectFile(projFileName, projFileAction, sr.ReadToEnd());
            }
        }
    }
}
