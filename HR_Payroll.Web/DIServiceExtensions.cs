using HR_Payroll.Core.Services;
using HR_Payroll.Web.CommonClients;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Net.Http.Headers;

namespace HR_Payroll.Web
{
    public static class DIServiceExtensions
    {
        public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
        {
            // 1. Add MVC with Views
            builder.Services.AddControllersWithViews();

            // 2. Add Session Support
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // HTTPS only
            });

            // 3. Configure HttpClient for API calls
            var apiBaseUrl = builder.Configuration["ApiBaseUrl"];
            if (string.IsNullOrEmpty(apiBaseUrl))
            {
                throw new InvalidOperationException("ApiBaseUrl is not configured in appsettings.json");
            }

            builder.Services.AddHttpClient("AuthClient", client =>
            {
                client.BaseAddress = new Uri(apiBaseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
            })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                AutomaticDecompression = System.Net.DecompressionMethods.GZip |
                                        System.Net.DecompressionMethods.Deflate,
                UseProxy = false,
                MaxConnectionsPerServer = 10
            })
            .SetHandlerLifetime(TimeSpan.FromMinutes(5));

            // 4. Register Services
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<CommonAPI_Client>();
            builder.Services.AddScoped<AuthCookieService>();

            // 5. Configure Cookie Authentication
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.LoginPath = "/Home/Login";
                options.LogoutPath = "/Home/Logout";
                options.AccessDeniedPath = "/Home/AccessDenied";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                options.SlidingExpiration = true;
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // HTTPS only
                options.Cookie.SameSite = SameSiteMode.Lax;
            });

            // 6. Configure Authorization Policies
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy =>
                    policy.RequireClaim(ApplicationClaims.RoleName, "Admin"));
            });

            // 7. Configure Localization (optional - uncomment if needed)
            ConfigureLocalization(builder);

            return builder.Build();
        }

        public static WebApplication ConfigurePipeline(this WebApplication app)
        {
            // 1. Error Handling
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            // 2. HTTPS Redirection
            app.UseHttpsRedirection();

            // 3. Static Files with Caching
            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    // Cache static files for 1 year
                    if (ctx.Context.Request.Path.StartsWithSegments("/css") ||
                        ctx.Context.Request.Path.StartsWithSegments("/js") ||
                        ctx.Context.Request.Path.StartsWithSegments("/images"))
                    {
                        ctx.Context.Response.Headers.Append(
                            "Cache-Control", "public,max-age=31536000,immutable");
                    }
                    else
                    {
                        ctx.Context.Response.Headers.Append(
                            "Cache-Control", "public,max-age=3600");
                    }
                }
            });

            // 4. Localization (if configured)
            var localizationOptions = app.Services.GetService<IOptions<RequestLocalizationOptions>>();
            if (localizationOptions?.Value != null)
            {
                app.UseRequestLocalization(localizationOptions.Value);
            }

            // 5. Session
            app.UseSession();

            // 6. Routing
            app.UseRouting();

            // 7. Authentication & Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            // 8. Map Routes
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Login}/{id?}");

            return app;
        }

        /// <summary>
        /// Configures localization support for multiple languages
        /// </summary>
        private static void ConfigureLocalization(WebApplicationBuilder builder)
        {
            // Uncomment and configure if you need multi-language support

            // builder.Services.AddSingleton<LanguageService>();
            // builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

            // builder.Services.AddControllersWithViews()
            //     .AddViewLocalization(Microsoft.AspNetCore.Mvc.Razor.LanguageViewLocationExpanderFormat.Suffix)
            //     .AddDataAnnotationsLocalization(options =>
            //     {
            //         options.DataAnnotationLocalizerProvider = (type, factory) =>
            //         {
            //             var assemblyName = new System.Reflection.AssemblyName(
            //                 typeof(SharedResource).GetTypeInfo().Assembly.FullName);
            //             return factory.Create("SharedResource", assemblyName.Name);
            //         };
            //     });

            // var supportedCultures = new List<CultureInfo>
            // {
            //     new CultureInfo("en-US"),
            //     new CultureInfo("or-IN")
            // };

            // builder.Services.Configure<RequestLocalizationOptions>(options =>
            // {
            //     var defaultCulture = builder.Configuration["DefaultCulture"] ?? "en-US";
            //     options.DefaultRequestCulture = new RequestCulture(
            //         culture: defaultCulture,
            //         uiCulture: defaultCulture);
            //     options.SupportedCultures = supportedCultures;
            //     options.SupportedUICultures = supportedCultures;
            //     options.RequestCultureProviders.Insert(0, new QueryStringRequestCultureProvider());
            // });
        }
    }
}