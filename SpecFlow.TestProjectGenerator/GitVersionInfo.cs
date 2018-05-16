using System;

namespace TechTalk.SpecFlow.TestProjectGenerator
{
    public class GitVersionInfo
    {
        public int Major { get; set; }

        public int Minor { get; set; }

        public int Patch { get; set; }

        public string PreReleaseTag { get; set; }

        public string PreReleaseTagWithDash { get; set; }

        public int PreReleaseNumber { get; set; }

        public string BuildMetaData { get; set; }

        public string BuildMetaDataPadded { get; set; }

        public string FullBuildMetaData { get; set; }

        public string MajorMinorPatch { get; set; }

        public string SemVer { get; set; }

        public string LegacySemVer { get; set; }

        public string LegacySemVerPadded { get; set; }

        public string AssemblySemVer { get; set; }

        public string FullSemVer { get; set; }

        public string InformationalVersion { get; set; }

        public string BranchName { get; set; }

        public string Sha { get; set; }

        public string NuGetVersionV2 { get; set; }

        public string NuGetVersion { get; set; }

        public int CommitsSinceVerionSource { get; set; }

        public string CommitsSinceVersionSourcePadded { get; set; }

        public DateTime CommitDate { get; set; }
    }
}
