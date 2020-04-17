# GetProjectVersions MSBuild Task

This is a basic MSBuild task to obtain the version from a manifest file or C# project file.

I used it to obtain the version in a WiX setup project before switching to a different versioning solution which had
its own task helper.

## Add version to TargetName

```xml
<UsingTask AssemblyFile="GetProjectVersions.dll" TaskName="GetProjectVersions.GetFromProjectFile" />
<Target Name="BeforeBuild">
  <GetFromProjectFile ProjectFile="$(SolutionDir)\MainApp\MainApp.csproj">
      <Output TaskParameter="PackageVersion" PropertyName="AppPackageVersion" />
  </GetFromProjectFile>
  <CreateProperty Value="$(SolutionName)-$(AppPackageVersion)">
    <Output TaskParameter="Value" PropertyName="TargetName" />
  </CreateProperty>
</Target>
```


## Add version to environment variables

```xml
<UsingTask AssemblyFile="GetProjectVersions.dll" TaskName="GetProjectVersions.GetFromProjectFile" />
<UsingTask AssemblyFile="$(MSBuildToolsPath)\Microsoft.Build.Tasks.Core.dll" TaskName="SetEnvironmentVariableTask" TaskFactory="CodeTaskFactory">
  <ParameterGroup>
    <Name ParameterType="System.String" Required="true" />
    <Value ParameterType="System.String" Required="true" />
  </ParameterGroup>
  <Task>
    <Using Namespace="System" />
    <Code Type="Fragment" Language="cs">
      <![CDATA[
        Environment.SetEnvironmentVariable(Name, Value);
      ]]>
    </Code>
  </Task>
</UsingTask>
<Target Name="BeforeBuild">
  <GetFromProjectFile ProjectFile="$(SolutionDir)\MainApp\MainApp.csproj">
    <Output TaskParameter="PackageVersion" PropertyName="AppPackageVersion" />
  </GetFromProjectFile>
  <SetEnvironmentVariableTask Name="AppPackageVersion" Value="$(AppPackageVersion)" />
</Target>
```