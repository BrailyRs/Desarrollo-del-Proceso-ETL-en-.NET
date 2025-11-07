






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



        services.AddTransient<IETLService, ETLService>();







        services.AddDbContext<OpinionDwContext>(options =>



            options.UseSqlServer(hostContext.Configuration.GetConnectionString("OpinionDwContext")));







        var dataSource = hostContext.Configuration["DataSource"];







        if (dataSource == "db")



        {



            services.AddDbContext<OpinionRContext>(options =>



                options.UseSqlServer(hostContext.Configuration.GetConnectionString("OpinionRContext")));



            services.AddTransient<IDataRepository, DbDataRepository>();



        }



        else



        {



            services.AddTransient<IDataRepository, CsvDataRepository>();



            services.AddDbContext<OpinionRContext>(options =>



                options.UseSqlServer(hostContext.Configuration.GetConnectionString("OpinionRContext")), ServiceLifetime.Scoped);



        }



    })



    .Build();







host.Run();




