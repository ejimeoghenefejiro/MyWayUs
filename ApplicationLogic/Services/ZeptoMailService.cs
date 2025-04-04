using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using ApplicationLogic.Interfaces;
using Domain.DTOs;
using Domain.Entities;
using Infrastructure.External;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ApplicationLogic.Services
{
    public class ZeptoMailService : IZeptoMailService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ZeptoMailService> _logger;
        //private readonly IConfiguration _config;
        private readonly ZeptoMailConfig _config;
        public ZeptoMailService(
            HttpClient httpClient,
            ILogger<ZeptoMailService> logger,
            IOptions<ZeptoMailConfig> config)
        {
            _httpClient = httpClient;
            _logger = logger;
            _config = config.Value;

            ValidateConfiguration();

        }
        private void ValidateConfiguration()
        {
            if (string.IsNullOrWhiteSpace(_config.FromEmail))
            {
                throw new ArgumentNullException(
                    nameof(ZeptoMailConfig.FromEmail),
                    "Missing required email configuration");
            }

            if (string.IsNullOrWhiteSpace(_config.ApiKey))
            {
                throw new ArgumentNullException(
                    nameof(ZeptoMailConfig.ApiKey),
                    "Missing API key configuration");
            }
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
                _httpClient.DefaultRequestHeaders.Authorization =
                   new AuthenticationHeaderValue("Bearer", _config.ApiKey);
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
