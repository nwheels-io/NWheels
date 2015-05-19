using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using NPEG;
using NPEG.GrammarInterpreter;
using NWheels.Tools.DevFlow.Parsers;
using NWheels.Logging.Core;
using System.Xml.Linq;
using System.Xml.XPath;
using NWheels.Utilities;

namespace NWheels.Tools.DevFlow
{
    internal class VisualStudioSolutionFile
    {
        private readonly string _filePath;
        private readonly IPlainLog _log;
        private readonly PegNode _syntaxRoot;
        private readonly List<HeaderNode> _headers;
        private readonly List<ProjectNode> _projects;
        private readonly GlobalNode _global;
        private readonly GlobalSectionNode _projectConfigurationPlatforms;
        private readonly int _originalProjectCount;
        private GlobalSectionNode _nestedProjects;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public VisualStudioSolutionFile(string filePath, IPlainLog log)
        {
            _filePath = (Path.IsPathRooted(filePath) ? filePath : PathUtility.GetAbsolutePath(filePath, relativeTo: Environment.CurrentDirectory));
            _log = log;

            var fileText = File.ReadAllText(filePath); 

            if ( !Parse(fileText, out _syntaxRoot) )
            {
                throw new Exception("Specified solution file could not be parsed.");
            }

            var topLevelSemanticNodes = CreateSemanticNodes(fileText);

            _headers = topLevelSemanticNodes.OfType<HeaderNode>().ToList();
            _projects = topLevelSemanticNodes.OfType<ProjectNode>().ToList();
            _global = topLevelSemanticNodes.OfType<GlobalNode>().Single();
            _projectConfigurationPlatforms = _global.SubNodes.OfType<GlobalSectionNode>().Single(s => s.SectionId == "ProjectConfigurationPlatforms");
            _nestedProjects = _global.SubNodes.OfType<GlobalSectionNode>().FirstOrDefault(s => s.SectionId == "NestedProjects");

            _originalProjectCount = _projects.Count;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void MergeProject(VisualStudioSolutionFile sourceSolution, ProjectNode sourceProject)
        {
            if ( TryFindMergedProject(sourceProject) != null )
            {
                _log.Warning("Project '{0}' already exists in the target solution - skipping.", sourceProject.ProjectName);
                return;
            }

            var solutionFolder = GetOrAddSolutionFolder(sourceSolution.FileName);
            
            var targetSolutionRelativePath = GetRelativePath(sourceSolution.GetAbsolutePath(sourceProject.SolutionRelativePath));
            var newProject = new ProjectNode(
                sourceProject.ProjectTypeId, 
                sourceProject.ProjectId, 
                sourceProject.ProjectName, 
                targetSolutionRelativePath);

            _projects.Add(newProject);

            AddProjectPlatformConfiguration(newProject.ProjectId, "Debug|Any CPU.ActiveCfg = Debug|Any CPU");
            AddProjectPlatformConfiguration(newProject.ProjectId, "Debug|Any CPU.Build.0 = Debug|Any CPU");
            AddProjectPlatformConfiguration(newProject.ProjectId, "Release|Any CPU.ActiveCfg = Release|Any CPU");
            AddProjectPlatformConfiguration(newProject.ProjectId, "Release|Any CPU.Build.0 = Release|Any CPU");

            AddNestedProject(child: newProject, parent: solutionFolder);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void UnMergeProject(VisualStudioSolutionFile sourceSolution, ProjectNode sourceProject, bool isLastProjectToUnmerge)
        {
            var mergedProject = TryFindMergedProject(sourceProject);

            if ( mergedProject == null )
            {
                _log.Warning("Project '{0}' doesn't exist in the target solution - skipping.", sourceProject.ProjectName);
                return;
            }

            _projects.Remove(mergedProject);

            var solutionFolder = TryFindSolutionFolder(sourceSolution.FileName);
            var platformConfigurationLines = FindProjectPlatformConfigurations(mergedProject);
            var nestedProjectLine = (solutionFolder != null ? TryFindNestedProjectLine(child: mergedProject, parent: solutionFolder) : null);

            _log.Debug("Removing {0} platform configurations.", platformConfigurationLines.Length);

            foreach (var line in platformConfigurationLines)
            {
                _projectConfigurationPlatforms.SubNodes.Remove(line);
                _projectConfigurationPlatforms.InvalidateNodeText();
            }

            if ( nestedProjectLine != null )
            {
                _log.Debug("Removing project nesting under solution folder.", platformConfigurationLines.Length);
                _nestedProjects.SubNodes.Remove(nestedProjectLine);
                _nestedProjects.InvalidateNodeText();

                if ( _nestedProjects.SubNodes.Count == 0 )
                {
                    _log.Debug("NestedProjects section is now empty - removing.", platformConfigurationLines.Length);
                    _global.SubNodes.Remove(_nestedProjects);
                    _global.InvalidateNodeText();
                    _nestedProjects = null;
                }
            }
            else
            {
                _log.Warning("Project nesting under solution folder doesn't exist - skipping.");
            }

            if ( solutionFolder != null )
            {
                if ( !FindNestedProjectLines(parent: solutionFolder).Any() )
                {
                    _log.Debug("Removing solution folder '{0}'.", solutionFolder.ProjectName);
                    _projects.Remove(solutionFolder);
                }
                else if ( isLastProjectToUnmerge )
                {
                    _log.Warning("Solution folder '{0}' in the target solution is not empty - cannot remove it.", solutionFolder.ProjectName);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void RewriteReferencesToMergedProject(ProjectNode mergedSourceProject)
        {
            for ( int i = 0 ; i < _originalProjectCount && i < _projects.Count ; i++ )
            {
                if ( _projects[i].IsCSharpProject )
                {
                    _projects[i].RewriteReferenceToMergedProject(this, mergedSourceProject);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void RestoreReferencesToUnmergedProject(ProjectNode unmergedSourceProject)
        {
            for ( int i = 0; i < _projects.Count ; i++ )
            {
                if ( _projects[i].IsCSharpProject )
                {
                    _projects[i].RestoreReferenceToUnmergedProject(this, unmergedSourceProject);
                }
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public string GetAbsolutePath(string relativePath)
        {
            return GetAbsolutePath(_filePath, relativePath);
            //var combinedAbsolutePath = Path.Combine(Path.GetDirectoryName(_filePath), relativePath);
            //var cleanAbsolutePath = Path.GetFullPath(new Uri(combinedAbsolutePath).LocalPath).Replace("/", "\\");

            //return cleanAbsolutePath;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public string GetRelativePath(string absolutePath)
        {
            return GetRelativePath(_filePath, absolutePath);
            //var relativePath = new Uri(_filePath).MakeRelativeUri(new Uri(absolutePath)).ToString().Replace("/", "\\");
            //return relativePath;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public void Save()
        {
            var contents = new StringBuilder();

            foreach ( var header in _headers )
            {
                contents.Append(header.NodeText);
            }

            foreach ( var project in _projects )
            {
                contents.Append(project.NodeText);
            }

            contents.Append(_global.NodeText);

            Log.Debug("Saving modifications to {0}", _filePath);
            File.WriteAllText(_filePath, contents.ToString());

            SaveProjects();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string FilePath
        {
            get { return _filePath; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string FileName
        {
            get { return Path.GetFileName(_filePath); }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public List<ProjectNode> Projects
        {
            get { return _projects; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IPlainLog Log
        {
            get { return _log; }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public static string GetAbsolutePath(string originPath, string relativePath)
        {
            return PathUtility.GetAbsolutePath(relativePath, originPath, isRelativeToFile: true);

            //var combinedAbsolutePath = Path.Combine(Path.GetDirectoryName(originPath), relativePath);
            //var cleanAbsolutePath = Path.GetFullPath(new Uri(combinedAbsolutePath).LocalPath).Replace("/", "\\");

            //return cleanAbsolutePath;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public static string GetRelativePath(string originPath, string absolutePath)
        {
            return PathUtility.GetRelativePath(absolutePath, originPath);

            //var relativePath = new Uri(originPath).MakeRelativeUri(new Uri(absolutePath)).ToString().Replace("/", "\\");
            //return relativePath;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool Parse(string fileText, out PegNode syntaxRoot)
        {
            var errorOutput = new StringWriter();
            var parser = new VisualStudioSolutionParser(fileText, errorOutput);

            if ( !parser.solution() )
            {
                syntaxRoot = null;
                return false;
            }

            syntaxRoot = parser.GetRoot();

            if ( syntaxRoot.VsSlnNodeType() != VsSlnNodeType.solution )
            {
                return false;
            }

            return true;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private List<SemanticNode> CreateSemanticNodes(string fileText)
        {
            var topLevelSemanticNodes = new List<SemanticNode>();

            for ( var syntaxNode = _syntaxRoot.child_ ; syntaxNode != null ; syntaxNode = syntaxNode.next_ )
            {
                var semanticNode = SemanticNode.Create(null, syntaxNode, fileText);
                topLevelSemanticNodes.Add(semanticNode);
            }

            return topLevelSemanticNodes;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void EnsureNestedProjectsSection()
        {
            if ( _nestedProjects == null )
            {
                _nestedProjects = new GlobalSectionNode(_global, "NestedProjects", VsSlnNodeType.pre_solution);
                _global.SubNodes.Add(_nestedProjects);

                _log.Debug("NestedProjects section didn't exist in the target solution - added.");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void SaveProjects()
        {
            foreach ( var project in _projects.Where(p => p.ProjectFileChanged) )
            {
                project.SaveProjectFile(this);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void AddNestedProject(ProjectNode child, ProjectNode parent)
        {
            EnsureNestedProjectsSection();

            if ( TryFindNestedProjectLine(child, parent) == null )
            {
                var line = new GuidAndGuidGlobalSectionLineNode(_nestedProjects, child.ProjectId, parent.ProjectId);
                _nestedProjects.SubNodes.Add(line);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private ProjectNode TryFindSolutionFolder(string folderName)
        {
            return _projects.FirstOrDefault(p => p.ProjectTypeId == SolutionFolderProjectTypeId && p.ProjectName == folderName);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private ProjectNode GetOrAddSolutionFolder(string folderName)
        {
            var existingFolder = TryFindSolutionFolder(folderName);

            if ( existingFolder != null )
            {
                return existingFolder;
            }

            var newFolder = new ProjectNode(SolutionFolderProjectTypeId, Guid.NewGuid(), folderName, folderName);
            _projects.Add(newFolder);

            _log.Debug("Added solution folder '{0}'.", folderName);

            return newFolder;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private GuidAndTextGlobalSectionLineNode[] FindProjectPlatformConfigurations(ProjectNode project)
        {
            return _projectConfigurationPlatforms.SubNodes
                .OfType<GuidAndTextGlobalSectionLineNode>()
                .Where(line => line.LeftHandSide == project.ProjectId)
                .ToArray();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void AddProjectPlatformConfiguration(Guid projectId, string platformConfiguration)
        {
            var newNode = new GuidAndTextGlobalSectionLineNode(_projectConfigurationPlatforms, projectId, platformConfiguration);
            _projectConfigurationPlatforms.SubNodes.Add(newNode);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private ProjectNode TryFindMergedProject(ProjectNode sourceProject)
        {
            return _projects.FirstOrDefault(p => p.ProjectId == sourceProject.ProjectId);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private GuidAndGuidGlobalSectionLineNode TryFindNestedProjectLine(ProjectNode child, ProjectNode parent)
        {
            if ( _nestedProjects == null )
            {
                return null;
            }

            return _nestedProjects.SubNodes
                .OfType<GuidAndGuidGlobalSectionLineNode>()
                .FirstOrDefault(line => line.LeftHandSide == child.ProjectId && line.RightHandSide == parent.ProjectId);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private GuidAndGuidGlobalSectionLineNode[] FindNestedProjectLines(ProjectNode parent)
        {
            if ( _nestedProjects == null )
            {
                return new GuidAndGuidGlobalSectionLineNode[0];
            }

            return _nestedProjects.SubNodes
                .OfType<GuidAndGuidGlobalSectionLineNode>()
                .Where(line => line.RightHandSide == parent.ProjectId)
                .ToArray();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly Guid _s_csharpProjectTypeId = Guid.Parse("FAE04EC0-301F-11D3-BF4B-00C04F79EFBC");
        private static readonly Guid _s_solutionFolderProjectTypeId = Guid.Parse("2150E333-8FDC-42A3-9474-1A3956D46DE8");

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static Guid CSharpProjectTypeId
        {
            get { return _s_csharpProjectTypeId; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static Guid SolutionFolderProjectTypeId
        {
            get { return _s_solutionFolderProjectTypeId; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class SemanticNode
        {
            private readonly SemanticNode _parentNode;
            private readonly List<SemanticNode> _subNodes;
            private string _nodeText;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public SemanticNode(SemanticNode parentNode)
            {
                _parentNode = parentNode;
                _subNodes = new List<SemanticNode>();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public SemanticNode(SemanticNode parentNode, PegNode syntaxNode, string fileText)
                : this(parentNode)
            {
                _nodeText = syntaxNode.GetAsString(fileText);

                for ( var syntaxChildNode = syntaxNode.child_ ; syntaxChildNode != null ; syntaxChildNode = syntaxChildNode.next_ )
                {
                    var semanticChildNode = SemanticNode.Create(this, syntaxChildNode, fileText);

                    if ( semanticChildNode != null )
                    {
                        _subNodes.Add(semanticChildNode);
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void InvalidateNodeText()
            {
                _nodeText = null;

                if ( _parentNode != null )
                {
                    _parentNode.InvalidateNodeText();
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string NodeText
            {
                get
                {
                    if ( _nodeText == null )
                    {
                        _nodeText = RebuildNodeText();
                    }

                    return _nodeText;
                }
                protected set
                {
                    _nodeText = value;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public SemanticNode ParentNode
            {
                get { return _parentNode; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public List<SemanticNode> SubNodes
            {
                get { return _subNodes; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected virtual string RebuildNodeText()
            {
                var newText = new StringBuilder();

                foreach ( var subNode in _subNodes )
                {
                    newText.Append(subNode.NodeText);
                }

                return newText.ToString();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static SemanticNode Create(SemanticNode parentNode, PegNode syntaxNode, string fileText)
            {
                switch ( syntaxNode.VsSlnNodeType() )
                {
                    case VsSlnNodeType.header_line:
                        return new HeaderNode(parentNode, syntaxNode, fileText);
                    case VsSlnNodeType.project:
                        return new ProjectNode(parentNode, syntaxNode, fileText);
                    case VsSlnNodeType.global:
                        return new GlobalNode(parentNode, syntaxNode, fileText);
                    case VsSlnNodeType.global_section:
                        return new GlobalSectionNode(parentNode, syntaxNode, fileText);
                    case VsSlnNodeType.global_section_line:
                        if ( syntaxNode.child_ != null )
                        {
                            return Create(parentNode, syntaxNode.child_, fileText);
                        }
                        else
                        {
                            return new TextGlobalSectionLineNode(parentNode, syntaxNode, fileText);
                        }
                    case VsSlnNodeType.lhs_guid_line:
                        return new GuidAndTextGlobalSectionLineNode(parentNode, syntaxNode, fileText);
                    case VsSlnNodeType.lhs_rhs_guid_line:
                        return new GuidAndGuidGlobalSectionLineNode(parentNode, syntaxNode, fileText);
                    default:
                        return null;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class HeaderNode : SemanticNode
        {
            public HeaderNode(SemanticNode parentNode, PegNode syntaxNode, string fileText)
                : base(parentNode, syntaxNode, fileText)
            {
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ProjectNode : SemanticNode
        {
            private XNamespace _namespace = "http://schemas.microsoft.com/developer/msbuild/2003";
            private XDocument _projectFile = null;
            private bool _projectFileChanged = false;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ProjectNode(SemanticNode parentNode, PegNode syntaxNode, string fileText)
                : base(parentNode, syntaxNode, fileText)
            {
                var headerNode = syntaxNode.SubNodes(VsSlnNodeType.project_header, recursive: false).Single();
                var guids = headerNode.SubNodes(VsSlnNodeType.guid);
                var strings = headerNode.SubNodes(VsSlnNodeType.@string);

                this.ProjectTypeId = Guid.Parse(guids[0].GetAsString(fileText));
                this.ProjectId = Guid.Parse(guids[1].GetAsString(fileText));
                this.ProjectName = strings[0].GetAsString(fileText);
                this.SolutionRelativePath = strings[1].GetAsString(fileText);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ProjectNode(Guid projectTypeId, Guid projectId, string projectName, string solutionRelativePath)
                : base(parentNode: null)
            {
                this.ProjectTypeId = projectTypeId;
                this.ProjectId = projectId;
                this.ProjectName = projectName;
                this.SolutionRelativePath = solutionRelativePath;

                InvalidateNodeText();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void RewriteReferenceToMergedProject(VisualStudioSolutionFile ownerSolution, ProjectNode mergedSourceProject)
            {
                var mergedTargetProject = ownerSolution.Projects.Single(p => p.ProjectId == mergedSourceProject.ProjectId);

                LoadProjectFile(ownerSolution);

                var allReferences = _projectFile.Root.Elements(_namespace + "ItemGroup").SelectMany(g => g.Elements(_namespace + "Reference")).ToArray();
                var affectedReference = allReferences.SingleOrDefault(r => GetRefernceElementProjectName(r) == mergedTargetProject.ProjectName);

                if ( affectedReference != null )
                {
                    ownerSolution.Log.Debug("Rewriting reference '{0}' -> '{1}'.", this.ProjectName, mergedTargetProject.ProjectName);

                    var backupComment = new XComment(string.Format("merge-solution:{0};{1}", mergedTargetProject.ProjectName, affectedReference.ToString()));
                    var rewrittenReference = new XElement(_namespace + "ProjectReference",
                        new XAttribute("Include", GetProjectRelativePath(ownerSolution, ownerSolution.GetAbsolutePath(mergedTargetProject.SolutionRelativePath))),
                        new XText("\r\n      "),
                        new XElement(_namespace + "Project", mergedTargetProject.ProjectId.ToString("B").ToLower()),
                        new XText("\r\n      "),
                        new XElement(_namespace + "Name", mergedTargetProject.ProjectName),
                        new XText("\r\n    "));

                    affectedReference.ReplaceWith(backupComment);
                    backupComment.AddAfterSelf(new XText("\r\n    "), rewrittenReference);

                    _projectFileChanged = true;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void RestoreReferenceToUnmergedProject(VisualStudioSolutionFile ownerSolution, ProjectNode unmergedSourceProject)
            {
                LoadProjectFile(ownerSolution);

                var allReferences = _projectFile.Root.Elements(_namespace + "ItemGroup").SelectMany(g => g.Elements(_namespace + "ProjectReference")).ToArray();
                var affectedReference = allReferences.SingleOrDefault(r => GetProjectRefernceElementProjectName(r) == unmergedSourceProject.ProjectName);

                if ( affectedReference != null )
                {
                    XElement backupReference = null;
                    var backupComment = affectedReference.Parent.Nodes().First(node => IsReferenceBackupCommentNode(node, unmergedSourceProject, out backupReference));

                    ownerSolution.Log.Debug("Restoring reference '{0}' -> '{1}'.", this.ProjectName, unmergedSourceProject.ProjectName);

                    var rebuiltReference = RebuildReferenceElement(backupReference);
                    affectedReference.ReplaceWith(rebuiltReference);

                    if ( backupComment.NextNode is XText )
                    {
                        backupComment.NextNode.Remove();
                    }
                    backupComment.Remove();

                    _projectFileChanged = true;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void SaveProjectFile(VisualStudioSolutionFile ownerSolution)
            {
                if ( _projectFile != null && _projectFileChanged )
                {
                    var filePath = ownerSolution.GetAbsolutePath(this.SolutionRelativePath);

                    ownerSolution.Log.Debug("Saving modifications to {0}", filePath);
                    _projectFile.Save(filePath, SaveOptions.DisableFormatting);
                }
                else
                {
                    ownerSolution.Log.Debug("Project '{0}' was not modified - skipping save.", this.ProjectName);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Guid ProjectTypeId { get; private set; }
            public Guid ProjectId { get; private set; }
            public string ProjectName { get; private set; }
            public string SolutionRelativePath { get; private set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool IsCSharpProject
            {
                get { return (this.ProjectTypeId == CSharpProjectTypeId); }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool IsSolutionFolderProject
            {
                get { return (this.ProjectTypeId == SolutionFolderProjectTypeId); }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool ProjectFileChanged
            {
                get { return _projectFileChanged; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Overrides of SemanticNode

            protected override string RebuildNodeText()
            {
                var text = new StringBuilder();

                text.AppendFormat(
                    "Project(\"{0}\") = \"{1}\", \"{2}\", \"{3}\"", 
                    ProjectTypeId.ToString("B").ToUpper(), 
                    ProjectName, 
                    SolutionRelativePath, 
                    ProjectId.ToString("B").ToUpper());
                
                text.AppendLine();
                text.AppendLine("EndProject");

                return text.ToString();
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private string GetProjectRelativePath(VisualStudioSolutionFile ownerSolution, string absolutePath)
            {
                var thisAbsolutePath = ownerSolution.GetAbsolutePath(this.SolutionRelativePath);
                return VisualStudioSolutionFile.GetRelativePath(thisAbsolutePath, absolutePath);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void LoadProjectFile(VisualStudioSolutionFile ownerSolution)
            {
                if ( _projectFile == null )
                {
                    var filePath = ownerSolution.GetAbsolutePath(this.SolutionRelativePath);
                    _projectFile = XDocument.Load(filePath, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            private string GetRefernceElementProjectName(XElement referenceElement)
            {
                var includeAttribute = referenceElement.Attribute("Include");

                if (includeAttribute != null)
                {
                    var includeValueParts = includeAttribute.Value.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    if (includeValueParts.Length > 0)
                    {
                        return includeValueParts[0];
                    }
                }

                return null;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private string GetProjectRefernceElementProjectName(XElement referenceElement)
            {
                var nameElement = referenceElement.Element(_namespace + "Name");

                if ( nameElement != null )
                {
                    return nameElement.Value;
                }
                else
                {
                    return null;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private bool IsReferenceBackupCommentNode(XNode node, ProjectNode project, out XElement restoredXml)
            {
                restoredXml = null;
                
                var comment = (node as XComment);

                if ( comment != null )
                {
                    var commentPrefix = "merge-solution:" + project.ProjectName + ";";

                    if ( comment.Value.StartsWith(commentPrefix) )
                    {
                        restoredXml = XElement.Parse(comment.Value.Substring(commentPrefix.Length));
                    }
                }

                return (restoredXml != null);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            private XElement RebuildReferenceElement(XElement backup)
            {
                var rebuilt = new XElement(_namespace + "Reference");

                foreach ( var attribute in backup.Attributes() )
                {
                    if ( attribute.Name != "xmlns" )
                    {
                        rebuilt.Add(new XAttribute(attribute.Name.LocalName, attribute.Value));
                    }
                }

                foreach ( var nested in backup.Elements() )
                {
                    rebuilt.Add(new XText("\r\n      "));
                    rebuilt.Add(new XElement(_namespace + nested.Name.LocalName, nested.Value));
                }

                rebuilt.Add(new XText("\r\n    "));
                return rebuilt;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class GlobalNode : SemanticNode
        {
            public GlobalNode(SemanticNode parentNode, PegNode syntaxNode, string fileText)
                : base(parentNode, syntaxNode, fileText)
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Overrides of SemanticNode

            protected override string RebuildNodeText()
            {
                var text = new StringBuilder();

                text.AppendLine("Global");
                text.Append(base.RebuildNodeText());
                text.AppendLine("EndGlobal");

                return text.ToString();
            }

            #endregion
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class GlobalSectionNode : SemanticNode
        {
            public GlobalSectionNode(SemanticNode parentNode, PegNode syntaxNode, string fileText)
                : base(parentNode, syntaxNode, fileText)
            {
                var headerNode = syntaxNode.SubNodes(VsSlnNodeType.global_section_header, recursive: false).Single();
                var sectionIdNode = headerNode.SubNodes(VsSlnNodeType.global_section_id).Single();
                var preSolutionNode = headerNode.SubNodes(VsSlnNodeType.pre_solution).FirstOrDefault();
                var postSolutionNode = headerNode.SubNodes(VsSlnNodeType.post_solution).FirstOrDefault();

                this.SectionId = sectionIdNode.GetAsString(fileText);
                this.PrePostSolution = (preSolutionNode ?? postSolutionNode).VsSlnNodeType();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public GlobalSectionNode(SemanticNode parentNode, string sectionId, VsSlnNodeType prePostSolution)
                : base(parentNode)
            {
                this.SectionId = sectionId;
                this.PrePostSolution = prePostSolution;

                InvalidateNodeText();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string SectionId { get; private set; }
            public VsSlnNodeType PrePostSolution { get; private set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Overrides of SemanticNode

            protected override string RebuildNodeText()
            {
                var text = new StringBuilder();

                text.AppendFormat(
                    "\tGlobalSection({0}) = {1}",
                    SectionId, 
                    PrePostSolution == VsSlnNodeType.pre_solution ? "preSolution" : "postSolution");

                text.AppendLine();
                text.Append(base.RebuildNodeText());
                text.AppendLine("\tEndGlobalSection");

                return text.ToString();
            }

            #endregion
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class TextGlobalSectionLineNode : SemanticNode
        {
            public TextGlobalSectionLineNode(SemanticNode parentNode, PegNode syntaxNode, string fileText)
                : base(parentNode, syntaxNode, fileText)
            {
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class GuidAndTextGlobalSectionLineNode : SemanticNode
        {
            public GuidAndTextGlobalSectionLineNode(SemanticNode parentNode, PegNode syntaxNode, string fileText)
                : base(parentNode, syntaxNode, fileText)
            {
                var lhsNode = syntaxNode.SubNodes(VsSlnNodeType.guid).Single();
                var rhsNode = syntaxNode.SubNodes(VsSlnNodeType.lhs_guid_line_rhs).Single();

                LeftHandSide = Guid.Parse(lhsNode.GetAsString(fileText));
                RightHandSide = rhsNode.GetAsString(fileText);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public GuidAndTextGlobalSectionLineNode(SemanticNode parentNode, Guid leftHandSide, string rightHandSide)
                : base(parentNode)
            {
                this.LeftHandSide = leftHandSide;
                this.RightHandSide = rightHandSide;

                InvalidateNodeText();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Guid LeftHandSide { get; private set; }
            public string RightHandSide { get; private set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Overrides of SemanticNode

            protected override string RebuildNodeText()
            {
                return string.Format("\t\t{0}.{1}{2}", this.LeftHandSide.ToString("B").ToUpper(), this.RightHandSide, Environment.NewLine);
            }

            #endregion
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class GuidAndGuidGlobalSectionLineNode : SemanticNode
        {
            public GuidAndGuidGlobalSectionLineNode(SemanticNode parentNode, PegNode syntaxNode, string fileText)
                : base(parentNode, syntaxNode, fileText)
            {
                var guidNodes = syntaxNode.SubNodes(VsSlnNodeType.guid);

                LeftHandSide = Guid.Parse(guidNodes[0].GetAsString(fileText));
                RightHandSide = Guid.Parse(guidNodes[1].GetAsString(fileText));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public GuidAndGuidGlobalSectionLineNode(SemanticNode parentNode, Guid leftHandSide, Guid rightHandSide)
                : base(parentNode)
            {
                this.LeftHandSide = leftHandSide;
                this.RightHandSide = rightHandSide;

                InvalidateNodeText();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Guid LeftHandSide { get; private set; }
            public Guid RightHandSide { get; private set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Overrides of SemanticNode

            protected override string RebuildNodeText()
            {
                return string.Format(
                    "\t\t{0} = {1}{2}", 
                    this.LeftHandSide.ToString("B").ToUpper(), 
                    this.RightHandSide.ToString("B").ToUpper(), 
                    Environment.NewLine);
            }

            #endregion
        }
    }
}
