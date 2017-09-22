using System;
using System.Collections.Generic;
using System.Text;
using NWheels.Kernel.Api.Execution;

namespace NWheels.Kernel.Api.Extensions
{
    public static class ExecutionPathActivityExtensions
    {
        public static void RunActivityOrThrow(this IExecutionPathActivity activity, Action code)
        {
            using (activity)
            {
                try
                {
                    code();
                }
                catch (Exception error)
                {
                    activity.Fail(error);
                    throw;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void RunActivityOrThrow(this IExecutionPathActivity activity, Action<IExecutionPathActivity> code)
        {
            using (activity)
            {
                try
                {
                    code(activity);
                }
                catch (Exception error)
                {
                    activity.Fail(error);
                    throw;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static T RunActivityOrThrow<T>(this IExecutionPathActivity activity, Func<T> code)
        {
            using (activity)
            {
                try
                {
                    return code();
                }
                catch (Exception error)
                {
                    activity.Fail(error);
                    throw;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static T RunActivityOrThrow<T>(this IExecutionPathActivity activity, Func<IExecutionPathActivity, T> code)
        {
            using (activity)
            {
                try
                {
                    return code(activity);
                }
                catch (Exception error)
                {
                    activity.Fail(error);
                    throw;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool RunActivityOrCatch(this IExecutionPathActivity activity, Action code, out Exception error)
        {
            using (activity)
            {
                try
                {
                    code();
                    error = null;
                    return true;
                }
                catch (Exception e)
                {
                    activity.Fail(e);
                    error = e;
                    return false;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool RunActivityOrCatch(this IExecutionPathActivity activity, Action<IExecutionPathActivity> code, out Exception error)
        {
            using (activity)
            {
                try
                {
                    code(activity);
                    error = null;
                    return true;
                }
                catch (Exception e)
                {
                    activity.Fail(e);
                    error = e;
                    return false;
                }
            }
        }
    }
}
