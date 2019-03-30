namespace NWheels.Build
{
    public class BuildOptions
    {
        public BuildOptions(string projectFilePath)
        {
            ProjectFilePath = projectFilePath;
        }

        public string ProjectFilePath { get; }
    }
}