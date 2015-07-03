using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Builder;
using Autofac.Features.LightweightAdapters;
using Autofac.Features.Metadata;

namespace NWheels.Extensions
{
    public static partial class AutofacExtensions
    {
        public static readonly string PipelineIndexMetadataKey = "NWheels::Extensions::AutofacExtensions::PipelineIndex";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static int _s_pipelineHeadIndex = 0;
        private static int _s_pipelineTailIndex = 0;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static IRegistrationBuilder<Pipeline<TService>, LightweightAdapterActivatorData, DynamicRegistrationStyle> RegisterPipeline<TService>(
            this ContainerBuilder builder)
        {
            return builder.RegisterAdapter<IEnumerable<Meta<TService>>, Pipeline<TService>>(
                metaPipe => {
                    return new Pipeline<TService>(metaPipe.OrderBy(GetPipelineIndex).Select(m => m.Value));
                }
            );
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> FirstInPipeline<TLimit, TActivatorData, TRegistrationStyle>(
            this IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> registration)
        {
            return registration.WithMetadata(PipelineIndexMetadataKey, Interlocked.Decrement(ref _s_pipelineHeadIndex));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> LastInPipeline<TLimit, TActivatorData, TRegistrationStyle>(
            this IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> registration)
        {
            return registration.WithMetadata(PipelineIndexMetadataKey, Interlocked.Increment(ref _s_pipelineTailIndex));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static Pipeline<TService> ResolvePipeline<TService>(this IComponentContext container)
        {
            return container.Resolve<Pipeline<TService>>();
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private static int GetPipelineIndex<T>(Meta<T> meta)
        {
            object indexValue;

            if ( meta.Metadata.TryGetValue(AutofacExtensions.PipelineIndexMetadataKey, out indexValue) )
            {
                return (int)indexValue;
            }
            else
            {
                return 0;
            }
        }
    }
}
