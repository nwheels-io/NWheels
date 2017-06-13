using NWheels.Frameworks.Uidl.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Frameworks.Uidl.Testability
{
    public static class AbstractUIElementExtensions
    {
        public static Task<bool> IsEnabled(this IAbstractUIElement element)
        {
            return Task<bool>.FromResult(true);
        }

        public static Task<bool> IsDisabled(this IAbstractUIElement element)
        {
            return Task<bool>.FromResult(true);
        }

        public static Task<bool> IsVisible(this IAbstractUIElement element)
        {
            return Task<bool>.FromResult(true);
        }

        public static Task<bool> IsHidden(this IAbstractUIElement element)
        {
            return Task<bool>.FromResult(true);
        }

        public static Task<bool> IsValid(this IAbstractUIElement element)
        {
            return Task<bool>.FromResult(true);
        }

        public static Task<bool> IsInvalid(this IAbstractUIElement element)
        {
            return Task<bool>.FromResult(true);
        }

        public static Task<bool> HasText(this IAbstractUIElement element, string text)
        {
            return Task<bool>.FromResult(true);
        }

        public static Task TypeText(this IAbstractUIElement element, string text)
        {
            return Task.CompletedTask;
        }

        public static Task ClearValue(this IAbstractUIElement element)
        {
            return Task.CompletedTask;
        }

        public static Task Click(this IAbstractUIElement element)
        {
            return Task.CompletedTask;
        }
    }
}
