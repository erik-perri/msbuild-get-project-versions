using Microsoft.Build.Framework;
using System;
using System.IO;
using System.Text.RegularExpressions;

// Based on code by Tom Hunter posted at https://stackoverflow.com/a/1253478
namespace GetProjectVersions
{
    public class GetFromAssemblyFile : ITask
    {
        [Required]
        public string AssemblyFile { get; set; }

        public bool StripToBuildNumber { get; set; }

        [Output]
        public string AssemblyVersion { get; set; }

        [Output]
        public string AssemblyFileVersion { get; set; }

        public bool Execute()
        {
            if (!File.Exists(AssemblyFile))
            {
                BuildEngine.LogMessageEvent(new BuildMessageEventArgs(
                    $"Failed to find assembly file \"{AssemblyFile}\"",
                    string.Empty,
                    "GetProjectVersions",
                    MessageImportance.High
                ));
                return false;
            }

            try
            {
                UpdateValuesFromFile(AssemblyFile);
            }
            catch (Exception e)
            {
                BuildEngine.LogMessageEvent(new BuildMessageEventArgs(
                    $"Failed to load versions from file \"{AssemblyFile}\", {e.Message}",
                    string.Empty,
                    "GetProjectVersions",
                    MessageImportance.High
                ));
            }

            return true;
        }

        private void UpdateValuesFromFile(string file)
        {
            AssemblyVersion = string.Empty;
            AssemblyFileVersion = string.Empty;

            using (var streamReader = new StreamReader(file))
            {
                while (!streamReader.EndOfStream)
                {
                    var line = streamReader.ReadLine();
                    var match = Regex.Match(
                        line,
                        @"(?:(?<type>AssemblyFileVersion|AssemblyVersion)(Attribute)?\("")(?<version>(\d*)\.(\d*)(\.(\d*)(\.(\d*))?)?)(?:""\))",
                        RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline |
                        RegexOptions.ExplicitCapture
                    );

                    if (match.Success)
                    {
                        switch (match.Groups["type"].Value)
                        {
                            case "AssemblyVersion":
                                AssemblyVersion = StripVersion(match.Groups["version"].Value);
                                break;

                            case "AssemblyFileVersion":
                                AssemblyFileVersion = StripVersion(match.Groups["version"].Value);
                                break;
                        }
                    }
                }
            }
        }

        private string StripVersion(string versionString)
        {
            if (!StripToBuildNumber)
            {
                return versionString;
            }

            var version = Version.Parse(versionString);

            return $"{version.Major}.{version.Minor}.{version.Build}";
        }

        public IBuildEngine BuildEngine { get; set; }

        public ITaskHost HostObject { get; set; }
    }
}