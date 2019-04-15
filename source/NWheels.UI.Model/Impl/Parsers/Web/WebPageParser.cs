using System;
using System.Linq;
using System.Runtime.InteropServices;
using MetaPrograms.Expressions;
using MetaPrograms.Members;
using MetaPrograms.Statements;
using NWheels.Composition.Model.Impl.Metadata;
using NWheels.Composition.Model.Impl.Parsers;
using NWheels.UI.Model.Impl.Metadata;
using NWheels.UI.Model.Impl.Metadata.Web;
using NWheels.UI.Model.Web;

namespace NWheels.UI.Model.Impl.Parsers.Web
{
    public class WebPageParser : IModelParser, IModelParserWithInit
    {
        private PropertyParsersMap _propParsers;
        
        public void Initialize(IModelPreParserContext context)
        {
            _propParsers = new PropertyParsersMap(context);
            _propParsers.RegisterParserMethods(new ComponentParsers());
            _propParsers.RegisterParserMethods(new WebComponentParsers());
        }

        public MetadataObject CreateMetadataObject(IModelPreParserContext context)
        {
            return new WebPageMetadata(context.Input);
        }

        public void Parse(IModelParserContext context)
        {
            var pageMeta = (WebPageMetadata) context.Output;

            pageMeta.Title = ParseTitle();
            ParseProperties();
            ParseConstructor();
            
            string ParseTitle()
            {
                return context.Input.ConcreteType.TryGetPropertyValue<string>(nameof(WebPage.Title));
            }

            void ParseProperties()
            {
                foreach (var prop in context.Input.GetAllProperties())
                {
                    var parser = _propParsers.GetParser(prop);
                    var propMeta = parser(prop, context);

                    switch (propMeta)
                    {
                        case UIComponentMetadata compMeta:
                            pageMeta.Components.Add(compMeta);
                            break;
                        case IBackendApiMetadata apiMeta:
                            pageMeta.BackendApis.Add(apiMeta);
                            break;
                        default:
                            pageMeta.UnknownChildren.Add(propMeta);
                            break;
                    }
                }
            }

            void ParseConstructor()
            {
                var constructor = context.Input.ConcreteType.Members.OfType<ConstructorMember>().FirstOrDefault();
                if (constructor == null)
                {
                    return;
                }

                foreach (var statement in constructor.Body.Statements.OfType<ExpressionStatement>())
                {
                    if (statement.Expression is AssignmentExpression assignment)
                    {
                        if (assignment.Left is MemberExpression eventMember &&
                            eventMember.Target is AbstractExpression compMember && 
                            assignment.Right is AnonymousDelegateExpression lambda)
                        {
                            ParseEventHandler(compMember, eventMember, lambda);
                        }
                    }
                }

                void ParseEventHandler(
                    AbstractExpression compMember, 
                    MemberExpression eventMember, 
                    AnonymousDelegateExpression lambda)
                {
                    var targetMeta = FindEventTarget(compMember, eventMember);

                    if (!targetMeta.EventByName.TryGetValue(eventMember.Name, out var eventMeta))
                    {
                        eventMeta = new UIEventMetadata {
                            Name = eventMember.Name
                        };
                        targetMeta.EventByName[eventMember.Name] = eventMeta;
                    }
                    
                    Console.WriteLine($"CTOR > ASSIGN > EVENT > {targetMeta.Header.Name}::{eventMeta.Name}");
                    eventMeta.Listeners.Add(ParseFunction(lambda));
                }

                UIComponentMetadata FindEventTarget(AbstractExpression comp, MemberExpression @event)
                {
                    UIComponentMetadata result = null;
                    
                    if (comp is ThisExpression)
                    {
                        result = pageMeta;
                    }

                    if (comp is MemberExpression member)
                    {
                        result = pageMeta.Components.FirstOrDefault(c => c.Header.Name == member.Name);
                    }

                    return result
                        ?? throw new ArgumentException($"Cannot find event target for event '{@event.Name}'.");
                }
                
                UIFunctionMetadata ParseFunction(AnonymousDelegateExpression lambda)
                {
                    var result = new UIFunctionMetadata();
                    result.Lambda = lambda;
                    
                    foreach (var statement in lambda.Body.Statements.OfType<ExpressionStatement>())
                    {
                        if (statement.Expression is AssignmentExpression assignment &&
                            assignment.Left is MemberExpression member)
                        {
                            var step = new UIStateMutationBehavior {
                                StatePropertyName = member.Name,
                                NewValue = ParseValueBehavior(assignment.Right)
                            };
                            result.Steps.Add(step);
                        }
                    }

                    return result;
                }
                
                UIBehavior ParseValueBehavior(AbstractExpression expression)
                {
                    if (expression is ConstantExpression constant)
                    {
                        return new UIConstantBehavior {
                            Value = constant.Value
                        };
                    }

                    if (expression is MemberExpression member)
                    {
                        return new UIStateReadBehavior {
                            StatePropertyName = member.Name
                        };
                    }

                    if (expression is ParameterExpression parameter)
                    {
                        return new UIConstantBehavior {
                            Value = 12345
                        };
                    }

                    if (expression is AwaitExpression awaitExpr &&
                        awaitExpr.Expression is MethodCallExpression call)
                    {
                        return new UIFetchBehavior {
                            BackendApiName = call.Method.DeclaringType.Name,
                            OperationName = call.Method.Name,
                            Arguments = call.Arguments
                                .Select(arg => ParseValueBehavior(arg.Expression))
                                .ToList()
                        };
                    }
                 
                    throw new ArgumentException($"Cannot recognize UI behavior of type '{expression.GetType().Name}'.");
                }
            }
        }
    }
}
