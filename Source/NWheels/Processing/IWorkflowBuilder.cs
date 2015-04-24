using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Processing.Core;

namespace NWheels.Processing
{
    public interface IWorkflowBuilder
    {
        void AddActor<TWorkItem>(string name, int priority, IWorkflowActor<TWorkItem> actor, IWorkflowRouter router);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface ISequentialFlow
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public static class SequentialFlowExtensions
    {
        public static ISequentialFlow AddSequence(this IWorkflowBuilder builder)
        {
            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static ISequentialFlow Do(this ISequentialFlow flow, string stepName, Action action)
        {
            return flow;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static ISequentialFlowThen If(this ISequentialFlow flow, string stepName, Func<bool> condition)
        {
            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static ISequentialFlowThen While(this ISequentialFlow flow, string stepName, Func<bool> condition)
        {
            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static ISequentialFlow Parallel(this ISequentialFlow flow, string stepName, params Action<ISequentialFlow>[] branches)
        {
            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public interface ISequentialFlowThen
        {
            ISequentialFlowElse Then(Action<ISequentialFlow> sequence);
        }
        public interface ISequentialFlowElse : ISequentialFlowEndif
        {
            ISequentialFlowEndif Else(Action<ISequentialFlow> sequence);
            ISequentialFlowElse ElseIf(string stepName, Func<bool> condition);
        }
        public interface ISequentialFlowEndif
        {
            ISequentialFlow EndIf();
        }
    }
}
