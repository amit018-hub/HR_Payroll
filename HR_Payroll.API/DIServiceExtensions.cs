using HR_Payroll.API.Config;
using HR_Payroll.API.JWTExtension;
using HR_Payroll.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace HR_Payroll.API
{
    public static class DIServiceExtensions
    {
        private const string CorsPolicy = "_MyAllowDomainPolicy";
        private const string JwtIdentitySetting = "JwtIdentitySetting";

        public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
        {
            // 1. Configure DbContext with SQL Server
            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    sqlServerOptions =>
                    {
                        sqlServerOptions.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(5),
                            errorNumbersToAdd: null
                        );
                        sqlServerOptions.CommandTimeout(30);
                    }
                );
            });

            // 2. Configure JWT Identity Settings with validation
            var identityServerSettings = builder.Configuration
                .GetSection(JwtIdentitySetting)
                .Get<JwtIdentitySetting>();

            if (identityServerSettings == null)
            {
                throw new InvalidOperationException(
                    $"Configuration section '{JwtIdentitySetting}' is missing or invalid.");
            }

            if (string.IsNullOrEmpty(identityServerSettings.Secret))
            {
                throw new InvalidOperationException("JWT Secret is not configured.");
            }

            builder.Services.Configure<JwtIdentitySetting>(
                builder.Configuration.GetSection(JwtIdentitySetting));
            builder.Services.AddSingleton(identityServerSettings);

            // 3. Add Core Services
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            builder.Services.ConfigureDIServices();

            // 4. Configure CORS
            //builder.Services.AddCors(options => options.AddPolicy(CorsPolicy, builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(CorsPolicy, policy =>
                {
                    policy.WithOrigins(identityServerSettings.AllowedOrigins)
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials(); // Add if using cookies/credentials
                });
            });

            // 5. Configure JWT Authentication
            var key = Encoding.UTF8.GetBytes(identityServerSettings.Secret);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = identityServerSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = identityServerSettings.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero // Remove default 5-minute tolerance
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";

                        var message = context.Exception is SecurityTokenExpiredException
                            ? "Token expired. Please login again."
                            : "Invalid token.";

                        var response = new { status = false, message };
                        var json = System.Text.Json.JsonSerializer.Serialize(response);
                        
                        return context.Response.WriteAsync(json);
                    },
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";

                        var response = new { status = false, message = "Unauthorized access" };
                        var json = System.Text.Json.JsonSerializer.Serialize(response);
                        
                        return context.Response.WriteAsync(json);
                    },
                    OnForbidden = context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/json";

                        var response = new { status = false, message = "Forbidden" };
                        var json = System.Text.Json.JsonSerializer.Serialize(response);
                        
                        return context.Response.WriteAsync(json);
                    }
                };
            });

            builder.Services.AddAuthorization();

            // 6. Add Controllers and API Documentation
            //builder.Services.AddControllers().AddNewtonsoftJson(options =>{options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;});
            builder.Services.AddControllers().AddNewtonsoftJson();
            builder.Services.AddEndpointsApiExplorer();
            RegisterSwaggerDocumentation(builder.Services);

            return builder.Build();
        }

        public static WebApplication ConfigurePipeline(this WebApplication app)
        {
            // 1. Development-only middleware
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "DRCTS Payroll API v1");
                });
            }

            // 2. Global exception handler (before other middleware)
            app.ConfigureExceptionHandler();

            // 3. HTTPS Redirection
            app.UseHttpsRedirection();

            // 4. Static Files
            app.UseStaticFiles();

            // 5. Routing
            app.UseRouting();

            // 6. CORS (must be after UseRouting and before UseAuthentication)
            app.UseCors(CorsPolicy);

            // 7. Authentication & Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            // 8. Custom status code pages
            app.ConfigureRedundantStatusCodePages();
            //app.UseHttpContextHelper(); // Helper to get Base URL anywhere in application
            //InitializeRoles(app.Services).Wait();
            //InitializeUser(app.Services).Wait();


            // 9. Diagnostic middleware (development only)
            if (app.Environment.IsDevelopment())
            {
                app.Use(async (context, next) =>
                {
                    var startTime = DateTime.UtcNow;
                    Console.WriteLine($"[{startTime:HH:mm:ss}] Request: {context.Request.Method} {context.Request.Path}");
                    
                    try
                    {
                        await next();
                        var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                        Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] Response: {context.Response.StatusCode} ({duration}ms)");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[ERROR] {ex.Message}");
                        throw;
                    }
                });
            }

            // 10. Map Controllers
            app.UseEndpoints(endpoints =>
            {
                _ = endpoints.MapControllers();
            });

            return app;
        }

        private static void RegisterSwaggerDocumentation(IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "DRCTS Payroll API",
                    Description = "ASP.NET Core Web API for managing HR and Payroll operations",
                    Contact = new OpenApiContact
                    {
                        Name = "Support Team",
                        Email = "support@drcts.com"
                    }
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Enter 'Bearer' [space] and then your valid token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                        Array.Empty<string>()
                    }
                });
            });
        }
    }
}