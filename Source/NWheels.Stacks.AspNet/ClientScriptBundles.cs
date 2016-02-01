using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace NWheels.Stacks.AspNet
{
    public class ClientScriptBundles
    {
        private readonly IWebModuleContext _context;
        private readonly string _jsBundleUrlPath;
        private readonly string _cssBundleUrlPath;
        private readonly Func<string, string> _mapPath;
        private List<FileEntry> _jsFiles;
        private List<FileEntry> _cssFiles;
        private string _jsBundle;
        private string _cssBundle;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ClientScriptBundles(IWebModuleContext context, string jsBundleUrlPath, string cssBundleUrlPath, Func<string, string> mapPath)
        {
            _jsFiles = new List<FileEntry>();
            _cssFiles = new List<FileEntry>();
            _context = context;
            _jsBundleUrlPath = jsBundleUrlPath;
            _cssBundleUrlPath = cssBundleUrlPath;
            _mapPath = mapPath;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string ProcessHtml(string html)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            ReplaceJsLinks(htmlDoc);
            ReplaceCssLinks(htmlDoc);
            
            return ToHtmlString(htmlDoc);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void BuildBundles()
        {
            _jsBundle = BuildBundle(_jsBundleUrlPath, _jsFiles);
            _cssBundle = BuildBundle(_cssBundleUrlPath, _cssFiles, transform: RebaseUrlsInCssFile);

            _jsFiles = null;
            _cssFiles = null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string JsBundle
        {
            get { return _jsBundle; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string CssBundle
        {
            get { return _cssBundle; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private string BuildBundle(string bundleUrlPath, List<FileEntry> fileList, FileTransformCallback transform = null)
        {
            var bundle = new StringBuilder();

            foreach ( var file in fileList )
            {
                bundle.AppendLine();
                bundle.AppendLine("/* INCLUDE [" + file.FilePath + "] */");

                if ( File.Exists(file.FilePath) )
                {
                    var fileContents = File.ReadAllText(file.FilePath);
                    
                    if ( transform != null )
                    {
                        fileContents = transform(bundleUrlPath, file.UrlPath, fileContents);
                    }

                    bundle.Append(fileContents);
                }
                else
                {
                    bundle.Append("/* warning: file not found */");
                }
            }

            return bundle.ToString();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ReplaceCssLinks(HtmlDocument htmlDoc)
        {
            ReplaceLinks(htmlDoc, "//link[@href and @rel='stylesheet']", "href", _cssFiles, _cssBundleUrlPath);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ReplaceJsLinks(HtmlDocument htmlDoc)
        {
            ReplaceLinks(htmlDoc, "//script[@src]", "src", _jsFiles, _jsBundleUrlPath);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ReplaceLinks(
            HtmlDocument htmlDoc, 
            string xpath, 
            string pathAttributeName, 
            List<FileEntry> destinationFiles,
            string bundleUrlPath)
        {
            var firstLink = true;
            var allLinks = htmlDoc.DocumentNode.SelectNodes(xpath).ToArray();

            foreach ( var link in allLinks )
            {
                var pathAttribute = link.Attributes[pathAttributeName];

                if ( pathAttribute != null )
                {
                    var mappedPath = _mapPath(pathAttribute.Value);

                    if ( mappedPath == null )
                    {
                        continue;
                    }

                    destinationFiles.Add(new FileEntry(
                        urlPath: pathAttribute.Value, 
                        filePath: mappedPath));

                    if ( firstLink )
                    {
                        pathAttribute.Value = bundleUrlPath;
                        firstLink = false;
                    }
                    else
                    {
                        link.Remove();
                    }
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private string ToHtmlString(HtmlDocument htmlDoc)
        {
            var output = new StringBuilder();
            var writer = new StringWriter(output);
            htmlDoc.Save(writer);
            writer.Flush();
            return output.ToString();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly Regex _s_cssUrlRegex = new Regex(
            @"url\s*\(\s*['""]?(?!\s*data\s*\:\s*image)(?!https?\:)\s*([^'""\)]+)\s*['""]?\s*\)",
            RegexOptions.IgnoreCase);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static string RebaseUrlsInCssFile(string bundleUrlPath, string fileUrlPath, string fileContents)
        {
            var bundleUrlPathParts = bundleUrlPath.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            var fileUrlPathParts = fileUrlPath.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);

            var rebasedContents = _s_cssUrlRegex.Replace(
                fileContents,
                match => {
                    if ( match.Groups.Count == 2 )
                    {
                        var resourceUrlPath = match.Groups[1].Value;
                        var resourceUrlPathParts = resourceUrlPath.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
                        var rebasedPath = RebaseResourceUrlPath(resourceUrlPathParts, fileUrlPathParts, bundleUrlPathParts);
                        var matchValueToReplaceWith = "url('" + rebasedPath + "')";

                        //File.AppendAllText(
                        //    @"C:\Temp\css-url-regex.log", 
                        //    string.Format("[{0}] -> [{1}] -> [{2}] -> [{3}]", match.Value, resourceUrlPath, rebasedPath, matchValueToReplaceWith) + Environment.NewLine);

                        return matchValueToReplaceWith;
                    }
                    else
                    {
                        return match.Value;
                    }
                });

            return rebasedContents;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private delegate string FileTransformCallback(string bundleUrlPath, string fileUrlPath, string fileContents);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static string RebaseResourceUrlPath(string[] resourceUrlParts, string[] fileUrlParts, string[] bundleUrlParts)
        {
            var rebasedResourceUrlParts = new List<string>();
            var commonPrefixLength = fileUrlParts.TakeWhile((part, index) => index < bundleUrlParts.Length - 1 && part == bundleUrlParts[index]).Count();

            rebasedResourceUrlParts.AddRange(bundleUrlParts.Skip(commonPrefixLength).Take(bundleUrlParts.Length - commonPrefixLength - 1).Select(x => ".."));
            rebasedResourceUrlParts.AddRange(fileUrlParts.Skip(commonPrefixLength).Take(fileUrlParts.Length - commonPrefixLength - 1));
            rebasedResourceUrlParts.AddRange(resourceUrlParts);

            var rebasedResourceUrl = string.Join("/", rebasedResourceUrlParts);
            return rebasedResourceUrl;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class FileEntry
        {
            public FileEntry(string urlPath, string filePath)
            {
                this.UrlPath = urlPath;
                this.FilePath = filePath;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string UrlPath { get; private set; }
            public string FilePath { get; private set; }
        }
    }
}
