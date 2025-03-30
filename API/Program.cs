using System.Net.Http.Headers;
using System.Text;
using System.Threading.RateLimiting;
using ApplicationLogic.Interfaces;
using ApplicationLogic.Services;
using Domain.DTOs;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


// Add services
builder.Services.AddScoped<IZeptoMailService, ZeptoMailService>();
builder.Services.Configure<ZeptoMailConfig>( builder.Configuration.GetSection(ZeptoMailConfig.SectionName));
builder.Services.AddScoped<IPaymentService, PaymentService>();
//builder.Services.AddDbContext<AppDbContext>(options =>
   // options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// Configure HttpClient for Zeptomail
builder.Services.AddHttpClient<IZeptoMailService, ZeptoMailService>(client =>
{
    client.BaseAddress = new Uri("https://api.zeptomail.zoho.com/v1/");
    client.DefaultRequestHeaders.Add("Authorization",
        $"Zoho-encryptedAPI {Environment.GetEnvironmentVariable("ZEPTOMAIL_API_KEY")}");
});


//builder.Services.AddHttpClient<IZeptoMailService, ZeptoMailService>(client =>
//{
//    client.BaseAddress = new Uri("https://api.zeptomail.com/");
//    client.DefaultRequestHeaders.Add("Authorization",
//        $"Zoho-encryptedAPI {builder.Configuration["ZeptoMail:ApiKey"]}");
//    client.DefaultRequestHeaders.Accept.Add(
//        new MediaTypeWithQualityHeaderValue("application/json"));
//});
builder.Services.AddHttpClient<IListmonkService, ListmonkService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Listmonk:BaseUrl"]);
    client.DefaultRequestHeaders.Add("Authorization",
        $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes(builder.Configuration["Listmonk:ApiKey"]))}");
});

//builder.Services.AddRateLimiter(options =>
//{
//    options.AddPolicy<ZeptoMailPolicy>("zeptomail", context =>
//        RateLimitPartition.GetTokenBucketLimiter(
//            partitionKey: context.Connection.RemoteIpAddress?.ToString(),
//            factory: _ => new TokenBucketRateLimiterOptions
//            {
//                TokenLimit = 100,
//                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
//                QueueLimit = 10,
//                ReplenishmentPeriod = TimeSpan.FromMinutes(1),
//                TokensPerPeriod = 100,
//                AutoReplenishment = true
//            }));
//});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.MapHealthChecks("/health/zeptomail", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("zeptomail")
});

app.MapHealthChecks("/health/listmonk", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("listmonk")
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
