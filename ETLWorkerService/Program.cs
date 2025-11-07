using System;
using ETLWorkerService.Application.Services;
using ETLWorkerService.Core.Interfaces;
using ETLWorkerService.Infrastructure.Data;
using ETLWorkerService.Infrastructure.Repositories;
using ETLWorkerService.Presentation;
using Microsoft.EntityFrameworkCore;







IHost host = Host.CreateDefaultBuilder(args)



    .ConfigureServices((hostContext, services) =>



    {



        services.AddHostedService<ETLWorker>();



        // Register a factory for IETLService that takes an IServiceProvider and a string (dataSourceType)
        services.AddTransient<Func<IServiceProvider, string, IETLService>>(rootServiceProvider => (scopedServiceProvider, dataSourceType) =>
        {
            IDataRepository dataRepository;
            var logger = scopedServiceProvider.GetRequiredService<ILogger<ETLService>>();
            var dwContext = scopedServiceProvider.GetRequiredService<OpinionDwContext>();
            var rContext = scopedServiceProvider.GetRequiredService<OpinionRContext>();

            switch (dataSourceType)
            {
                case "csv":
                    dataRepository = scopedServiceProvider.GetRequiredService<CsvDataRepository>();
                    break;
                case "api":
                    dataRepository = scopedServiceProvider.GetRequiredService<ApiDataRepository>();
                    break;
                case "db":
                    dataRepository = scopedServiceProvider.GetRequiredService<DbDataRepository>();
                    break;
                default:
                    throw new ArgumentException($"Unknown data source type: {dataSourceType}");
            }

            return new ETLService(logger, dataRepository, dwContext, rContext);
        });







        services.AddDbContext<OpinionDwContext>(options =>



            options.UseSqlServer(hostContext.Configuration.GetConnectionString("OpinionDwContext")));







        services.AddHttpClient();

        // Register all IDataRepository implementations
        services.AddTransient<CsvDataRepository>();
        services.AddTransient<ApiDataRepository>();
        services.AddTransient<DbDataRepository>();

        // Register OpinionRContext as scoped
        services.AddDbContext<OpinionRContext>(options =>
            options.UseSqlServer(hostContext.Configuration.GetConnectionString("OpinionRContext")), ServiceLifetime.Scoped);



    })



    .Build();







host.Run();




