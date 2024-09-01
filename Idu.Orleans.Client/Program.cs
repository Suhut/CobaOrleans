using Idu.Orleans.Client.Contracts;
using Idu.Orleans.Grains.Abstractions;
using Orleans.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseOrleansClient((context, client) =>
{
    client.UseAdoNetClustering(configureOptions: options =>
    {
        //https://learn.microsoft.com/id-id/dotnet/orleans/host/configuration-guide/configuring-ado-dot-net-providers
        options.Invariant = "System.Data.SqlClient";
        options.ConnectionString = "Server=SUHUT-TUF;Database=IDU_ORLEANS;TrustServerCertificate=True;Trusted_Connection=True;MultipleActiveResultSets=true;Application Name=OrleansApp;";

    });

    client.Configure<ClusterOptions>(options =>
    {
        options.ClusterId = "IduCluster";
        options.ServiceId = "IduService";
    });

    client.UseTransactions();

});

var app = builder.Build();

app.MapGet("CheckingAccount/{checkingAccountId}/balance",
    async (
        Guid checkingAccountId,
        ITransactionClient transactionClient,
        IClusterClient clusterClient) =>
    {
        decimal balance = 0;
        await transactionClient.RunTransaction(TransactionOption.Create, async () =>
        {
            var checkingAccountGrain = clusterClient.GetGrain<ICheckingAcountGrain>(checkingAccountId);

            balance = await checkingAccountGrain.GetBalance();
        });


        return TypedResults.Ok(balance);
    });


app.MapPost("CheckingAccount",
    async (
    CreateAccount createAccount,
    ITransactionClient transactionClient,
    IClusterClient clusterClient) =>
{
    var checkingAccountId = Guid.NewGuid();



    await transactionClient.RunTransaction(TransactionOption.Create, async () =>
    {
        var checkingAccountGrain = clusterClient.GetGrain<ICheckingAcountGrain>(checkingAccountId);

        await checkingAccountGrain.Initialise(createAccount.OpeningBalance);
    });



    return TypedResults.Created($"CheckingAccount/{checkingAccountId}");
});

app.MapPost("CheckingAccount/{checkingAccountId:guid}/debit",
    async (
    Guid checkingAccountId,
    Debit debit,
    ITransactionClient transactionClient,
    IClusterClient clusterClient) =>
    {
        await transactionClient.RunTransaction(TransactionOption.Create, async () =>
        {
            var checkingAccountGrain = clusterClient.GetGrain<ICheckingAcountGrain>(checkingAccountId);

            await checkingAccountGrain.Debit(debit.Amount);
        });


        return TypedResults.NoContent();
    });



app.MapPost("CheckingAccount/{checkingAccountId:guid}/credit",
    async (
    Guid checkingAccountId,
    Credit credit,
    ITransactionClient transactionClient,
    IClusterClient clusterClient) =>
    {

        await transactionClient.RunTransaction(TransactionOption.Create, async () =>
        {
            var checkingAccountGrain = clusterClient.GetGrain<ICheckingAcountGrain>(checkingAccountId);

            await checkingAccountGrain.Credit(credit.Amount);
        }); 
       

        return TypedResults.NoContent();
    });

app.MapPost("CheckingAccount/{checkingAccountId:guid}/recurringPayment",
    async (
    Guid checkingAccountId,
    CreateRecurringPayment createRecurringPayment,
    IClusterClient clusterClient) =>
    {

        var checkingAccountGrain = clusterClient.GetGrain<ICheckingAcountGrain>(checkingAccountId);

        await checkingAccountGrain.AddRecurringPayment(createRecurringPayment.PaymentId, createRecurringPayment.PaymentAmount, createRecurringPayment.PaymentRecurrsEveryMinutes);

        return TypedResults.NoContent();
    });

app.MapPost("Atm",
    async (
    CreateAtm createAtm,
    ITransactionClient transactionClient,
    IClusterClient clusterClient) =>
    {
        var atmId = Guid.NewGuid(); 

        await transactionClient.RunTransaction(TransactionOption.Create, async () =>
        {
            var atmGrain = clusterClient.GetGrain<IAtmGrain>(atmId);

            await atmGrain.Initialise(createAtm.InitialAtmCashBalance);
        });

        return TypedResults.Created($"atm/{atmId}");
    });


app.MapPost("Atm/{atmId:guid}/withdraw",
    async (
    Guid atmId,
    AtmWithdraw atmWithdraw,
    ITransactionClient transactionClient,
    IClusterClient clusterClient) =>
    {
        await transactionClient.RunTransaction(TransactionOption.Create, async () =>
        {
            var atmGrain = clusterClient.GetGrain<IAtmGrain>(atmId);

            var checkingAccountGrain=clusterClient.GetGrain<ICheckingAcountGrain>(atmWithdraw.CheckingAccountId);

            await atmGrain.Withdraw(atmWithdraw.CheckingAccountId, atmWithdraw.Amount);

            await checkingAccountGrain.Debit(atmWithdraw.Amount);
        });


        return TypedResults.NoContent();
    });


app.Run();
