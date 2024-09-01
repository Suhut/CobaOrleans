using Idu.Orleans.Grains.Abstractions;
using Idu.Orleans.Grains.State;

namespace Idu.Orleans.Grains.Grains;

public class AtmGrain : Grain, IAtmGrain
{
    private readonly IPersistentState<AtmState> _atmState;
    public AtmGrain(
        [PersistentState("atm", "BalanceStorage")] IPersistentState<AtmState> atmState
        )
    {
        _atmState = atmState;
    }
    public async Task Initialise(decimal openingBalance)
    {
        _atmState.State.Balance = openingBalance;
        _atmState.State.Id = this.GetGrainId().GetGuidKey();
    }

    public async Task Withdraw(Guid checkingAccountId, decimal amount)
    {
        var checkingAccountGrain = this.GrainFactory.GetGrain<ICheckingAcountGrain>(checkingAccountId);
        await checkingAccountGrain.Debit(amount);

        var currenctAtmBalance = _atmState.State.Balance;
        var updateBalance = currenctAtmBalance - amount;
        _atmState.State.Balance = updateBalance;
        await _atmState.WriteStateAsync();
    }
}
