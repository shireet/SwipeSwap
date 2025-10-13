using SwipeSwap.WebApi;

public class Program
{
    public static void Main(string[] args)
    {
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                config.AddEnvironmentVariables();
            })
            .ConfigureWebHostDefaults(builder => builder.UseStartup<Startup>())
            .Build()
            .Run();
    }
}