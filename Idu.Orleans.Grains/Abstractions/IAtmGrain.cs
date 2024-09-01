namespace Idu.Orleans.Grains.Abstractions;

public interface IAtmGrain : IGrainWithGuidKey
{
    public Task Initialise(decimal openingBalance);
    public Task Withdraw(Guid checkingAccountId, decimal amount);
}
