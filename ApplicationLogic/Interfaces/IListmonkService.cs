using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ApplicationLogic.Interfaces
{
    public interface IListmonkService
    {
        Task<SubscriptionResult> SubscribeAsync(SubscriberRequest request);
        Task<bool> UnsubscribeAsync(string email);
        Task<List<ListmonkList>> GetListsAsync();
        Task SendCampaignViaZeptoAsync(CampaignRequest request);
    }

    public record SubscriptionResult(bool Success, string? Id, string? Error);
    public record SubscriberRequest(string Email, string Name, List<int> ListIds);
    public record CampaignRequest(string Subject, string Content, int ListId);
   // public record ListmonkList(int Id, string Name, string Type);



    // Supporting DTOs
    public record SubscriberSearchResponse(ListmonkSubscriberData Data);
    public record ListmonkSubscriberData(List<ListmonkSubscriber> Subscribers);
    public record ListmonkSubscriber(int Id, string Email, string Status);

    public record ListmonkListsResponse(ListmonkListData Data);
    public record ListmonkListData(List<ListmonkList> Results);

    public record ListmonkList(
        int Id,
        string Name,
        string Type,
        [property: JsonPropertyName("optin")] string OptIn,
        string Tags,
        [property: JsonPropertyName("created_at")] DateTime CreatedAt,
        [property: JsonPropertyName("updated_at")] DateTime UpdatedAt);
    public class ListmonkSubscriberResponse
    {
        [JsonPropertyName("data")]
        public SubscriberData? Data { get; set; }

        public class SubscriberData
        {
            [JsonPropertyName("results")]
            public List<Subscriber>? Subscribers { get; set; }
        }

        public class Subscriber
        {
            [JsonPropertyName("id")]
            public int Id { get; set; }

            [JsonPropertyName("email")]
            public string? Email { get; set; }
        }
    }
}
