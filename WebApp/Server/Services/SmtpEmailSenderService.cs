using System.Net;
using System.Net.Mail;

public class SmtpEmailSenderService
{
    private readonly IConfiguration _config;

    public SmtpEmailSenderService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendAsync(string to, string subject, string body)
    {
        var host = _config["Smtp:Host"];
        var port = int.Parse(_config["Smtp:Port"]);
        var user = _config["Smtp:User"];
        var pass = _config["Smtp:Pass"];
        var from = _config["Smtp:From"];

        var addresses = to.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        using var message = new MailMessage
        {
            From = new MailAddress(from),
            Subject = subject,
            Body = body
        };

        foreach (var addr in addresses)
        {
            message.To.Add(addr);
        }

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(user, pass)
        };

        await client.SendMailAsync(message);
    }

}
