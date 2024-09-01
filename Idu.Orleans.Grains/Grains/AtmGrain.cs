using Idu.Orleans.Grains.Abstractions;
using Idu.Orleans.Grains.State;
using Orleans.Concurrency;
using Orleans.Transactions.Abstractions;

namespace Idu.Orleans.Grains.Grains;

[Reentrant]
public class AtmGrain : Grain, IAtmGrain
{
    private readonly ITransactionalState<AtmState> _atmTransactionState;
    public AtmGrain(
        [TransactionalState("atm", "BalanceStorage")] ITransactionalState<AtmState> atmTransactionState
        )
    {
        _atmTransactionState = atmTransactionState;
    }
    public async Task Initialise(decimal openingBalance)
    {
        await _atmTransactionState.PerformUpdate( state =>
        {
            state.Balance = openingBalance;
            state.Id = this.GetGrainId().GetGuidKey();
        }
         ); 
    }

    public async Task Withdraw(Guid checkingAccountId, decimal amount)
    {
        var checkingAccountGrain = this.GrainFactory.GetGrain<ICheckingAcountGrain>(checkingAccountId);
        //await checkingAccountGrain.Debit(amount);

        await _atmTransactionState.PerformUpdate(state =>
        {
            var currenctAtmBalance = state.Balance;
            var updateBalance = currenctAtmBalance - amount;
            state.Balance = updateBalance; 
        });
      
    }
}
