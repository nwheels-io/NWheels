using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Members;
using NWheels.Concurrency;
using NWheels.Endpoints.Factories;
using NWheels.Exceptions;

namespace NWheels.Endpoints
{
    public static class DuplexNetworkApi
    {
        public abstract class SessionBehaviorAttributeBase : Attribute
        {
            protected SessionBehaviorAttributeBase(SessionBehaviorType behavior)
            {
                this.Behavior = behavior;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public SessionBehaviorType Behavior { get; private set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [AttributeUsage(AttributeTargets.Method)]
        public class OpenSessionAttribute : SessionBehaviorAttributeBase
        {
            public OpenSessionAttribute()
                : base(SessionBehaviorType.OpenAnonymousDropIfNotAuthenticated)
            {
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [AttributeUsage(AttributeTargets.Method | AttributeTargets.Event)]
        public class RequireSessionAttribute : SessionBehaviorAttributeBase
        {
            public RequireSessionAttribute()
                : base(SessionBehaviorType.RequireAuthenticated)
            {
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [AttributeUsage(AttributeTargets.Method | AttributeTargets.Event)]
        public class CloseSessionAttribute : SessionBehaviorAttributeBase
        {
            public CloseSessionAttribute()
                : base(SessionBehaviorType.GracefullyClose)
            {
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public enum SessionBehaviorType
        {
            Ignore,
            Require,
            RequireAuthenticated,
            GlobalAnonymous,
            GlobalSystem,
            OpenAnonymous,
            OpenAnonymousDropIfNotAuthenticated,
            GracefullyClose,
            Abort
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static class CurrentCall
        {
            private static readonly Disposable _s_disposable = new Disposable();

            [ThreadStatic]
            private static IDuplexNetworkEndpointApiProxy _s_currentClientProxy;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static TRemoteApi GetRemotePartyAs<TRemoteApi>()
            {
                return (TRemoteApi)ClientProxy;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            internal static IDuplexNetworkEndpointApiProxy ClientProxy
            {
                get
                {
                    return _s_currentClientProxy;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            internal static IDisposable UseClientProxy(IDuplexNetworkEndpointApiProxy proxy)
            {
                _s_currentClientProxy = proxy;
                return _s_disposable;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private class Disposable : IDisposable
            {
                public void Dispose()
                {
                    _s_currentClientProxy = null;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ContractDescription
        {
            public ContractDescription(Type contractType)
            {
                if (contractType == null)
                {
                    throw new ArgumentNullException("contractType");
                }
                
                if (!contractType.IsInterface)
                {
                    throw new ArgumentException("RPC contract must be an interface", "contractType");
                }

                this.ContractType = contractType;

                var members = TypeMemberCache.Of(contractType);
                var methodMembers = members.SelectAllMethods().ToArray().Select(m => new MemberDescription(this, m)).ToList();
                var eventMembers = members.SelectAllEvents().ToArray().Select(e => new MemberDescription(this, e)).ToList();

                this.Methods = methodMembers.ToDictionary(x => x.MethodInfo);
                this.Events = eventMembers.ToDictionary(x => x.EventInfo);
                this.SessionInitiators = methodMembers.Concat(eventMembers).Where(m => m.IsSessionInitiator).ToList();
                this.SessionTerminators = methodMembers.Concat(eventMembers).Where(m => m.IsSessionTerminator).ToList();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Type ContractType { get; private set; }
            public IReadOnlyDictionary<MethodInfo, MemberDescription> Methods { get; private set; }
            public IReadOnlyDictionary<EventInfo, MemberDescription> Events { get; private set; }
            public IReadOnlyList<MemberDescription> SessionInitiators { get; private set; }
            public IReadOnlyList<MemberDescription> SessionTerminators { get; private set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class MemberDescription
        {
            public MemberDescription(ContractDescription declaringContract, MethodInfo member)
            {
                this.DeclaringContract = declaringContract;
                ValidateRemotableMethod(member);

                this.MethodInfo = member;
                this.EventInfo = null;
                this.IsOneWay = member.IsVoid();

                if (!member.IsVoid() && member.ReturnType.IsGenericType && member.ReturnType.GetGenericTypeDefinition() == typeof(Promise<>))
                {
                    this.PromiseResultType = member.ReturnType.GetGenericArguments()[0];
                }

                DetermineSessionBehavior(member);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public MemberDescription(ContractDescription declaringContract, EventInfo member)
            {
                this.DeclaringContract = declaringContract;
                ValidateRemotableEvent(member);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ContractDescription DeclaringContract { get; private set; }
            public MethodInfo MethodInfo { get; private set; }
            public EventInfo EventInfo { get; private set; }
            public Type PromiseResultType { get; private set; }
            public bool IsOneWay { get; private set; }
            public bool IsSessionInitiator { get; private set; }
            public bool IsSessionTerminator { get; private set; }
            public SessionBehaviorType SessionBehavior { get; private set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ValidateRemotableMethod(MethodInfo methodInfo)
            {
                if (!methodInfo.IsVoid() && !typeof(IAnyPromise).IsAssignableFrom(methodInfo.ReturnType))
                {
                    throw NewContractConventionException(methodInfo, "Method must either be void or return a Promise");
                }

                if (methodInfo.GetParameters().Any(p => p.IsOut || p.ParameterType.IsByRef))
                {
                    throw NewContractConventionException(methodInfo, "Method cannot have ref or out parameters");
                }

                if (methodInfo.IsGenericMethod && methodInfo.IsGenericMethodDefinition)
                {
                    throw NewContractConventionException(methodInfo, "Open generic methods are not supported");
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ValidateRemotableEvent(EventInfo eventInfo)
            {
                throw NewContractConventionException(eventInfo, "Events are not yet supported");
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void DetermineSessionBehavior(MemberInfo member)
            {
                var attribute = member.GetCustomAttribute<SessionBehaviorAttributeBase>();

                if (attribute != null)
                {
                    this.SessionBehavior = attribute.Behavior;
                }

                this.IsSessionInitiator = (
                    this.SessionBehavior == SessionBehaviorType.OpenAnonymous || 
                    this.SessionBehavior == SessionBehaviorType.OpenAnonymousDropIfNotAuthenticated);

                this.IsSessionTerminator = (
                    this.SessionBehavior == SessionBehaviorType.GracefullyClose ||
                    this.SessionBehavior == SessionBehaviorType.Abort);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private ContractConventionException NewContractConventionException(MemberInfo member, string message)
            {
                return new ContractConventionException(
                    typeof(DuplexNetworkApiProxyFactory.ProxyConvention),
                    DeclaringContract.ContractType,
                    member,
                    message);
            }
        }
    }
}
