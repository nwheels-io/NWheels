using NWheels.Api;
using NWheels.Api.Logging;
using NWheels.Api.Exceptions;
using NWheels.Api.Ddd;
using NWheels.Api.Ddd.Exceptions;

namespace ExpenseTracker.Domain
{
    [ByConvention.SemanticLogger]
    public interface IBudgetManagementLogger
    {
        [LogAs.Error(FaultParty.Client)]
        DomainErrorException<BudgetManagementError> CannotMoveCategory(BudgetManagementError error, Category source, Category destination);

        
    }
}