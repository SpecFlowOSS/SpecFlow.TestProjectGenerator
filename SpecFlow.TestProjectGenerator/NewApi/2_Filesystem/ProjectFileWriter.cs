using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            File.WriteAllText(absolutePath, projectFile.Content);
        }
    }
}
