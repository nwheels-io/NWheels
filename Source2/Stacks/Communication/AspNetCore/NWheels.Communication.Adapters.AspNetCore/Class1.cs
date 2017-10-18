using System;
using Microsoft.AspNetCore.Builder;

namespace NWheels.Communication.Adapters.AspNetCore
{
    public class Class1
    {
        public void F()
        {
            IApplicationBuilder b = null;
            b.Use(async (context, next) => {
                // Do work that doesn't write to the Response.
                await next.Invoke();
                // Do logging or other work that doesn't write to the Response.
            });
        }
    }
}
