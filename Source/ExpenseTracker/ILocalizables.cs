using NWheels.Api;

namespace ExpenseTracker.Domain
{
    [ByConvention.LocalizableResources]
    public interface ILocalizables
    {
        string AllExpenses { get; }
    }
}