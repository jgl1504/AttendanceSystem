using System.Text;
using Microsoft.EntityFrameworkCore;
using WebApp.Server.Data;

namespace WebApp.Server.Services.Background;

public class DailyAbsentReporter
{
    private readonly DataContext _context;
    private readonly SmtpEmailSenderService _email;

    public DailyAbsentReporter(DataContext context, SmtpEmailSenderService email)
    {
        _context = context;
        _email = email;
    }

    public async Task SendTodayReportAsync(CancellationToken token = default)
    {
        // Skip Sundays
        if (DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
            return;

        var today = DateTime.Now.Date;
        var startUtc = DateTime.SpecifyKind(today, DateTimeKind.Local).ToUniversalTime();
        var endUtc = startUtc.AddDays(1);

        // Cutoff: 08:00 local time converted to UTC
        var cutoffLocal = today.AddHours(8);
        var cutoffUtc = DateTime.SpecifyKind(cutoffLocal, DateTimeKind.Local).ToUniversalTime();

        // All active employees with their department
        var employees = await _context.Employees
            .Include(e => e.Department)
            .Where(e => e.IsActive)
            .ToListAsync(token);

        // All employee IDs that clocked in before 08:00 today
        var clockedInIds = await _context.AttendanceRecords
            .Where(a => a.ClockInTime >= startUtc && a.ClockInTime < cutoffUtc)
            .Select(a => a.EmployeeId)
            .Distinct()
            .ToListAsync(token);

        // Employees that did NOT clock in by 08:00
        var absent = employees
            .Where(e => !clockedInIds.Contains(e.Id))
            .OrderBy(e => e.Department?.Name ?? "Unknown")
            .ThenBy(e => e.Name)
            .ToList();

        if (!absent.Any())
            return; // Everyone clocked in, no email needed

        // Build email body grouped by department
        var body = new StringBuilder();
        body.AppendLine($"Employees not clocked in by 08:00 on {today:yyyy-MM-dd}");
        body.AppendLine(new string('=', 50));
        body.AppendLine();

        var grouped = absent
            .GroupBy(e => e.Department?.Name ?? "Unknown")
            .OrderBy(g => g.Key);

        foreach (var dept in grouped)
        {
            body.AppendLine($"Department: {dept.Key}");
            body.AppendLine(new string('-', 30));

            foreach (var e in dept)
            {
                var phone = string.IsNullOrWhiteSpace(e.Phone) ? "No phone" : e.Phone;
                body.AppendLine($"  {e.Name}  |  {phone}");
            }

            body.AppendLine();
        }

        body.AppendLine(new string('=', 50));
        body.AppendLine($"Total absent: {absent.Count}");

        await _email.SendAsync(
            to: "projects@aics.co.za,jacques@aics.co.za",
            subject: $"Absent employees {today:yyyy-MM-dd}",
            body: body.ToString()
        );
    }
}
