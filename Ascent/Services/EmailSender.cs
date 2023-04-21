using Ascent.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MimeKit;

namespace Ascent.Services;

public class EmailSettings
{
    public string Host { get; set; }
    public int Port { get; set; }
    public bool RequireAuthentication { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public int MaxRecipientsPerMessage { get; set; } = 30;
    public string SenderName { get; set; }
    public string SenderEmail { get; set; }
}

public class EmailSender
{
    private readonly EmailSettings _settings;

    private ILogger<EmailSender> _logger;

    public EmailSender(IOptions<EmailSettings> settings, ILogger<EmailSender> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public bool Send(Message message, List<(string Name, string Email)> recipients, string senderName = null)
    {
        List<MimeMessage> messages = new List<MimeMessage>();

        MimeMessage msg = new MimeMessage();
        for (int i = 0; i < recipients.Count; ++i)
        {
            if (i % _settings.MaxRecipientsPerMessage == 0)
            {
                msg = new MimeMessage();
                msg.From.Add(new MailboxAddress(senderName ?? _settings.SenderName, _settings.SenderEmail));
                msg.ReplyTo.Add(new MailboxAddress(message.Author.Name, message.Author.Email));
                msg.To.Add(new MailboxAddress(message.Author.Name, message.Author.Email));
                msg.Subject = message.Subject;
                msg.Body = new TextPart("html") { Text = message.Content };
                messages.Add(msg);
            }

            if (!message.Author.Email.Equals(recipients[i].Email, StringComparison.OrdinalIgnoreCase))
            {
                if (message.UseBcc)
                    msg.Bcc.Add(new MailboxAddress(recipients[i].Name, recipients[i].Email));
                else
                    msg.To.Add(new MailboxAddress(recipients[i].Name, recipients[i].Email));
            }
        }

        return Send(messages);
    }

    public bool Send(List<MimeMessage> messages)
    {
        var success = true;
        using var client = new SmtpClient();

        try
        {
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;
            client.Connect(_settings.Host, _settings.Port, SecureSocketOptions.None);
            if (_settings.RequireAuthentication)
                client.Authenticate(_settings.Username, _settings.Password);

            foreach (var msg in messages)
            {
                client.Send(msg);
                _logger.LogInformation("Message [{subject}] sent to {receipients}", msg.Subject, msg.Bcc.IsNullOrEmpty() ? msg.To : msg.Bcc);
            }


            client.Disconnect(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email error");
            success = false;
        }

        return success;
    }
}
