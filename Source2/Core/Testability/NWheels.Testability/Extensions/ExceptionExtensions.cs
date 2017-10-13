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
    }
}
