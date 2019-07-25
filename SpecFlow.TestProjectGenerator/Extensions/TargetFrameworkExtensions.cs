using System;
using System.Text;
using TechTalk.SpecFlow.TestProjectGenerator.Data;

namespace TechTalk.SpecFlow.TestProjectGenerator.Extensions
{
    public static class TargetFrameworkExtensions
    {
        private static StringBuilder AppendTargetFrameworkMoniker(this StringBuilder stringBuilder, string tfm)
        {
            return stringBuilder.Append($"{(stringBuilder.Length == 0 ? string.Empty : ";")}{tfm}");
        }

        public static string ToOldNetVersion(this TargetFramework targetFramework)
        {
            StringBuilder AppendMoniker(TargetFramework tf, StringBuilder stringBuilder)
            {
                if (tf != 0 && stringBuilder.Length > 0)
                {
                    throw new InvalidOperationException(
                        "The old project format only supports one target framework version.");
                }

                if ((tf & TargetFramework.Net452) == TargetFramework.Net452)
                {
                    return AppendMoniker(tf & ~TargetFramework.Net452, stringBuilder.AppendTargetFrameworkMoniker("v4.5.2"));
                }

                if ((tf & TargetFramework.Net45) == TargetFramework.Net45)
                {
                    return AppendMoniker(tf & ~TargetFramework.Net45, stringBuilder.AppendTargetFrameworkMoniker("v4.5"));
                }

                if ((tf & TargetFramework.Net35) == TargetFramework.Net35)
                {
                    return AppendMoniker(tf & ~TargetFramework.Net35, stringBuilder.AppendTargetFrameworkMoniker("v3.5"));
                }

                if ((tf & TargetFramework.Netcoreapp20) == TargetFramework.Netcoreapp20
                    || (tf & TargetFramework.NetStandard20) == TargetFramework.NetStandard20)
                {
                    throw new InvalidOperationException(
                        ".NET Core (netcoreapp) and .NET Standard (netstandard) are not supported in the old project format.");
                }

                return stringBuilder;
            }

            return AppendMoniker(targetFramework, new StringBuilder()).ToString();
        }
    }
}
