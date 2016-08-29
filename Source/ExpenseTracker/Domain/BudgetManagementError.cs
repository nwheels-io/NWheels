namespace ExpenseTracker.Domain
{
    public enum BudgetManagementError
    {
        DestinationIsSubCategoryOfSource,
        DestinationSiblingCategoryMismatch,
        CouldNotFindCategoryToDetach
    }
}
