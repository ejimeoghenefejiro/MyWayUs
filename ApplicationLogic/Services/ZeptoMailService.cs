using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using ApplicationLogic.Interfaces;
using Domain.Entities;
using Infrastructure.External;
using Microsoft.Extensions.Logging;

namespace ApplicationLogic.Services
{
    public class ZeptoMailService : IZeptoMailService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ZeptoMailService> _logger;

        public ZeptoMailService(
            HttpClient httpClient,
            ILogger<ZeptoMailService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<EmailResponse> SendEmailAsync(EmailRequest request)
        {
            try
            {
                var payload = new ZeptoMailPayload(request);

                if (!payload.Validate(out var validationErrors))
                {
                    _logger.LogWarning("Validation failed for email request: {Errors}", validationErrors);
                    return EmailResponse.Failure($"Validation errors: {string.Join(", ", validationErrors)}");
                }

                var response = await _httpClient.PostAsJsonAsync("/v1.1/email", payload);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("ZeptoMail API Error: {StatusCode} - {Response}",
                        response.StatusCode, errorContent);

                    return EmailResponse.Failure($"API Error: {response.StatusCode} - {errorContent}");
                }

                var content = await response.Content.ReadFromJsonAsync<ZeptoMailApiResponse>();
                _logger.LogInformation("Email sent successfully. Message ID: {MessageId}", content?.MessageId);

                return EmailResponse.Success(content?.MessageId);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Critical error sending email");
                return EmailResponse.Failure($"Internal error: {ex.Message}");
            }
        }
    }

}
