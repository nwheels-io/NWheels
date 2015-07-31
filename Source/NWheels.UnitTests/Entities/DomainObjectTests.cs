using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NUnit.Framework;
using NWheels.Testing;

namespace NWheels.UnitTests.Entities
{
    [TestFixture]
    public class DomainObjectTests : UnitTestBase
    {
        [Test]
        public void Example_RenewContract()
        {
            //- arrange

            #region Simulate existing data data
            GeneratedDomainObject_ContractEntity contractInDb = new GeneratedDomainObject_ContractEntity(Framework);
            contractInDb.IsApproved_PersistenceWrapper = true;
            contractInDb.Expiration_PersistenceWrapper = new DateTime(2015, 5, 30);
            contractInDb.Term = ContractTermType.Monthly;
            #endregion

            // contract = db.Contracts.FirstOrDefault(c => c.Expiration.Subtract(framework.UtcNow) < TimeSpan.FromDays(7)));
            ContractEntity contract = contractInDb;
            
            //- act

            contract.Renew(); // this is what OOP truly is - encapsulate data and operations together.

            //- assert

            // 2015-05-30 -> renew -> 2015-06-30
            Assert.That(contract.Expiration, Is.EqualTo(new DateTime(2015, 6, 30)));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Application Code - coded by application developer

        /// <summary>
        /// Type of contract term (yearly/monthly)
        /// </summary>
        public enum ContractTermType
        {
            /// <summary>
            /// Monthly term
            /// </summary>
            Monthly,
            /// <summary>
            /// Yearly term
            /// </summary>
            Yearly
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Represents a contract with customer.
        /// This interface is exposed to UI layer, external client applications, and B2B intergations
        /// </summary>
        /// <remarks>
        /// Encapsulates both entity data and operations -- because this is what OOP truly is.
        /// </remarks>
        public interface IContractEntity
        {
            /// <summary>
            /// Expands contract expiration 1 time the term.
            /// </summary>
            /// <remarks>
            /// Since this method is part of entity interface, it can be invoked by a request received from outside of our system 
            /// (e.g., from web browser or through a B2B integration).
            /// </remarks>
            void Renew();
            
            /// <summary>
            /// Gets or sets type of contract term (monthly/yearly)
            /// </summary>
            ContractTermType Term { get; set; }
            
            /// <summary>
            /// Expiration of the contract according to current term.
            /// </summary>
            /// <remarks>
            /// The value of this property can only be set by the business logic.
            /// </remarks>
            DateTime Expiration { get; }

            /// <summary>
            /// Whether this contract has been approved. If not, the contract is not effective.
            /// </summary>
            /// <remarks>
            /// The value of this property can only be set by the business logic.
            /// </remarks>
            bool IsApproved { get; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// This class is abstract because we abstract from the other layers, e.g. communication, presentation, and persistence.
        /// </summary>
        /// <remarks>
        /// The persistence layer will generate concrete implementations of persistable properties, and properly handle data mechanics required from every 
        /// entity object implementation.
        /// 
        /// The most important, this class allows us encapsulate our business logic and the data together -- which is what OOP truly is.
        /// 
        /// For high throughput systems, the best options is to have at most one instance of every entity object per request, as opposed to having
        /// object per layer, which means copying data between objects when crossing the layers. The latter means added latency and heavier duty for GC, 
        /// which usually results in waveform graph of system throughput, as opposed to straight line when GC is minimized.
        /// </remarks>
        public abstract class ContractEntity : IContractEntity
        {
            private readonly IFramework _framework;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected ContractEntity(IFramework framework)
            {
                _framework = framework;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Implementation of IContractEntity - concrete class will be generated on the fly.

            // All we need here is write abstract property declarations. 
            // The underlying infrastructure will implement proper DTO/persistence mechanics under the hood.

            public abstract ContractTermType Term { get; set; }
            public abstract DateTime Expiration { get; protected set; }
            public abstract bool IsApproved { get; protected set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            // this method is part of the entity interface - it can be invoked by request received from client applications or through B2B intergation
            public void Renew()
            {
                // since this operation can be requested from outside of our system, proper validation is mandatory
                if ( !IsApproved )
                {
                    throw new BusinessRuleException("Contract cannot be renewed because it is not approved.");
                }

                // request is valid, let's do the change
                switch ( Term )
                {
                    case ContractTermType.Monthly:
                        Expiration = Expiration.AddMonths(1);
                        break;
                    case ContractTermType.Yearly:
                        Expiration = Expiration.AddYears(1);
                        break;
                }
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            // this method is not part of IContractEntity interface - it is only known within our business logic layer
            public void Approve()
            {
                IsApproved = true;
                Expiration = _framework.UtcNow;
                Renew();
            }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Infrastructure Classes - provided as part of NWheels

        public class BusinessRuleException : Exception
        {
            public BusinessRuleException(string message)
                : base(message)
            {
            }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Generated Code - dynamic class emitted on the fly, no human codes it

        // this class is basically implementation of entity object;
        // while the inherited members of the business logic are here, they are ignored by the persistence.
        public class GeneratedDomainObject_ContractEntity : ContractEntity
        {
            // constructor - injects dependencies
            public GeneratedDomainObject_ContractEntity(IFramework framework)
                : base(framework)
            {
            }

            // proper implementation of persistence mechanics per specific ORM/ODM framework fill follow.
            // in this example, those are just automatic properties + wrapper properties

            public override ContractTermType Term { get; set; }
            public override DateTime Expiration { get; protected set; }
            public override bool IsApproved { get; protected set; }

            // ORM/ODM frameworks will most likely want public get and set accessors to persistable properties
            public virtual DateTime Expiration_PersistenceWrapper
            {
                get { return Expiration; }
                set { Expiration = value; }
            }
            public virtual bool IsApproved_PersistenceWrapper
            {
                get { return IsApproved; }
                set { IsApproved = value; }
            }
        }

        #endregion
    }
}
