using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class ZeptoMailPayload
    {
        public ZeptoMailPayload(EmailRequest request)
        {
            From = new EmailAddress(request.From);
            To = new List<EmailAddress> { new(request.To) };
            Subject = request.Subject;
            HtmlBody = request.IsHtmlBody ? request.Body : null;
            TextBody = request.IsHtmlBody ? null : request.Body;
            Attachments = request.Attachments?.Select(a => new Attachment(a)).ToList();
            TrackOpens = request.TrackOpens;
            TrackClicks = request.TrackClicks;
        }

        [Required]
        [JsonPropertyName("from")]
        public EmailAddress From { get; set; }

        [Required]
        [MinLength(1)]
        [JsonPropertyName("to")]
        public List<EmailAddress> To { get; set; }

        [JsonPropertyName("cc")]
        public List<EmailAddress>? Cc { get; set; }

        [JsonPropertyName("bcc")]
        public List<EmailAddress>? Bcc { get; set; }

        [Required]
        [StringLength(998)] // RFC 5322 limit
        [JsonPropertyName("subject")]
        public string Subject { get; set; }

        [JsonPropertyName("htmlbody")]
        public string? HtmlBody { get; set; }

        [JsonPropertyName("textbody")]
        public string? TextBody { get; set; }

        [JsonPropertyName("attachments")]
        public List<Attachment>? Attachments { get; set; }

        [JsonPropertyName("track_opens")]
        public bool TrackOpens { get; set; } = true;

        [JsonPropertyName("track_clicks")]
        public bool TrackClicks { get; set; } = true;

        public bool Validate(out List<string> errors)
        {
            errors = new List<string>();
            var context = new ValidationContext(this);
            var results = new List<ValidationResult>();

            if (!Validator.TryValidateObject(this, context, results, true))
            {
                errors.AddRange(results.Select(r => r.ErrorMessage!));
            }

            // Custom validation
            if (string.IsNullOrWhiteSpace(HtmlBody) && string.IsNullOrWhiteSpace(TextBody))
            {
                errors.Add("Either HTML or Text body must be provided");
            }

            return !errors.Any();
        }

        public record EmailAddress
        {
            public EmailAddress(string email) : this(email, null) { }

            public EmailAddress(string email, string? name)
            {
                Address = email;
                Name = name;
            }

            [EmailAddress]
            [Required]
            [JsonPropertyName("address")]
            public string Address { get; }

            [JsonPropertyName("name")]
            public string? Name { get; }
        }

        public record Attachment
        {
            public Attachment(FileData file)
            {
                Content = Convert.ToBase64String(file.Content);
                Name = file.FileName;
                ContentType = file.ContentType;
            }

            [Required]
            [JsonPropertyName("content")]
            public string Content { get; }

            [Required]
            [JsonPropertyName("name")]
            public string Name { get; }

            [Required]
            [JsonPropertyName("content_type")]
            public string ContentType { get; }
        }
    }
}
