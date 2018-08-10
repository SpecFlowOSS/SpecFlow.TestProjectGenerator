using System;
using System.IO;
using TechTalk.SpecFlow.TestProjectGenerator.Helpers;
using TechTalk.SpecFlow.TestProjectGenerator.NewApi._1_Memory;

namespace TechTalk.SpecFlow.TestProjectGenerator.NewApi._2_Filesystem
{
    public class ProjectFileWriter
    {
        public void Write(ProjectFile projectFile, string projectRootPath)
        {
            if (projectFile is null)
            {
                throw new ArgumentNullException(nameof(projectFile));
            }

            string absolutePath = Path.Combine(projectRootPath, projectFile.Path);
            string folderPath = Path.GetDirectoryName(absolutePath);

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            if (projectFile.Content.IsNotNullOrWhiteSpace())
            {
                File.WriteAllText(absolutePath, projectFile.Content);
            }
        }
    }
}
