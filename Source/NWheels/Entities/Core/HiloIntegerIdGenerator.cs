using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.Hosting;

namespace NWheels.Entities.Core
{
    public class HiloIntegerIdGenerator : LifecycleEventListenerBase, IPropertyValueGenerator<int>, IDomainContextPopulator
    {
        private readonly object _syncRoot = new object();
        private readonly IFramework _framework;
        private readonly int _hiBase;
        private int _max;
        private int _next;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public HiloIntegerIdGenerator(IFramework framework, int loDigits)
        {
            if ( loDigits < 1 || loDigits > 7 )
            {
                throw new ArgumentOutOfRangeException("loDigits", "loDigits must be 1..7");
            }

            _framework = framework;
            _hiBase = (int)Math.Pow(10, loDigits);
            _next = 1;
            _max = _hiBase;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        void IDomainContextPopulator.Populate(IApplicationDataRepository context)
        {
            var hiloContext = (IHiloDataRepository)context;
            
            var hilo = hiloContext.Hilo.New();
            hilo.Hi = 1;
            hiloContext.Hilo.Insert(hilo);
            hiloContext.CommitChanges();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        Type IDomainContextPopulator.ContextType
        {
            get
            {
                return typeof(IHiloDataRepository);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public int GenerateValue(string qualifiedPropertyName)
        {
            var taken = Interlocked.Increment(ref _next);

            while ( taken >= _max )
            {
                lock ( _syncRoot )
                {
                    if ( _next >= _max )
                    {
                        TakeNextHi();
                    }

                    taken = Interlocked.Increment(ref _next);
                }
            }

            return taken;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void NodeLoading()
        {
            TakeNextHi();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void TakeNextHi()
        {
            using ( var data = _framework.NewUnitOfWork<IHiloDataRepository>() )
            {
                var hilo = data.Hilo.AsQueryable().Single();

                _next = hilo.Hi * _hiBase;
                _max = _next + _hiBase;

                hilo.Hi = hilo.Hi + 1;
                data.Hilo.Update(hilo);
                data.CommitChanges();
            }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [EntityContract]
    public interface IHiloEntity
    {
        [PropertyContract.EntityId]
        int Hi { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IHiloDataRepository : IApplicationDataRepository
    {
        IEntityRepository<IHiloEntity> Hilo { get; }
    }
}
