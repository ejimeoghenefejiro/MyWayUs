using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOs
{
    public class ZeptoMailConfig
    {
        public const string SectionName = "ZeptoMail";

        public string FromEmail { get; set; }
        public string ApiKey { get; set; }
    }
}
