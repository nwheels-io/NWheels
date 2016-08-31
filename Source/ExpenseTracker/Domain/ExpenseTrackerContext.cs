using NWheels.Api.Ddd;
using ExpenseTracker.Domain;
using System.Linq;
using System;

namespace ExpenseTracker.Domain
{
    [DomainModel.BoundedContext]
    public class ExpenseTrackerContext
    {

        public IQueryable<Transaction> QueryDailyTransactions(DateTime date)
        {
            return Transactions.AsQueryable().Where(t => t.Date.Date == date.Date);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Guid InsertTransaction(DateTime date, decimal amount, string memo)
        {
            var transaction = new Transaction() {
                Date = date,
                Amount = amount,
                Memo = memo
            };

            this.Transactions.Insert(transaction);

            return transaction.Id;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual IEntityRepository<Transaction> Transactions { get; set; }
    }
}