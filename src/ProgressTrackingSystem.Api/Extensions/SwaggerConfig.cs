using Microsoft.OpenApi.Models;

namespace ProgressTrackingSystem.Extensions
{
    /// <summary>
    /// Extension methods for configuring Swagger/OpenAPI.
    /// </summary>
    public static class SwaggerConfig
    {
        public static void AddSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Progress Tracking System API",
                    Version = "v1",
                    Description = "API for tracking user progress and course enrollments in an e-learning platform."
                });
                c.AddServer(new OpenApiServer { Url = "http://localhost:5004" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter JWT with Bearer into field (e.g., 'Bearer {token}')",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });
        }

        public static void UseSwaggerUI(this WebApplication app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Progress Tracking System API V1");
                c.RoutePrefix = "swagger";
                c.DocumentTitle = "PTS API Documentation";
            });
        }
    }
}