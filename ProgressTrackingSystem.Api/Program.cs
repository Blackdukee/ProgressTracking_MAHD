using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Polly;
using Polly.Extensions.Http;
using ProgressTrackingSystem.Data;
using ProgressTrackingSystem.Extensions;
using ProgressTrackingSystem.Services;
using ProgressTrackingSystem.Services.Interfaces;
using System.Net.Http;
using System.Text;

namespace ProgressTrackingSystem
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.WebHost.UseUrls("http://0.0.0.0:5004");

            // Configure HttpClients with null safety
            var umsBaseUrl = builder.Configuration["Ums:BaseUrl"];
            var umsServerKey = builder.Configuration["Ums:ServerKey"];
            if (string.IsNullOrEmpty(umsBaseUrl))
                throw new InvalidOperationException("Ums:BaseUrl configuration is required");
            if (string.IsNullOrEmpty(umsServerKey))
                throw new InvalidOperationException("Ums:ServerKey configuration is required");

            builder.Services.AddHttpClient("UmsApiClient", client =>
            {
                client.BaseAddress = new Uri(umsBaseUrl);
                client.DefaultRequestHeaders.Add("X-Server-Key", umsServerKey);
            }).AddPolicyHandler(GetRetryPolicy());

            var cmsBaseUrl = builder.Configuration["Cms:BaseUrl"];
            var cmsServerKey = builder.Configuration["Cms:ServerKey"];
            if (string.IsNullOrEmpty(cmsBaseUrl))
                throw new InvalidOperationException("Cms:BaseUrl configuration is required");
            if (string.IsNullOrEmpty(cmsServerKey))
                throw new InvalidOperationException("Cms:ServerKey configuration is required");

            builder.Services.AddHttpClient("CmsApiClient", client =>
            {
                client.BaseAddress = new Uri(cmsBaseUrl);
                client.DefaultRequestHeaders.Add("X-Server-Key", cmsServerKey);
            }).AddPolicyHandler(GetRetryPolicy());

            var enrollmentBaseUrl = builder.Configuration["EnrollmentApi:BaseUrl"];
            var enrollmentServerKey = builder.Configuration["EnrollmentApi:ServerKey"];
            if (string.IsNullOrEmpty(enrollmentBaseUrl))
                throw new InvalidOperationException("EnrollmentApi:BaseUrl configuration is required");
            if (string.IsNullOrEmpty(enrollmentServerKey))
                throw new InvalidOperationException("EnrollmentApi:ServerKey configuration is required");

            builder.Services.AddHttpClient("EnrollmentApiClient", client =>
            {
                client.BaseAddress = new Uri(enrollmentBaseUrl);
                client.DefaultRequestHeaders.Add("X-Server-Key", enrollmentServerKey);
            }).AddPolicyHandler(GetRetryPolicy());

            // Configure DbContext
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException("Default connection string is required");

            builder.Services.AddDbContext<ProgressDbContext>(options =>
                options.UseSqlServer(connectionString));

            // Configure In-Memory Caching
            builder.Services.AddMemoryCache();
            builder.Services.AddScoped<ICacheService, CacheService>();

            // Configure Services
            builder.Services.AddScoped<IProgressService, ProgressService>();
            builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
            builder.Services.AddScoped<IUmsApiService, UmsApiService>();
            builder.Services.AddScoped<ICmsApiService, CmsApiService>();
            builder.Services.AddScoped<IAuditLogService, AuditLogService>();


            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            });

            // Configure Controllers
            builder.Services.AddControllers();

            // Configure Swagger
            builder.Services.AddSwagger(); // Use extension method from SwaggerConfig.cs

            builder.Services.AddAuthorization();
            builder.Services.AddCustomMiddleware();
            builder.Services.AddRateLimiter(options =>
            {
                options.AddFixedWindowLimiter("Api", opt =>
                {
                    opt.PermitLimit = 100;
                    opt.Window = TimeSpan.FromSeconds(10);
                });
            });

            // Add Application Insights if configured
            var appInsightsConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
            if (!string.IsNullOrEmpty(appInsightsConnectionString))
            {
                builder.Services.AddApplicationInsightsTelemetry();
            }
            builder.Services.AddHttpContextAccessor();

            var app = builder.Build(); app.UseRouting();
            // Our custom middleware handles token validation with UMS API
            app.UseCustomMiddleware();
            // Keep the built-in authentication for compatibility with [Authorize] attributes
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseRateLimiter();
            app.UseSwaggerUI(); // Use extension method from SwaggerConfig.cs

            app.Urls.Add("http://0.0.0.0:5004");

            app.MapControllers();
            // // Temporary code to generate a test token
            // var tokenGenerator = new TokenGenerator(
            //     builder.Configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer is not configured"),
            //     builder.Configuration["Jwt:Audience"] ?? throw new InvalidOperationException("Jwt:Audience is not configured"),
            //     builder.Configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("Jwt:SecretKey is not configured")
            // );
            // var testToken = tokenGenerator.GenerateJwtToken("test-user-123");
            // Console.WriteLine("Generated JWT Token:");
            // Console.WriteLine(testToken);
            // // End temporary code

            app.Run();
        }

        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }
    }
}