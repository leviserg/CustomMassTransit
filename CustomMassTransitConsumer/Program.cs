using CommonContracts;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

// Consumer app can be started before the producer app - the Queue should exist
const string QueueName = "customQueue";
const bool PersistMessages = true;

var factory = new ConnectionFactory { HostName = "localhost" };

using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

await channel.QueueDeclareAsync(
    queue: QueueName,
    durable: PersistMessages, // keep message s in the queue even if RabbitMQ restarts
    exclusive: false,
    autoDelete: false,
    arguments: null
);

Console.WriteLine("Waiting for messages...");

var consumer = new AsyncEventingBasicConsumer(channel);

consumer.ReceivedAsync += async (sender, eventArgs) =>
{
    try
    {
        byte[] body = eventArgs.Body.ToArray();
        var messageBody = Encoding.UTF8.GetString(body);

        var message = JsonSerializer.Deserialize<CustomMessage>(messageBody);

        Console.WriteLine($"{message!.MessageDateTime}\t{message.Id}\t{message.MessageText}");

        await ((AsyncEventingBasicConsumer)sender).Channel.BasicAckAsync(
            deliveryTag: eventArgs.DeliveryTag,
            multiple: false
        );
    }
    catch(Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error processing message: {ex.Message}");
        await ((AsyncEventingBasicConsumer)sender).Channel.BasicNackAsync(
            deliveryTag: eventArgs.DeliveryTag,
            multiple: false,
            requeue: true // false for do not requeue the message
        );
        Console.ResetColor();
    }

};

await channel.BasicConsumeAsync(
    queue: QueueName,
    autoAck: false, // acknowledge messages manually by consumer
    consumer: consumer
);

Console.ReadLine();