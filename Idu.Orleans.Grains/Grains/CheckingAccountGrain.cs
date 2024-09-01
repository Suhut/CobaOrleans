using Idu.Orleans.Grains.Abstractions;
using Idu.Orleans.Grains.State;
using Orleans;
using Orleans.Concurrency;
using Orleans.Transactions.Abstractions;

namespace Idu.Orleans.Grains.Grains;

[Reentrant]
public class CheckingAccountGrain : Grain, ICheckingAcountGrain, IRemindable
{
    private readonly ITransactionClient _transactionClient;
    private readonly ITransactionalState<BalanceState> _balanceTransactioState;
    private readonly IPersistentState<CheckingAccountState> _checkingAccountState;
    public CheckingAccountGrain(
        ITransactionClient transactionClient,
        [TransactionalState("balance", "BalanceStorage")] ITransactionalState<BalanceState> balanceTransactioState,
        [PersistentState("checkingAccount", "CheckingAccountStorage")] IPersistentState<CheckingAccountState> checkingAccountState
        )
    {
        _transactionClient = transactionClient;
        _balanceTransactioState = balanceTransactioState;
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

            await _transactionClient.RunTransaction(TransactionOption.Create,async () =>
            {
                await Debit(recuringPayment.PaymentAmount);
            });
           
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

        await _balanceTransactioState.PerformUpdate(state =>
        {
            var currentBalance = state.Balance;
            var newBalance = currentBalance + amount;
            state.Balance = newBalance;
        });

    }
    public async Task Debit(decimal amount)
    {
        ////untuk membuktikan bahwa timer tidak create massage baru
        //{
        //    Console.WriteLine($"Starting Debit : ");
        //    await Task.Delay(TimeSpan.FromSeconds(20));
        //    Console.WriteLine($"Ending Debit : ");
        //} 

        await _balanceTransactioState.PerformUpdate(state =>
        {
            var currentBalance = state.Balance;
            var newBalance = currentBalance - amount;
            state.Balance = newBalance;

        });

    }

    public async Task<decimal> GetBalance()
    {
        return await _balanceTransactioState.PerformRead(state => state.Balance);
    }

    public async Task Initialise(decimal openingBalance)
    {
        _checkingAccountState.State.OpenedAtUtc = DateTime.UtcNow;
        _checkingAccountState.State.AccountType = "Default";
        _checkingAccountState.State.AccountId = this.GetGrainId().GetGuidKey();

        _checkingAccountState.State.OpenedAtUtc = DateTime.UtcNow;

        await _balanceTransactioState.PerformUpdate(state =>
        {
            state.Balance = openingBalance;
        });

        await _checkingAccountState.WriteStateAsync();
    }
}
