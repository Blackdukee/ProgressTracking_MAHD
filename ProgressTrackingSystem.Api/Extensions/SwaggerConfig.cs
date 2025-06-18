using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Reflection;

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
                c.AddServer(new OpenApiServer { Url = "http://3.70.227.2:5004/api/v1/progress" });
                c.AddServer(new OpenApiServer { Url = "http://localhost:5004/api/v1/progress" });

                // Set the comments path for the Swagger JSON and UI
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
                
                // Enable annotations for Swagger
                c.EnableAnnotations();
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
        }        public static void UseSwaggerUI(this WebApplication app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Progress Tracking System API V1");
                c.RoutePrefix = "swagger";
                c.DocumentTitle = "PTS API Documentation";
                
                // Add better descriptions for authentication
                c.DefaultModelExpandDepth(2);
                c.DefaultModelRendering(Swashbuckle.AspNetCore.SwaggerUI.ModelRendering.Model);
                c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
                c.EnableDeepLinking();
                c.DisplayRequestDuration();
                
                // Add custom CSS to make the Authorize button more noticeable
                c.InjectStylesheet("/swagger-ui/styles.css");
                
                // Add a custom message about authentication at the top
                c.DocumentTitle = "Progress Tracking System API - Swagger UI";
            });
              // Define a simple CSS file endpoint
            app.MapGet("/swagger-ui/styles.css", () =>
            {
                var css = @"
.swagger-ui .auth-wrapper .authorize {
    background-color: #49cc90;
    color: #fff;
    border-color: #49cc90;
    font-size: 14px;
    font-weight: bold;
    padding: 5px 23px;
    border-radius: 4px;
}

.swagger-ui .auth-container input {
    min-width: 350px;
}

.swagger-ui .opblock-tag-section {
    margin-top: 10px;
}

.swagger-ui .information-container:after {
    content: ""Authentication Required: Click the Authorize button above and enter your JWT token with the format 'Bearer your-token-here'"";
    display: block;
    background-color: #e8f5e9;
    color: #004d40;
    padding: 15px;
    margin-top: 20px;
    border-radius: 4px;
    font-size: 14px;
    font-weight: bold;
}";
                return Results.Content(css, "text/css");
            });
        }
    }
}