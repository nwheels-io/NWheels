using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LibGit2Sharp;
using MetaPrograms;

namespace NWheels.Demos.Tests.Helpers
{
    public static class GitRepoFilter
    {
        public static Func<string, bool> Create(FilePath directory)
        {
            var repoRoot = FindGitRepoRoot(directory);
            var directoryRelativeToRepo = directory.RelativeTo(repoRoot);
            
            using (var repo = new Repository(repoRoot.FullPath))
            {
                var filesInRepo = repo.Index.Select(entry => entry.Path)
                    .Where(path => path.StartsWith(directoryRelativeToRepo.FullPath))
                    .Select(path => FilePath.Parse(path))
                    .Select(path => path.RelativeTo(directoryRelativeToRepo))
                    .Select(path => path.FullPath);

                var filesInRepoSet = new HashSet<string>(filesInRepo);
                return filesInRepoSet.Contains;
            }
        }

        private static FilePath FindGitRepoRoot(FilePath probe)
        {
            while (!Directory.Exists(probe.Append(".git").FullPath))
            {
                if (probe.SubFolder.Count == 0)
                {
                    throw new ArgumentException("Specified path is not inside a git repo");
                }

                probe = probe.Up(1);
            }

            return probe;
        }
    }
}
