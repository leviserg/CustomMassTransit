using CustomMassTransitProducer.Services;
using Microsoft.AspNetCore.Mvc;

namespace CustomMassTransitProducer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProducerController(IProduceService service) : ControllerBase
    {
        [HttpPost("produce")]
        public async Task<IActionResult> ProduceMessage([FromBody]string messageText)
        {
            try
            {
                await service.ProduceMessageAsync(messageText);
                return Ok($"Message '{messageText}' was produced");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
