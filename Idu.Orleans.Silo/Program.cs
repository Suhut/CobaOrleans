using Microsoft.Extensions.Hosting;
using Orleans.Configuration;

await Host.CreateDefaultBuilder(args)
        .UseOrleans(siloBuilder =>
        {
            siloBuilder.UseAdoNetClustering(configureOptions: options =>
            {
                //https://learn.microsoft.com/id-id/dotnet/orleans/host/configuration-guide/configuring-ado-dot-net-providers
                options.Invariant = "System.Data.SqlClient";
                options.ConnectionString = "Server=SUHUT-TUF;Database=IDU_ORLEANS;TrustServerCertificate=True;Trusted_Connection=True;MultipleActiveResultSets=true;Application Name=OrleansApp;";

            });

            siloBuilder.Configure<ClusterOptions>(options =>
            {
                options.ClusterId = "IduCluster";
                options.ServiceId = "IduService";
            });

            siloBuilder.AddAdoNetGrainStorage("BalanceStorage", options =>
            {
                options.Invariant = "System.Data.SqlClient";
                options.ConnectionString = "Server=SUHUT-TUF;Database=IDU_ORLEANS;TrustServerCertificate=True;Trusted_Connection=True;MultipleActiveResultSets=true;Application Name=OrleansApp;";
            });

            siloBuilder.AddAdoNetGrainStorage("CheckingAccountStorage", options =>
            {
                options.Invariant = "System.Data.SqlClient";
                options.ConnectionString = "Server=SUHUT-TUF;Database=IDU_ORLEANS;TrustServerCertificate=True;Trusted_Connection=True;MultipleActiveResultSets=true;Application Name=OrleansApp;";
            });

            //siloBuilder.Configure<GrainCollectionOptions>(options =>
            //{
            //    options.CollectionQuantum = TimeSpan.FromSeconds(20);
            //    options.CollectionAge = TimeSpan.FromSeconds(20);
            //});

        }).RunConsoleAsync();
 