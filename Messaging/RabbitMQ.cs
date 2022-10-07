using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;

namespace Demo.RabbitMQ.Notifications.Messaging;

public class RabbitMQ
{
    public static event Func<string, Task> MessageReceived;

    public async void Setup(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory() { HostName = "host.docker.internal" };
        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            channel.QueueDeclare(queue: "user-queue",
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                MessageReceived.Invoke(message);
            };
            channel.BasicConsume(queue: "user-queue",
                                 autoAck: true,
                                 consumer: consumer);

            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000);
            }
        }
    }
}