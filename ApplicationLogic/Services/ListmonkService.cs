using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ApplicationLogic.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ApplicationLogic.Services
{

    public class ListmonkService : IListmonkService
    {
        private readonly HttpClient _httpClient;
        private readonly IZeptoMailService _zeptoMailService;
        private readonly ILogger<ListmonkService> _logger;
        private readonly IConfiguration _config;

        public ListmonkService(
            HttpClient httpClient,
            IZeptoMailService zeptoMailService,
            ILogger<ListmonkService> logger,
            IConfiguration config)
        {
            _httpClient = httpClient;
            _zeptoMailService = zeptoMailService;
            _logger = logger;
            _config = config;
        }

        public async Task<SubscriptionResult> SubscribeAsync(SubscriberRequest request)
        {
            try
            {
                var listmonkRequest = new
                {
                    email = request.Email,
                    name = request.Name,
                    status = "enabled",
                    lists = request.ListIds,
                    preconfirm_subscriptions = true
                };

                var response = await _httpClient.PostAsJsonAsync("/api/subscribers", listmonkRequest);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Listmonk subscription failed: {Error}", error);
                    return new SubscriptionResult(false, null, error);
                }

                var content = await response.Content.ReadFromJsonAsync<dynamic>();
                return new SubscriptionResult(true, content?.id.ToString(), null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Listmonk subscription error");
                return new SubscriptionResult(false, null, ex.Message);
            }
        }

        public async Task SendCampaignViaZeptoAsync(CampaignRequest request)
        {
            var subscribers = await GetSubscribersAsync(request.ListId);

            // Batch emails in groups of 100
            foreach (var batch in subscribers.Chunk(100))
            {
                var tasks = batch.Select(email => _zeptoMailService.SendEmailAsync(
                    new EmailRequest
                    {
                        From = _config["ZeptoMail:FromEmail"],
                        To = email,
                        Subject = request.Subject,
                        Body = request.Content,
                        IsHtmlBody = true
                    }));

                await Task.WhenAll(tasks);
            }
        }       
        private async Task<List<string>> GetSubscribersAsync(int listId)
        {
            var allSubscribers = new List<string>();
            int page = 1;
            bool morePages;
            do
            {
                var response = await _httpClient.GetAsync(
                    $"/api/subscribers?list_id={listId}&page={page}&per_page=100");

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadFromJsonAsync<ListmonkSubscriberResponse>();
                var emails = content?.Data?.Subscribers?.Select(s => s.Email)?.ToList() ?? new();

                allSubscribers.AddRange(emails);

                morePages = emails.Count >= 100;
                page++;
            } while (morePages);

            return allSubscribers;
        }

        public async Task<bool> UnsubscribeAsync(string email)
        {
            try
            {
                // First get subscriber ID by email
                var response = await _httpClient.GetAsync($"/api/subscribers?query=email:\"{email}\"");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to find subscriber {Email}. Status: {StatusCode}",
                        email, response.StatusCode);
                    return false;
                }

                var content = await response.Content.ReadFromJsonAsync<SubscriberSearchResponse>();
                var subscriber = content?.Data?.Subscribers?.FirstOrDefault();

                if (subscriber == null)
                {
                    _logger.LogWarning("Subscriber {Email} not found", email);
                    return false;
                }

                // Update subscriber status to unsubscribed
                var updateRequest = new
                {
                    status = "unsubscribed",
                    preconfirm_subscriptions = true
                };

                var updateResponse = await _httpClient.PutAsJsonAsync(
                    $"/api/subscribers/{subscriber.Id}",
                    updateRequest);

                if (!updateResponse.IsSuccessStatusCode)
                {
                    var error = await updateResponse.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to unsubscribe {Email}: {Error}", email, error);
                    return false;
                }

                _logger.LogInformation("Successfully unsubscribed {Email}", email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unsubscribing {Email}", email);
                return false;
            }
        }

        public async Task<List<ListmonkList>> GetListsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/lists?per_page=all");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadFromJsonAsync<ListmonkListsResponse>();
                return content?.Data?.Results ?? new List<ListmonkList>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving mailing lists");
                return new List<ListmonkList>();
            }
        }

       

        
    }
}
