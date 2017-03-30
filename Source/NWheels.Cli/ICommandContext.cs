using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Cli
{
    public interface ICommandContext
    {
        string GetCurrentDirectory();
        bool DirectoryExists(string path);
        void CreateDirectory(string path);
        string[] FindFiles(string directoryPath, string wildcard, bool recursive);
        bool FileExists(string path);
        void CreateFile(string path, params string[] lines);
        int ExecuteProgram(string path, params string[] args);
        void LogDebug(string message);
        void LogInfo(string message);
        void LogWarning(string message);
        void LogError(string message);
    }
}
