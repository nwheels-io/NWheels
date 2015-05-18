using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using NWheels.Logging.Core;
using NWheels.Utilities.Core;

namespace NWheels.Tools.DevFlow
{
    public class MergeSolutionTool : UtilityToolBase<MergeSolutionTool.Options>
    {
        public const string ToolName = "merge-solution";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Options _options;
        private VisualStudioSolutionFile _sourceSolution;
        private VisualStudioSolutionFile _targetSolution;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MergeSolutionTool(IPlainLog log)
            : base(log)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void Execute(Options options)
        {
            _options = options;

            if ( options.Unmerge )
            {
                Log.Info("Un-merging {0} -X-> {1}", options.SourceSlnPath, options.TargetSlnPath);
            }
            else
            {
                Log.Info("Merging {0} -> {1}", options.SourceSlnPath, options.TargetSlnPath);
            }

            _sourceSolution = new VisualStudioSolutionFile(options.SourceSlnPath, Log);
            _targetSolution = new VisualStudioSolutionFile(options.TargetSlnPath, Log);

            var projectsSubjectToMerge = _sourceSolution.Projects.Where(p => p.ProjectTypeId == VisualStudioSolutionFile.CSharpProjectTypeId).ToArray();

            if ( !string.IsNullOrWhiteSpace(_options.ProjectsFile) )
            {
                projectsSubjectToMerge = FilterProjectsToNerge(projectsSubjectToMerge);
            }
            
            RewriteSolutionFile(projectsSubjectToMerge);
            RewriteProjectFiles(projectsSubjectToMerge);

            _targetSolution.Save();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private VisualStudioSolutionFile.ProjectNode[] FilterProjectsToNerge(IEnumerable<VisualStudioSolutionFile.ProjectNode> projects)
        {
            var projectNames = new HashSet<string>(
                File.ReadAllLines(_options.ProjectsFile).Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)),
                StringComparer.InvariantCultureIgnoreCase);

            return projects.Where(p => projectNames.Contains(p.ProjectName)).ToArray();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void RewriteSolutionFile(VisualStudioSolutionFile.ProjectNode[] projectsSubjectToMerge)
        {
            Log.Debug("Processing target solution.");

            var projectsDone = 0;

            foreach ( var project in projectsSubjectToMerge )
            {
                if ( _options.Unmerge )
                {
                    Log.Debug("Removing project: {0} -X-> {1}", project.ProjectName, _options.TargetSlnPath);

                    var isLastProjectToUnmerge = (projectsDone == projectsSubjectToMerge.Length - 1);
                    _targetSolution.UnMergeProject(_sourceSolution, project, isLastProjectToUnmerge);
                }
                else
                {
                    Log.Debug("Adding project: {0} -> {1}", project.ProjectName, _options.TargetSlnPath);
                    _targetSolution.MergeProject(_sourceSolution, project);
                }

                projectsDone++;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void RewriteProjectFiles(VisualStudioSolutionFile.ProjectNode[] sourceProjectsSubjectToMerge)
        {
            Log.Debug("Processing projects in target solution.");

            foreach ( var sourceProject in sourceProjectsSubjectToMerge )
            {
                if ( _options.Unmerge )
                {
                    _targetSolution.RestoreReferencesToUnmergedProject(sourceProject);
                }
                else
                {
                    _targetSolution.RewriteReferencesToMergedProject(sourceProject);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class Options
        {
            [Option("source", Required = true, HelpText = "Path to source .sln file")]
            public string SourceSlnPath { get; set; }
            [Option("target", Required = true, HelpText = "Path to target .sln file")]
            public string TargetSlnPath { get; set; }
            [Option('u', "unmerge", HelpText = "Specify to un-merge the solution (revert merge)")]
            public bool Unmerge { get; set; }
            [Option('p', "projects", 
                HelpText = "Optional. Path to a text file that lists projects to merge (each line must contain project name without the .DLL extension)")]
            public string ProjectsFile { get; set; }
        }
    }
}
