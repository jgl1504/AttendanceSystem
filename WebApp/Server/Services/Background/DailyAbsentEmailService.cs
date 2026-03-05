using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebApp.Server.Data;
using WebApp.Server.Services.Background;

public class DailyAbsentEmailService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<DailyAbsentEmailService> _logger;

    public DailyAbsentEmailService(IServiceProvider services,
                                   ILogger<DailyAbsentEmailService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var lastRunDate = DateOnly.MinValue;

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.Now;
            var today = DateOnly.FromDateTime(now);

            // Run once per day at 08:00, skip Sundays
            if (today != lastRunDate && now.TimeOfDay >= new TimeSpan(8, 0, 0))
            {
                try
                {
                    using var scope = _services.CreateScope();
                    var reporter = scope.ServiceProvider.GetRequiredService<DailyAbsentReporter>();
                    await reporter.SendTodayReportAsync(stoppingToken);

                    lastRunDate = today;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while running DailyAbsentEmailService.");
                }
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
