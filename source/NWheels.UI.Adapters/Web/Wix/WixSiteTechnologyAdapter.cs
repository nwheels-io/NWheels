using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using MetaPrograms;
using MetaPrograms.CSharp.Reader.Reflection;
using MetaPrograms.Expressions;
using MetaPrograms.JavaScript;
using MetaPrograms.JavaScript.Fluent;
using MetaPrograms.JavaScript.Writer;
using Microsoft.CodeAnalysis;
using NWheels.Composition.Model.Impl.Metadata;
using NWheels.UI.Model.Impl.Metadata;
using NWheels.UI.Model.Impl.Metadata.Web;
using static MetaPrograms.Fluent.Generator;

namespace NWheels.UI.Adapters.Web.Wix
{
    public class WixSiteTechnologyAdapter : TechnologyAdapter<WebAppMetadata>
    {
        protected override void GenerateOutputs(TechnologyAdapterContext<WebAppMetadata> context)
        {
            var codeWriter = new JavaScriptCodeWriter(context.Output);

            using (CreateCodeContext())
            {
                foreach (var page in context.Input.Pages)
                {
                    GeneratePage(page);
                }
            }

            IDisposable CreateCodeContext()
            {
                return new CodeGeneratorContext(
                    context.Preprocessor.Code,
                    new ClrTypeResolver(),
                    LanguageInfo.Entries.JavaScript());
            }

            void GeneratePage(WebAppMetadata.PageItem page)
            {
                var module = MODULE(new[] { "pages" }, page.Name, () => {

                    FINAL("pageName", out var @pageName, ANY($"{page.Name}"));

                    GenerateCorvid();
                    GenerateComponents();
                    GenerateSave();

                    void GenerateComponents()
                    {
                        FINAL("comps", out var @compsArray, NEWARRAY(
                            page.Metadata.Components
                                .Select(GenerateComponentEntry)
                                .Select(JavaScriptGenerator.JSON)
                                .ToArray()
                        ));
                    }

                    WixComponentEntry GenerateComponentEntry(UIComponentMetadata comp)
                    {
                        var generator = WixComponentGenerator.GetGenerator(comp);

                        return new WixComponentEntry {
                            CompDef = generator.GenerateDefinition(),
                            Html = generator.GenerateHtml()
                        };
                    }
                    
                    void GenerateCorvid()
                    {
                        var corvidOutput = new StringCodeGeneratorOutput(context.Output.TextOptions);
                        var corvidWriter = new JavaScriptCodeWriter(corvidOutput);

                        using (CreateCodeContext())
                        {
                            var corvidGenerator = new WixCorvidGenerator();
                            var corvidModule = corvidGenerator.GenerateCorvidModule(context, page);                           
                            corvidWriter.WriteModule(corvidModule);
                        }

                        FINAL("wixCode", out var @wixCode, INTERPOLATE(corvidOutput.GetString()));
                    }

                    void GenerateSave()
                    {
                        INCLUDE("Web.Wix.Code.upload.js");
                    }
                });
                
                codeWriter.WriteModule(module, privateScope: true);
            }
        }

    }
}
