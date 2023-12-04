using Microsoft.Extensions.DependencyInjection;
using Serilog;
using DataProcessor.Data;
using DataProcessor.Servicies.Implementations;
using DataProcessor.Servicies.Interfaces;
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

class Program
{
    static async Task Main()
    {
        IConfiguration configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .Build();
        string rabbitMQConnectionString = configuration["RabbitMQ:ConnectionString"] ?? throw new ArgumentNullException(nameof(rabbitMQConnectionString));
        string queueName = configuration["RabbitMQ:QueueName"] ?? throw new ArgumentNullException(nameof(queueName));
        string appDbContext = configuration["ConnectionDBStrings:AppDbContext"] ?? throw new ArgumentNullException(nameof(appDbContext));

        Console.WriteLine($"Connection String: {rabbitMQConnectionString}");
        Console.WriteLine($"Queue Name: {queueName}");
        Console.WriteLine($"AppDbContext: {appDbContext}");

        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(options => options.UseSqlite(appDbContext))
            .AddLogging(builder =>
            {
                builder.AddSerilog();
            })
            .AddScoped<IRabbitMQService, RabbitMQService>()
            .BuildServiceProvider();

        using (var scope = serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var logger = Log.Logger;
            var rabbitMQService = new RabbitMQService(dbContext, queueName, rabbitMQConnectionString, logger);

            await rabbitMQService.StartReceivingMessagesAsync();

            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();
        }
    }
}