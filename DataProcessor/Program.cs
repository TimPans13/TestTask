using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SQLiteDB.Data;
using SQLiteDB.Servicies.Implementations;
using SQLiteDB.Servicies.Interfaces;

class Program
{
    static void Main()
    {
        var serviceProvider = new ServiceCollection()
            .AddDbContext<AppDbContext>(options => options.UseSqlite("Data Source=test.db"))
            .AddScoped<IRabbitMQService, RabbitMQService>()
            .BuildServiceProvider();

        using (var scope = serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var rabbitMQService = new RabbitMQService(dbContext);
            rabbitMQService.ReceiveMessages();

            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();
        }
    }
}