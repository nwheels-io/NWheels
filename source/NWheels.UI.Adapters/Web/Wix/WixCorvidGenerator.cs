using System;
using System.Collections.Generic;
using System.Linq;
using MetaPrograms;
using MetaPrograms.Expressions;
using MetaPrograms.Members;
using Microsoft.Build.Logging.StructuredLogger;
using NWheels.Composition.Model.Impl.Metadata;
using NWheels.UI.Model.Impl.Metadata;
using NWheels.UI.Model.Impl.Metadata.Web;
using static MetaPrograms.Fluent.Generator;

namespace NWheels.UI.Adapters.Web.Wix
{
    public class WixCorvidGenerator
    {
        public ModuleMember GenerateCorvidModule(
            TechnologyAdapterContext<WebAppMetadata> context,
            WebAppMetadata.PageItem page)
        {
            var hasBackend = page.Metadata.BackendApis.Any();
            
            return MODULE(new string[0], "corvid", () => {

                INCLUDE("Web.Wix.Code.corvid-correct.js");
                return;
                
                if (hasBackend)
                {
                    IMPORT.TUPLE("fetch", out var @fetch).FROM("wix-fetch");
                    FINAL("endpointUrl", out var @endpointUrl, ANY("https://api.tixlab.app/api/graphql"));
                    INCLUDE("Web.Wix.Code.backend-client.js");
                }

                USE("this.state").ASSIGN(INITOBJECT());

                LocalVariable @pushStateToComps = null;
                GeneratePushStateToComps();
                    
                USE("$w").DOT("onReady").INVOKE(LAMBDA(() => {

                    if (page.Metadata.EventByName.TryGetValue("PageReady", out var listeners))
                    {
                        GenerateEventHandlers(listeners);
                    }

                    foreach (var comp in page.Metadata.Components)
                    {
                        foreach (var eventName in comp.EventByName.Keys)
                        {
                            GenerateEventSubscription(comp, listeners);
                        }
                    }
                    
                }));

                void GeneratePushStateToComps()
                {
                    FINAL("pushStateToComps", out @pushStateToComps, LAMBDA(() => {
                        foreach (var comp in page.Metadata.Components)
                        {
                            if (comp.MapStateToValue != null)
                            {
                                switch (comp)
                                {
                                    case TextContentMetadata text:
                                        USE("$w").INVOKE(ANY($"#{comp.Header.Name}"))
                                            .DOT("text").ASSIGN(comp.MapStateToValue);
                                        break;
                                    case SeatingPlanMetadata seatingPlan:
                                        USE("$w").INVOKE(ANY($"#{comp.Header.Name}"))
                                            .DOT("postMessage").INVOKE(comp.MapStateToValue);
                                        break;
                                }
                            }
                        }
                    }));
                }
                
                void GenerateEventSubscription(UIComponentMetadata compMeta, UIEventMetadata eventMeta)
                {
                    USE("$w").INVOKE(ANY("#" + compMeta.Header.Name.ToString())).DOT("onMessage").INVOKE(LAMBDA(@event => {
                        USE("console").DOT("log").INVOKE(ANY($"got message!"), @event.DOT("data"));
                        GenerateEventHandlers(eventMeta);
                    }));
                    
                }

                void GenerateEventHandlers(UIEventMetadata eventMeta)
                {
                    foreach (var listener in eventMeta.Listeners)
                    {
                        listener.Lambda.INVOKE(USE("event.data"));
                        //GenerateBehaviorSteps(listener.Steps);
                    }
                    
                    USE(@pushStateToComps).INVOKE();
                }

                void GenerateBehaviorSteps(List<UIBehavior> listenerSteps)
                {
                    foreach (var step in listenerSteps)
                    {
                        switch (step)
                        {
                            case UIStateMutationBehavior mutation:
                                USE("this.state")
                                    .DOT(mutation.StatePropertyName)
                                    .ASSIGN(GetValueBehaviorExpression(mutation.NewValue));
                                break;
                        }
                    }
                }

                AbstractExpression GetValueBehaviorExpression(UIBehavior behavior)
                {
                    switch (behavior)
                    {
                        case UIConstantBehavior constant:
                            return new ConstantExpression {Value = constant.Value};
                        case UIStateReadBehavior read:
                            return USE("this.state").DOT(read.StatePropertyName);
                        case UIFetchBehavior fetch:
                            return USE("fetchGraphQL").INVOKE();
                        default:
                            throw new NotSupportedException($"GetValueBehaviorExpression of {behavior.GetType().Name}");
                    }
                }
            });
        }

    }
}