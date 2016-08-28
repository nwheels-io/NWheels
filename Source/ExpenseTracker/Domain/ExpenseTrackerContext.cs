using NWheels.Api.Ddd;
using ExpenseTracker.Domain.Entities;
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
            var transaction =  new Transaction() {
                Id = Guid.NewGuid(),
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