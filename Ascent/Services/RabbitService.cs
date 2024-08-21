using MessagePack;
using Microsoft.Extensions.Options;
using MimeKit;
using RabbitMQ.Client;

namespace Ascent.Services;

public class RabbitMQSettings
{
    public string HostName { get; set; } = "localhost";
    public string UserName { get; set; } = ConnectionFactory.DefaultUser;
    public string Password { get; set; } = ConnectionFactory.DefaultPass;
    public string QueueName { get; set; } = "alice-mail-service";
}

public class RabbitService : IDisposable
{
    private readonly RabbitMQSettings _settings;

    private readonly ConnectionFactory _factory;
    private readonly IConnection _connection;

    private readonly ILogger<RabbitService> _logger;

    public RabbitService(IOptions<RabbitMQSettings> settings, ILogger<RabbitService> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        _factory = new ConnectionFactory
        {
            HostName = _settings.HostName,
            UserName = _settings.UserName,
            Password = _settings.Password
        };
        _connection = _factory.CreateConnection();
        _logger.LogInformation("Connected to RabbitMQ server at {host}", _settings.HostName);
    }

    public void Send(MimeMessage message) => Send(new List<MimeMessage> { message });

    public void Send(List<MimeMessage> messages)
    {
        using var channel = _connection.CreateModel();
        channel.QueueDeclare(
            queue: _settings.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        // Alice Mail Service expects a list of byte arrays, each being an MimeMessage
        List<byte[]> messagesInBytes = messages.Select(msg =>
        {
            using (MemoryStream stream = new MemoryStream())
            {
                msg.WriteTo(stream);
                return stream.ToArray();
            }
        }).ToList();
        var body = MessagePackSerializer.Serialize(messagesInBytes);

        channel.BasicPublish(
            exchange: string.Empty,
            routingKey: _settings.QueueName,
            basicProperties: null,
            body: body
        );
        _logger.LogInformation("Publish {n} message to RabbitMQ queue {queue}", messages.Count, _settings.QueueName);
    }

    public void Dispose() => _connection.Dispose();
}
