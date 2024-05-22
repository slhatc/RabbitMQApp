using RabbitMQ.Client;
using WorkerService.Services;
using WorkerService;
using WorkerService.Models;
using Microsoft.EntityFrameworkCore;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext,services) =>
    {
        IConfiguration Configuration = hostContext.Configuration;
        services.AddDbContext<AdventureWorks2017Context>(options => { options.UseSqlServer(Configuration.GetConnectionString("SqlServer")); }); 
        services.AddSingleton<RabbitMQClientService>();
        services.AddSingleton(sp => new ConnectionFactory() { Uri = new Uri(Configuration.GetConnectionString("RabbitMQ")!), DispatchConsumersAsync = true });
        services.AddHostedService<Worker>();
    })
    .Build();

host.Run();
