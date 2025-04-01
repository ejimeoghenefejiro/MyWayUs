using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOs
{
    public class ListmonkConfig
    {
        public const string SectionName = "Listmonk";

        [Required]
        [Url]
        public string BaseUrl { get; set; }

        [Required]
        public string ApiKey { get; set; }

        public List<int> DefaultListIds { get; set; } = new();
    }
}
