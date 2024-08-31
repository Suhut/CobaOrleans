using Idu.Orleans.Grains.Abstractions;
using Idu.Orleans.Grains.State;

namespace Idu.Orleans.Grains.Grains;

public class CheckingAccountGrain : Grain, ICheckingAcountGrain
{
    private readonly IPersistentState<BalanceState> _balanceState;
    private readonly IPersistentState<CheckingAccountState> _checkingAccountState;
    public CheckingAccountGrain(
        [PersistentState("balance", "BalanceStorage")] IPersistentState<BalanceState> balanceState,
        [PersistentState("checkingAccount", "CheckingAccountStorage")] IPersistentState<CheckingAccountState> checkingAccountState
        )
    {
        _balanceState = balanceState;
        _checkingAccountState = checkingAccountState;
    }

    public async Task Credit(decimal amount)
    {
        var currentBalance = _balanceState.State.Balance;
        var newBalance = currentBalance + amount;
        _balanceState.State.Balance = newBalance;
        await _balanceState.WriteStateAsync();
    }
    public async Task Debit(decimal amount)
    {
        var currentBalance = _balanceState.State.Balance;
        var newBalance = currentBalance - amount;
        _balanceState.State.Balance = newBalance;
        await _balanceState.WriteStateAsync();
    }

    public async Task<decimal> GetBalance()
    {
        return _balanceState.State.Balance;
    }

    public async Task Initialise(decimal openingBalance)
    {
        _checkingAccountState.State.OpenedAtUtc= DateTime.UtcNow;
        _checkingAccountState.State.AccountType= "Default";
        _checkingAccountState.State.AccountId=  this.GetGrainId().GetGuidKey();

        _checkingAccountState.State.OpenedAtUtc= DateTime.UtcNow;


        _balanceState.State.Balance = openingBalance;
        await _balanceState.WriteStateAsync();
        await _checkingAccountState.WriteStateAsync();
    }
}
