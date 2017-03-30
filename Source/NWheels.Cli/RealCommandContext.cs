using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Text;

namespace NWheels.Cli
{
    public class RealCommandContext : ICommandContext
    {
        string ICommandContext.GetCurrentDirectory()
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        bool ICommandContext.DirectoryExists(string path)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void ICommandContext.CreateDirectory(string path)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        string[] ICommandContext.FindFiles(string directoryPath, string wildcard, bool recursive)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        bool ICommandContext.FileExists(string path)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void ICommandContext.CreateFile(string path, string[] lines)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        int ICommandContext.ExecuteProgram(string path, string[] args)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void ICommandContext.LogDebug(string message)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void ICommandContext.LogInfo(string message)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void ICommandContext.LogWarning(string message)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void ICommandContext.LogError(string message)
        {
            throw new NotImplementedException();
        }
    }
}
