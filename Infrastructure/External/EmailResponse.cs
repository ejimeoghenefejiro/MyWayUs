using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.External
{
    // Response Object
    public class EmailResponse
    {
        public bool IsSuccessful { get; }  // Renamed property
        public string? MessageId { get; }
        public string? ErrorMessage { get; }  // Renamed property

        private EmailResponse(bool isSuccessful, string? messageId, string? errorMessage)
        {
            IsSuccessful = isSuccessful;
            MessageId = messageId;
            ErrorMessage = errorMessage;
        }

        public static EmailResponse Success(string? messageId) => new(true, messageId, null);
        public static EmailResponse Failure(string error) => new(false, null, error);
    }
}
