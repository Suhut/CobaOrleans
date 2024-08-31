using Idu.Orleans.Grains.Abstractions;

namespace Idu.Orleans.Grains.Grains;

public class CheckingAccountGrain : Grain, ICheckingAcountGrain
{
    private decimal _balance;
    public async Task<decimal> GetBalance()
    {
       return _balance;
    }

    public async Task Initialise(decimal openingBalance)
    {
       _balance = openingBalance;
    }
}
