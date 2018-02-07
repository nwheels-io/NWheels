using System;

namespace NWheels.Testability.Extensions
{
    public static class ExceptionExtensions
    {
        public static Exception InnermostException(this Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            var result = exception.InnerException;

            while (result != null && result.InnerException != null)
            {
                result = result.InnerException;
            }

            return result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static Exception Flatten(this Exception exception)
        {
            if (exception is AggregateException aggregate)
            {
                if (aggregate.InnerExceptions.Count == 1)
                {
                    return aggregate.InnerExceptions[0];
                }
                else
                {
                    return aggregate.Flatten();
                }
            }

            return exception;
        }
    }
}
