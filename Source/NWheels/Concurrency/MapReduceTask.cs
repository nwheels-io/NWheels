using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NWheels.Concurrency
{
    public static class MapReduceTask
    {
        public static Task<TResult> StartNew<TInput, TPartialResult, TResult>(
            Func<TInput, TPartialResult> map, 
            Func<TPartialResult[], TResult> reduce, 
            TInput[] inputs,
            CancellationToken cancellation)
        {
            var mapTasks = CreateMapTasks(map, inputs, cancellation);
            var reduceTask = CreateReduceTask(reduce, mapTasks, cancellation);

            return reduceTask;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static Task<TPartialResult>[] CreateMapTasks<TInput, TPartialResult>(
            Func<TInput, TPartialResult> map,
            TInput[] inputs,
            CancellationToken cancellation)
        {
            var tasks = new Task<TPartialResult>[inputs.Length];

            for (int i = 0; i < inputs.Length; ++i)
            {
                var input = inputs[i];

                tasks[i] = Task.Factory.StartNew(
                    () => {
                        return map(input);
                    },
                    cancellation
                );
            }

            return tasks;
        }
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static Task<TResult> CreateReduceTask<TPartialResult, TResult>(
            Func<TPartialResult[], TResult> reduce, 
            Task<TPartialResult>[] mapTasks,
            CancellationToken cancellation)
        {
            return Task.Factory.ContinueWhenAll(
                mapTasks,
                tasks => {
                    return PerformReduce(reduce, tasks);
                },
                cancellation);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static TResult PerformReduce<TPartialResult, TResult>(Func<TPartialResult[], TResult> reduce, Task<TPartialResult>[] mapTasks)
        {
            var partialResults = mapTasks.Select(task => task.Result);
            return reduce(partialResults.ToArray());
        }
    }
}
