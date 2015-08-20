using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NWheels.Domains.Security.Core;
using NWheels.Domains.Security.Impl;
using NWheels.Domains.Security.UI;
using NWheels.Extensions;

namespace NWheels.Domains.Security
{
    public class ModuleLoader : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.NWheelsFeatures().Logging().RegisterLogger<ISecurityDomainLogger>();
            builder.NWheelsFeatures().Entities().RegisterDataRepository<IUserAccountDataRepository>();

            builder.RegisterType<DummyCryptoProvider>().As<ICryptoProvider>().SingleInstance();
            builder.RegisterType<PrivateAuthenticationProvider>().As<IAuthenticationProvider>().SingleInstance();
            builder.RegisterType<ClaimFactory>().SingleInstance();

            builder.RegisterType<UserLoginTransactionScript>().SingleInstance();
            builder.RegisterType<SecurityDomainApi>().As<ISecurityDomainApi>().SingleInstance();

            builder.RegisterType<UserAccountPolicySet>();

            builder.NWheelsFeatures().ObjectContracts().Concretize<IUserAccountEntity>().With<UserAccountEntity>();
        }
    }
}
