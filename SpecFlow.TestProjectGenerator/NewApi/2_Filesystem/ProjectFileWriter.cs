using System;
using System.IO;
using SpecFlow.TestProjectGenerator.NewApi._1_Memory;

namespace SpecFlow.TestProjectGenerator.NewApi._2_Filesystem
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

            File.WriteAllText(absolutePath, projectFile.Content);
        }
    }
}
