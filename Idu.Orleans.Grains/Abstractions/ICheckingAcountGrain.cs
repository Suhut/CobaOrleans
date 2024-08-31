namespace Idu.Orleans.Grains.Abstractions;

public interface ICheckingAcountGrain : IGrainWithGuidKey
{
    Task Initialise(decimal openingBalance);
    Task<decimal> GetBalance();
    Task Debit(decimal amount);
    Task Credit(decimal amount);
}
