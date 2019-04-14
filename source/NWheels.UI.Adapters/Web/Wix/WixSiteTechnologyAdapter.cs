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

                    var printName = page.Name.ToString(CasingStyle.Kebab);
                    USE("console").DOT("log").INVOKE(ANY($"--- generating page: {printName} ---"));
                    FINAL("pageName", out var @pageName, ANY($"{page.Name}"));

                    GenerateComponents();
                    GenerateCorvid();
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
                            Html = generator.GenerateHtml()?.ToString()
                        };
                    }
                    
                    void GenerateCorvid()
                    {
                        var corvidOutput = new StringCodeGeneratorOutput(context.Output.TextOptions);
                        var corvidWriter = new JavaScriptCodeWriter(corvidOutput);

                        using (CreateCodeContext())
                        {
                            var corvidModule = MODULE(new string[0], "corvid", () => {
                                IMPORT.TUPLE("fetch", out var @fetch).FROM("wix-fetch");
                                USE("$w").DOT("onReady").INVOKE(LAMBDA(() => {
                                    USE("$w").INVOKE(ANY("#html1")).DOT("onMessage").INVOKE(LAMBDA(@event => {
                                        USE("console").DOT("log").INVOKE(ANY($"got message!"), @event.DOT("data"));
                                    }));
                                }));
                            });
                            
                            corvidWriter.WriteModule(corvidModule);
                        }

                        FINAL("wixCode", out var @wixCode, INTERPOLATE(corvidOutput.GetString()));
                    }

                    void GenerateSave()
                    {
                        FINAL("getCodeAsString", out var @getCodeAsString, LAMBDA(@func => {
                            FINAL("funcStr", out var @funcStr, @func.DOT("toString"));
                            DO.IF(@funcStr.DOT("indexOf").INVOKE(ANY("() =>")).EQ(ANY(0))).THEN(() => {
                                DO.RETURN(@funcStr.DOT("substring").INVOKE(ANY(6)));
                            });
                            DO.RETURN(@funcStr);
                        }));
                    }
                });
                
                codeWriter.WriteModule(module);
            }
        }

    }
}
