//using System;
//using System.Collections.Generic;
//using Autofac;
//using NWheels.Composition.Model.Metadata;
//
//namespace NWheels.Build
//{
//    public class ModelParserRegistry : IModelParserRegistry
//    {
//        private readonly List<Action<ContainerBuilder>> _registrations = new List<Action<ContainerBuilder>>();
//        
//        public void RegisterParsers(ContainerBuilder builder)
//        {
//            foreach (var registration in _registrations)
//            {
//                registration(builder);
//            }
//
//            Console.WriteLine($"{_registrations.Count} parser(s) registered");
//        }
//
//        void IModelParserRegistry.RegisterParser<TParser>()
//        {
//            Console.WriteLine($"Registering non-root parser {typeof(TParser).FullName}");
//            
//            _registrations.Add(builder => builder
//                .RegisterType<TParser>()
//                .AsSelf()
//                .As<IModelParser>()
//                .InstancePerDependency());
//        }
//
//        void IModelParserRegistry.RegisterRootParser<TParser>()
//        {
//            Console.WriteLine($"Registering root parser {typeof(TParser).FullName}");
//
//            _registrations.Add(builder => builder
//                .RegisterType<TParser>()
//                .AsSelf()
//                .As<IModelParser, IRootModelParser>()
//                .InstancePerDependency());
//        }
//    }
//}
