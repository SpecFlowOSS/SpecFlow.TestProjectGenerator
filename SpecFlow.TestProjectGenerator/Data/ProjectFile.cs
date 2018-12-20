using System.Collections.Generic;
using System.Diagnostics;

namespace TechTalk.SpecFlow.TestProjectGenerator.Data
{
    [DebuggerDisplay("{" + nameof(Path) + ("} [{" + nameof(BuildAction) + "}]"))]
    public class ProjectFile  //FeatureFiles, Code, App.Config, NuGet.Config, packages.config,
    {
        public ProjectFile(string path, string buildAction, string content, CopyToOutputDirectory copyToOutputDirectory = CopyToOutputDirectory.DoNotCopy) :
            this(path, buildAction, content, copyToOutputDirectory, new Dictionary<string, string>())
        {

        }

        public ProjectFile(string path, string buildAction, string content, CopyToOutputDirectory copyToOutputDirectory, IReadOnlyDictionary<string, string> additionalMsBuildProperties)
        {
            Path = path;
            Content = content;
            BuildAction = buildAction;
            CopyToOutputDirectory = copyToOutputDirectory;
            AdditionalMsBuildProperties = additionalMsBuildProperties;
        }

        public string Path { get; } //relative from project
        public string Content { get; }
        public string BuildAction { get; }
        public CopyToOutputDirectory CopyToOutputDirectory { get; }
        public IReadOnlyDictionary<string, string> AdditionalMsBuildProperties { get; }
    }

    public enum CopyToOutputDirectory
    {
        CopyIfNewer,
        CopyAlways,
        DoNotCopy
    }
}