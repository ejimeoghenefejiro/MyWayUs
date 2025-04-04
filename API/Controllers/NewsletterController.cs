using ApplicationLogic.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController] 
   // [Route("api/newsletter")]
    public class NewsletterController : ControllerBase
    {
        private readonly IListmonkService _listmonk;
        private readonly IZeptoMailService _zepto;

        public NewsletterController(
            IListmonkService listmonk,
            IZeptoMailService zepto)
        {
            _listmonk = listmonk;
            _zepto = zepto;
        }

        [HttpPost("subscribe")]
        public async Task<IActionResult> Subscribe([FromBody] SubscriberRequest request)
        {
            var result = await _listmonk.SubscribeAsync(new SubscriberRequest(
                request.Email,
                request.Name,
                new List<int> { 1 } // Default list ID
            ));

            if (!result.Success)
                return BadRequest(result.Error);

            // Send confirmation email via Zepto
            await _zepto.SendEmailAsync(new EmailRequest
            {
                From = "noreply@yourdomain.com",
                To = request.Email,
                Subject = "Subscription Confirmed",
                Body = "<h1>Welcome!</h1><p>Thanks for subscribing</p>",
                IsHtmlBody = true
            });
            return Ok(new { result.Id });
        }

    }
}
