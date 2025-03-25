using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class EmailRequest
    {
        public string From { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool IsHtml { get; set; } = true;
        //public string Body { get; set; } = null!;
        public bool IsHtmlBody { get; set; } = true;
        public List<string>? Cc { get; set; }
        public List<string>? Bcc { get; set; }
        public List<FileData>? Attachments { get; set; }
        public bool TrackOpens { get; set; } = true;
        public bool TrackClicks { get; set; } = true;
    }

    public record FileData
    {
        public string FileName { get; set; } = null!;
        public byte[] Content { get; set; } = null!;
        public string ContentType { get; set; } = "application/octet-stream";
    }

}
