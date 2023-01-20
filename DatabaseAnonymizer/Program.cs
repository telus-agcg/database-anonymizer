using DatabaseAnonymizer;
using DataMasker.Models;
using Serilog;

internal class Program
{
    private static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("logs/anonymizer.log", restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information, rollingInterval: RollingInterval.Day)
                .CreateLogger();

        Log.Information(new string('-', 30));
        Log.Information("DataAnonymizer started");
        Log.Information(new string('-', 30));

        Config config = Config.Load("configAdventureWorks.example.json");

        var specificScripts = File
                .ReadAllText("SpecificScripts.sql")
                .Trim()
                .Replace(Environment.NewLine, " ")
                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

        try
        {
            var datamasker = new DataAnonymizer(config, specificScripts);
            await datamasker.Start();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occured while anonymization process");
            throw;
        }

        Log.Information("Successfully finished anonymization.");
        Log.Information("Press Enter to exit");
        Console.ReadLine();
    }
}