using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using NWheels.Extensions;

namespace NWheels.Testing.BehaviorDriven
{
    public abstract class ExecutableSpecification
    {
        public void Execute(IComponentContext components)
        {
            var context = new SpecificationContext(
                components.Resolve<IFramework>(), 
                components,
                components.Resolve<ISpecificationLogger>());

            using ( var activity = context.Logger.TestingSpecification(this.ToString()) )
            {
                try
                {
                    Execute(context);
                    context.Logger.SpecificationPassed(this.ToString());
                }
                catch ( Exception e )
                {
                    activity.Fail(e);
                    context.Logger.SpecificationFailed(this.ToString(), e);
                    throw;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Dependency]
        protected IFramework Framework { get; set; }
        [Dependency]
        protected IComponentContext Components { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void Execute(SpecificationContext context)
        {
            context.Logger.Running(this.ToString());
            InjectDependencies(context);

            try
            {
                ExecuteMembers(context);
                context.Logger.Passed(this.ToString());
            }
            catch ( Exception e )
            {
                context.Logger.Failed(this.ToString(), e);
                throw;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ExecuteMembers(SpecificationContext context)
        {
            MethodInfo initMethod;
            MethodInfo doneMethod;
            DiscoverLifecycleMembers(out initMethod, out doneMethod);

            SpecificationStep[] givenSteps;
            SpecificationStep[] whenSteps;
            SpecificationStep[] thenSteps;
            Compile(out givenSteps, out whenSteps, out thenSteps);

            if ( initMethod != null )
            {
                initMethod.Invoke(this, ResolveMethodDependencies(initMethod, context));
            }

            try
            {
                ExecuteSteps(context, givenSteps, failOnFirstError: true);
                ExecuteSteps(context, whenSteps, failOnFirstError: true);
                ExecuteSteps(context, thenSteps, failOnFirstError: false);
            }
            finally
            {
                if ( doneMethod != null )
                {
                    doneMethod.Invoke(this, ResolveMethodDependencies(doneMethod, context));
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void InjectDependencies(SpecificationContext context)
        {
            var dependencyProperties = DiscoverDependencyProperties();

            foreach ( var property in dependencyProperties )
            {
                if ( context.ExecutedSpecificationByType.ContainsKey(property.PropertyType) )
                {
                    property.SetValue(this, context.ExecutedSpecificationByType[property.PropertyType]);
                }
                else
                {
                    property.SetValue(this, context.Components.Resolve(property.PropertyType));
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ExecuteSteps(SpecificationContext context, SpecificationStep[] steps, bool failOnFirstError = true)
        {
            var exceptions = new List<Exception>();

            foreach ( var step in steps )
            {
                try
                {
                    //context.Logger.Executing(step.ToString());
                    step.Execute(context);
                    context.Logger.Passed(step.ToString());
                }
                catch ( Exception e )
                {
                    context.Logger.Failed(step.ToString(), exception: e);

                    if ( failOnFirstError )
                    {
                        throw;
                    }
                    else
                    {
                        exceptions.Add(e);
                    }
                }
            }

            if ( exceptions.Count > 0 )
            {
                throw new AggregateException(exceptions).Flatten();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void Compile(out SpecificationStep[] givenSteps, out SpecificationStep[] whenSteps, out SpecificationStep[] thenSteps)
        {
            var clauses = DiscoverClauseMembers();

            givenSteps = CompileSteps(clauses.Where(c => c.Item1.Clause == ClauseType.Given));
            whenSteps = CompileSteps(clauses.Where(c => c.Item1.Clause == ClauseType.When));
            thenSteps = CompileSteps(clauses.Where(c => c.Item1.Clause == ClauseType.Then));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private SpecificationStep[] CompileSteps(IEnumerable<Tuple<ClauseAttributeBase, MemberInfo>> members)
        {
            var steps = new List<SpecificationStep>();

            foreach ( var member in members.OrderBy(m => m.Item1.Index) )
            {
                steps.Add(CompileStep(member));
            }

            return steps.ToArray();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private SpecificationStep CompileStep(Tuple<ClauseAttributeBase, MemberInfo> member)
        {
            var method = member.Item2 as MethodInfo;
            var property = member.Item2 as PropertyInfo;

            if ( method != null )
            {
                return new MethodStep(member.Item1.Clause, member.Item1.Index, this, method);
            }
            
            if ( property != null )
            {
                return new PropertyStep(member.Item1.Clause, member.Item1.Index, this, property);
            }

            throw new NotSupportedException(member.Item2.GetType().Name);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Tuple<ClauseAttributeBase, MemberInfo>[] DiscoverClauseMembers()
        {
            var clauseMembers = this.GetType()
                .GetMembers()
                .Where(m => m.HasAttribute<ClauseAttributeBase>())
                .Select(m => new Tuple<ClauseAttributeBase, MemberInfo>(m.GetCustomAttribute<ClauseAttributeBase>(), m))
                .OrderBy(m => m.Item1.Clause)
                .ThenBy(m => m.Item1.Index);

            return clauseMembers.ToArray();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void DiscoverLifecycleMembers(out MethodInfo initMethod, out MethodInfo doneMethod)
        {
            initMethod = this.GetType().GetMethods().FirstOrDefault(m => m.HasAttribute<InitAttribute>());
            doneMethod = this.GetType().GetMethods().FirstOrDefault(m => m.HasAttribute<DoneAttribute>());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private PropertyInfo[] DiscoverDependencyProperties()
        {
            var dependencyProperties = this.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(p => p.HasAttribute<DependencyAttribute>());

            return dependencyProperties.ToArray();
        }


        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private static object[] ResolveMethodDependencies(MethodInfo method, SpecificationContext context)
        {
            var formalParameters = method.GetParameters();
            var actualArguments = new object[formalParameters.Length];

            for ( int i = 0 ; i < formalParameters.Length ; i++ )
            {
                actualArguments[i] = context.Components.Resolve(formalParameters[i].ParameterType);
            }

            return actualArguments;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [AttributeUsage(AttributeTargets.Property)]
        public class DependencyAttribute : Attribute
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [AttributeUsage(AttributeTargets.Method)]
        public class InitAttribute : Attribute
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [AttributeUsage(AttributeTargets.Method)]
        public class DoneAttribute : Attribute
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
        public abstract class ClauseAttributeBase : Attribute
        {
            protected ClauseAttributeBase(int index)
            {
                Index = index;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public int Index { get; private set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            internal abstract ClauseType Clause { get; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
        public class GivenAttribute : ClauseAttributeBase
        {
            public GivenAttribute(int index)
                : base(index)
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            internal override ClauseType Clause
            {
                get { return ClauseType.Given; }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
        public class WhenAttribute : ClauseAttributeBase
        {
            public WhenAttribute(int index)
                : base(index)
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            internal override ClauseType Clause
            {
                get { return ClauseType.When; }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
        public class ThenAttribute : ClauseAttributeBase
        {
            public ThenAttribute(int index = 1)
                : base(index)
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            internal override ClauseType Clause
            {
                get { return ClauseType.Then; }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal enum ClauseType
        {
            Given = 1,
            When = 2,
            Then = 3
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal abstract class SpecificationStep
        {
            protected SpecificationStep(ClauseType clause, int index, ExecutableSpecification target)
            {
                Clause = clause;
                Index = index;
                Target = target;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public abstract void Execute(SpecificationContext context);

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ClauseType Clause { get; private set; }
            public int Index { get; private set; }
            public ExecutableSpecification Target { get; private set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal class MethodStep : SpecificationStep
        {
            public MethodStep(ClauseType type, int index, ExecutableSpecification target, MethodInfo method)
                : base(type, index, target)
            {
                Method = method;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override void Execute(SpecificationContext context)
            {
                var arguments = ResolveMethodDependencies(this.Method, context);
                Method.Invoke(this.Target, arguments);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override string ToString()
            {
                return string.Format("{0}.{1}[{2}] : {3}", this.Target.GetType().Name, this.Clause, this.Index, this.Method.Name.SplitPascalCase());
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public MethodInfo Method { get; private set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal class PropertyStep : SpecificationStep
        {
            public PropertyStep(ClauseType type, int index, ExecutableSpecification target, PropertyInfo property)
                : base(type, index, target)
            {
                Property = property;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override void Execute(SpecificationContext context)
            {
                if ( context.ExecutedSpecificationByType.ContainsKey(Property.PropertyType) )
                {
                    var executedSpecification = context.ExecutedSpecificationByType[this.Property.PropertyType];
                    Property.SetValue(this.Target, executedSpecification);
                    context.Logger.Reusing(executedSpecification.ToString());
                }
                else
                {
                    var newSpecification = CreateSpecificationInstance(context);
                    newSpecification.Execute(context);
                    context.ExecutedSpecificationByType.Add(Property.PropertyType, newSpecification);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override string ToString()
            {
                return string.Format("{0}.{1}[{2}] : {3}", this.Target.GetType().Name, this.Clause, this.Index, this.Property.Name.SplitPascalCase());
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public PropertyInfo Property { get; private set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private ExecutableSpecification CreateSpecificationInstance(SpecificationContext context)
            {
                var instance = (ExecutableSpecification)Activator.CreateInstance(this.Property.PropertyType);

                instance.Components = context.Components;
                instance.Framework = context.Framework;

                this.Property.SetValue(base.Target, instance);

                return instance;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        internal class SpecificationContext
        {
            public SpecificationContext(IFramework framework, IComponentContext components, ISpecificationLogger logger)
            {
                this.Framework = framework;
                this.Components = components;
                this.Logger = logger;
                this.ExecutedSpecificationByType = new Dictionary<Type, ExecutableSpecification>();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IFramework Framework { get; private set; }
            public IComponentContext Components { get; private set; }
            public ISpecificationLogger Logger { get; private set; }
            public Dictionary<Type, ExecutableSpecification> ExecutedSpecificationByType { get; private set; }
        }
    }
}
