using CommonContracts;
using System.Text.Json;
using RabbitMQ.Client;

namespace CustomMassTransitProducer.Services
{

    public interface IProduceService
    {
        Task ProduceMessageAsync(string messageText);
    }


    public class ProduceService(ILogger<ProduceService> logger) : IProduceService
    {
        private const string QueueName = "customQueue";
        private const bool PersistMessages = true;
        public async Task ProduceMessageAsync(string messageText)
        {
            try
            {
                var message = new CustomMessage(messageText);

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

                var body = JsonSerializer.SerializeToUtf8Bytes(message);

                await channel.BasicPublishAsync(
                    exchange: string.Empty,
                    routingKey: QueueName,
                    mandatory: true,
                    basicProperties: new BasicProperties { Persistent = PersistMessages },
                    body: body
                );

            }
            catch(Exception ex)
            {
                logger.LogError(ex, "Error producing message : {Error}", ex.Message);
            }

        }
    }
}
