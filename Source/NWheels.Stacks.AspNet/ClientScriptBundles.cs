using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
        private List<string> _jsFilePaths;
        private List<string> _cssFilePaths;
        private string _jsBundle;
        private string _cssBundle;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ClientScriptBundles(IWebModuleContext context, string jsBundleUrlPath, string cssBundleUrlPath, Func<string, string> mapPath)
        {
            _jsFilePaths = new List<string>();
            _cssFilePaths = new List<string>();
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
            
            _jsBundle = BuildBundle(_jsFilePaths);
            _cssBundle = BuildBundle(_cssFilePaths);

            return ToHtmlString(htmlDoc);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void BuildBundles()
        {
            _jsBundle = BuildBundle(_jsFilePaths);
            _cssBundle = BuildBundle(_cssFilePaths);

            _jsFilePaths = null;
            _cssFilePaths = null;
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

        private string BuildBundle(List<string> filePaths)
        {
            var bundle = new StringBuilder();

            foreach ( var filePath in filePaths )
            {
                bundle.AppendLine();
                bundle.AppendLine("/* INCLUDE FILE [" + filePath + "] */");

                if ( File.Exists(filePath) )
                {
                    bundle.Append(File.ReadAllText(filePath));
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
            ReplaceLinks(htmlDoc, "//link[@href and @rel='stylesheet']", "href", _cssFilePaths, _cssBundleUrlPath);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ReplaceJsLinks(HtmlDocument htmlDoc)
        {
            ReplaceLinks(htmlDoc, "//script[@src]", "src", _jsFilePaths, _jsBundleUrlPath);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ReplaceLinks(
            HtmlDocument htmlDoc, 
            string xpath, 
            string pathAttributeName, 
            List<string> destinationFilePaths,
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

                    destinationFilePaths.Add(mappedPath);

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
    }
}
