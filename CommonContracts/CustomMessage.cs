using System.Text.Json.Serialization;

namespace CommonContracts
{
    public record CustomMessage(Guid Id, string MessageText, DateTime MessageDateTime)
    {
        [JsonConstructor]
        public CustomMessage(string messageText) : this(Guid.NewGuid(), messageText, DateTime.UtcNow)
        {
        }

    }
}
