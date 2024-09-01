using Idu.Orleans.Grains.Abstractions;
using Idu.Orleans.Grains.State;

namespace Idu.Orleans.Grains.Grains;

public class CheckingAccountGrain : Grain, ICheckingAcountGrain, IRemindable
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

    public async Task AddRecurringPayment(Guid id, decimal amount, int reccursEveryMinutes)
    {
        _checkingAccountState.State.RecurringPayments.Add(new RecurringPayment { PaymentId = id, PaymentAmount = amount, OccursEveryMinutes = reccursEveryMinutes });
        await _checkingAccountState.WriteStateAsync();

        await this.RegisterOrUpdateReminder($"RecurringPayment:::{id}", TimeSpan.FromMinutes(reccursEveryMinutes), TimeSpan.FromMinutes(reccursEveryMinutes));
    }

    public async Task ReceiveReminder(string reminderName, TickStatus status)
    {
        if (reminderName.StartsWith("RecurringPayment"))
        {
            var recurringPaymentId = Guid.Parse(reminderName.Split(":::").Last());
            var recuringPayment = _checkingAccountState.State.RecurringPayments.Single(x => x.PaymentId == recurringPaymentId);

            await Debit(recuringPayment.PaymentAmount);
            Console.WriteLine($"RecurringPayment : {DateTime.Now.ToString("HH:mm:ss")}");
        }


    }

    public async Task Credit(decimal amount)
    {
        ////untuk membuktikan bahwa timer tidak create massage baru
        //RegisterTimer(async (State) =>
        //{
        //    Console.WriteLine($"Starting Timer : ");
        //    await Task.Delay(TimeSpan.FromSeconds(5));
        //    Console.WriteLine($"Ending Timer : ");
        //}, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));

        var currentBalance = _balanceState.State.Balance;
        var newBalance = currentBalance + amount;
        _balanceState.State.Balance = newBalance;
        await _balanceState.WriteStateAsync();
    }
    public async Task Debit(decimal amount)
    {
        ////untuk membuktikan bahwa timer tidak create massage baru
        //{
        //    Console.WriteLine($"Starting Debit : ");
        //    await Task.Delay(TimeSpan.FromSeconds(20));
        //    Console.WriteLine($"Ending Debit : ");
        //} 

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
        _checkingAccountState.State.OpenedAtUtc = DateTime.UtcNow;
        _checkingAccountState.State.AccountType = "Default";
        _checkingAccountState.State.AccountId = this.GetGrainId().GetGuidKey();

        _checkingAccountState.State.OpenedAtUtc = DateTime.UtcNow;


        _balanceState.State.Balance = openingBalance;
        await _balanceState.WriteStateAsync();
        await _checkingAccountState.WriteStateAsync();
    }
}
