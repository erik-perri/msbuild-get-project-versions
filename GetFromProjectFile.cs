using Microsoft.Build.Framework;
using System;
using System.IO;
using System.Xml.Linq;

namespace GetProjectVersions
{
    public class GetFromProjectFile : ITask
    {
        [Required]
        public string ProjectFile { get; set; }

        [Output]
        public string PackageVersion { get; set; }

        [Output]
        public string AssemblyVersion { get; set; }

        [Output]
        public string AssemblyFileVersion { get; set; }

        public bool Execute()
        {
            if (!File.Exists(ProjectFile))
            {
                BuildEngine.LogMessageEvent(new BuildMessageEventArgs(
                    $"Failed to find project file \"{ProjectFile}\"",
                    string.Empty,
                    "GetProjectVersions",
                    MessageImportance.High
                ));
                return false;
            }

            try
            {
                UpdateValuesFromFile(ProjectFile);
                return true;
            }
            catch (Exception e)
            {
                BuildEngine.LogMessageEvent(new BuildMessageEventArgs(
                    $"Failed to load versions from file \"{ProjectFile}\", {e.Message}",
                    string.Empty,
                    "GetProjectVersions",
                    MessageImportance.High
                ));
            }

            return false;
        }

        private void UpdateValuesFromFile(string file)
        {
            PackageVersion = null;
            AssemblyVersion = null;
            AssemblyFileVersion = null;

            var definition = XDocument.Load(file);
            foreach (var group in definition.Element("Project")?.Elements("PropertyGroup"))
            {
                foreach (var property in group.Elements())
                {
                    switch (property.Name.LocalName)
                    {
                        case "Version":
                            PackageVersion = property.Value;
                            break;

                        case "AssemblyVersion":
                            AssemblyVersion = property.Value;
                            break;

                        case "FileVersion":
                            AssemblyFileVersion = property.Value;
                            break;
                    }
                }
            }
        }

        public IBuildEngine BuildEngine { get; set; }

        public ITaskHost HostObject { get; set; }
    }
}