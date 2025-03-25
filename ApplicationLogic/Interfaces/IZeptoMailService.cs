using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using ApplicationLogic.Services;
using Domain.Entities;
using Infrastructure.External;

namespace ApplicationLogic.Interfaces
{
    public interface IZeptoMailService
    {
        //Task<HttpResponseMessage> SendEmailAsync(EmailRequest request);
        Task<EmailResponse> SendEmailAsync(EmailRequest request);
    }
    
}
