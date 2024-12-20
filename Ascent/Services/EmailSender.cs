using Ascent.Models;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Ascent.Services;

public class EmailSettings
{
    public int MaxRecipientsPerMessage { get; set; } = 30;
    public string SenderName { get; set; }
    public string SenderEmail { get; set; }
}

public class EmailSender
{
    private readonly EmailSettings _settings;
    private readonly RabbitService _rabbitService;

    public EmailSender(IOptions<EmailSettings> settings, RabbitService rabbitService)
    {
        _settings = settings.Value;
        _rabbitService = rabbitService;
    }

    public async Task SendAsync(Message message, List<(string Name, string Email)> recipients, string senderName = null)
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

        await SendAsync(messages);
    }

    public async Task SendAsync(List<MimeMessage> messages) => await _rabbitService.SendAsync(messages);
}
