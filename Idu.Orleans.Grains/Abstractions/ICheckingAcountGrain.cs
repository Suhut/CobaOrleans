namespace Idu.Orleans.Grains.Abstractions;

public interface ICheckingAcountGrain : IGrainWithGuidKey
{
    [Transaction(TransactionOption.Create)]
    Task Initialise(decimal openingBalance);
    [Transaction(TransactionOption.Create)]
    Task<decimal> GetBalance();
    [Transaction(TransactionOption.CreateOrJoin)]
    Task Debit(decimal amount);
    [Transaction(TransactionOption.CreateOrJoin)]
    Task Credit(decimal amount);
    Task AddRecurringPayment(Guid id, decimal amount, int reccursEveryMin);
}
