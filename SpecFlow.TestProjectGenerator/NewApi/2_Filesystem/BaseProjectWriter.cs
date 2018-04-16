using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpecFlow.TestProjectGenerator.NewApi._1_Memory;

namespace SpecFlow.TestProjectGenerator.NewApi._2_Filesystem
{
    public abstract class BaseProjectWriter
    {
        public abstract string WriteProject(Project project, string path);
    }
}
