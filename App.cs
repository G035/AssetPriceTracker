using System.CommandLine;
using System.Collections.Generic;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using AssetPriceTracker.Asset.Application.Commands;
using AssetPriceTracker.Asset.Application.Queries;
using AssetPriceTracker.Asset.Infastructure.Web;

using AssetPriceTracker.Notification.Application.Commands;
using AssetPriceTracker.Notification.Application.Services;
using AssetPriceTracker.Notification.Infrastructure;

namespace AssetPriceTracker;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();

        // TODO: Brazillian price parsing (comma instead of dot)
        var assetName = new Argument<string>("asset", "Asset name e.g. PETR4");
        var sellPrice = new Argument<decimal>("sellPrice", "Sell price in the adequate currency (BRL for PETR4) e.g. 43.21");
        var buyPrice = new Argument<decimal>("buyPrice", "Buy price in the adequate currency (BRL for PETR4) e.g. 12.34");

        var rootCommand = new RootCommand("Asset price tracking\n This application will continuously monitor prices for the asset you have selected. It will send emails to the recipients list when the asset price is not in the defined range.")
        {
            assetName,
            sellPrice,
            buyPrice
        };

        rootCommand.SetHandler(async context =>
        {
            using var scope = host.Services.CreateScope();

            while (true)
            {
                var handler = scope.ServiceProvider.GetRequiredService<TrackAssetCommandHandler>();
                await handler.Handle(
                    context.ParseResult.GetValueForArgument(assetName),
                    context.ParseResult.GetValueForArgument(sellPrice),
                    context.ParseResult.GetValueForArgument(buyPrice)
                );
                await Task.Delay(TimeSpan.FromSeconds(
                    host.Services
                        .GetRequiredService<IConfiguration>()
                        .GetValue<int>("PriceRefreshPeriodInSeconds")
                ));
            }
        });

        return await rootCommand.InvokeAsync(args);
    }

    static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                config.AddEnvironmentVariables();
                config.AddCommandLine(args);
            })
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton<ILogger>(sp =>
                {
                    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                    return loggerFactory.CreateLogger("Global"); ;
                });

                services.AddSingleton<IFetchAssetPrice>(sp =>
                {
                    return new TwelveDataProvider(
                        sp.GetRequiredService<ILogger<TwelveDataProvider>>(),
                        context.Configuration.GetValue<string>("ApiKeys:TwelveData")
                            ?? throw new InvalidOperationException("Missing configuration: ApiKeys:TwelveData")
                    );
                });
                services.AddSingleton<INotificationService>(sp =>
                {
                    return new EmailNotifier(
                        sp.GetRequiredService<ILogger<EmailNotifier>>(),
                        context.Configuration.GetValue<string>("SmtpCredentials:Host")
                            ?? throw new InvalidOperationException("Missing configuration: SmtpCredentials:Host"),
                        context.Configuration.GetValue<int>("SmtpCredentials:Port"),
                        context.Configuration.GetValue<string>("SmtpCredentials:Email")
                            ?? throw new InvalidOperationException("Missing configuration: SmtpCredentials:Email"),
                        context.Configuration.GetValue<string>("SmtpCredentials:Password")
                            ?? throw new InvalidOperationException("Missing configuration: SmtpCredentials:Password"),
                        context.Configuration.GetValue<bool>("SmtpCredentials:Ssl")
                    );
                });
                services.AddScoped<TrackAssetCommandHandler>();
                services.AddScoped<NotifyAssetPriceReachedSellPriceCommandHandler>(sp =>
                {
                    return new NotifyAssetPriceReachedSellPriceCommandHandler(
                        sp.GetRequiredService<INotificationService>(),
                        context.Configuration.GetSection("EmailAddresses").Get<List<string>>()
                            ?? throw new InvalidOperationException("Missing configuration: EmailAddresses")
                    );
                });
                services.AddScoped<NotifyAssetPriceReachedBuyPriceCommandHandler>(sp =>
                {
                    return new NotifyAssetPriceReachedBuyPriceCommandHandler(
                        sp.GetRequiredService<INotificationService>(),
                        context.Configuration.GetSection("EmailAddresses").Get<List<string>>()
                            ?? throw new InvalidOperationException("Missing configuration: EmailAddresses")
                    );
                });
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Information);
            });
}