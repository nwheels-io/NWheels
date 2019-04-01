using System;
using System.Collections.Generic;
using Autofac;
using NWheels.Composition.Model.Metadata;

namespace NWheels.Build
{
    public class ModelParserRegistry : IModelParserRegistry
    {
        private readonly List<Action<ContainerBuilder>> _registrations = new List<Action<ContainerBuilder>>();
        
        public void RegisterParsers(ContainerBuilder builder)
        {
            foreach (var registration in _registrations)
            {
                registration(builder);
            }

            Console.WriteLine($"{_registrations.Count} parser(s) registered");
        }

        void IModelParserRegistry.Add<TUnit, TParser>()
        {
            Console.WriteLine($"Registering non-root parser {typeof(TParser).FullName}");
            
            _registrations.Add(builder => builder
                .RegisterType<TParser>()
                .AsSelf()
                .As<ModelParser<TUnit>, IModelParser<TUnit>, IAnyModelParser>()
                .InstancePerDependency());
        }

        void IModelParserRegistry.AddRoot<TUnit, TParser>()
        {
            Console.WriteLine($"Registering root parser {typeof(TParser).FullName}");

            _registrations.Add(builder => builder
                .RegisterType<TParser>()
                .AsSelf()
                .As<ModelParser<TUnit>, IModelParser<TUnit>, IAnyModelParser>()
                .As<IRootModelParser<TUnit>, IAnyRootModelParser>()
                .InstancePerDependency());
        }
    }
}
