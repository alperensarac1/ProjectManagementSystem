using System.Globalization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using ProjectManagement.Api.Configuration;
using ProjectManagement.Api.HealthChecks;
using ProjectManagement.Api.Middleware;
using ProjectManagement.Api.Services;
using ProjectManagement.Application.DependencyInjection;
using ProjectManagement.Application.Interfaces.Authentication;
using ProjectManagement.Infrastructure.Authentication;
using ProjectManagement.Infrastructure.DependencyInjection;
using ProjectManagement.Infrastructure.Initialization;

var builder = WebApplication.CreateBuilder(args);

// =========================================================
// CONTROLLERS VE JSON AYARLARI
// =========================================================

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        /*
         * Enum değerlerinin JSON içinde sayı yerine metin olarak
         * gönderilip alınmasını sağlar.
         *
         * Örnek:
         * "status": "Active"
         */
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });

// =========================================================
// HTTP CONTEXT VE CURRENT USER
// =========================================================

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<
    ICurrentUserService,
    CurrentUserService>();

// =========================================================
// APPLICATION VE INFRASTRUCTURE KATMANLARI
// =========================================================

builder.Services.AddApplication();

builder.Services.AddInfrastructure(
    builder.Configuration);

/*
 *
 *MAILBOX
 *
 */
/*
 * Mailbox dosya yüklemeleri için multipart/form-data
 * request sınırını 210 MB yapıyoruz.
 *
 * Dosyaların kendi toplam sınırı Application validator
 * tarafından 200 MB olarak uygulanır.
 */
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit =
        210L * 1024L * 1024L;
});
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize =
        210L * 1024L * 1024L;
});
// =========================================================
// CORS
// =========================================================

var corsSettings =
    builder.Configuration
        .GetSection(CorsSettings.SectionName)
        .Get<CorsSettings>()
    ?? new CorsSettings();

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "ClientApplications",
        policy =>
        {
            /*
             * appsettings.json içerisinde en az bir origin
             * tanımlanmışsa yalnızca bu adreslere izin verilir.
             */
            if (corsSettings.AllowedOrigins.Length > 0)
            {
                policy
                    .WithOrigins(corsSettings.AllowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            }
        });
});

// =========================================================
// RATE LIMITING
// =========================================================

var generalPermitLimit =
    builder.Environment.IsEnvironment("Testing")
        ? 1000
        : 100;

var authenticationPermitLimit =
    builder.Environment.IsEnvironment("Testing")
        ? 1000
        : 10;

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;

    options.AddPolicy(
        "general",
        httpContext =>
        {
            var partitionKey =
                RateLimitPartitionHelper.GetPartitionKey(
                    httpContext);

            return RateLimitPartition
                .GetFixedWindowLimiter(
                    partitionKey,
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit =
                            generalPermitLimit,

                        Window =
                            TimeSpan.FromMinutes(1),

                        QueueLimit = 10,

                        QueueProcessingOrder =
                            QueueProcessingOrder.OldestFirst,

                        AutoReplenishment = true
                    });
        });

    options.AddPolicy(
        "authentication",
        httpContext =>
        {
            var remoteIp =
                httpContext.Connection
                    .RemoteIpAddress?
                    .ToString()
                ?? "unknown-ip";

            return RateLimitPartition
                .GetFixedWindowLimiter(
                    remoteIp,
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit =
                            authenticationPermitLimit,

                        Window =
                            TimeSpan.FromMinutes(1),

                        QueueLimit = 0,

                        QueueProcessingOrder =
                            QueueProcessingOrder.OldestFirst,

                        AutoReplenishment = true
                    });
        });

    options.OnRejected =
        async (context, cancellationToken) =>
        {
            context.HttpContext.Response.ContentType =
                "application/json; charset=utf-8";

            if (context.Lease.TryGetMetadata(
                    MetadataName.RetryAfter,
                    out var retryAfter))
            {
                context.HttpContext.Response.Headers.RetryAfter =
                    Math.Ceiling(
                            retryAfter.TotalSeconds)
                        .ToString(
                            CultureInfo.InvariantCulture);
            }

            var response = new
            {
                success = false,
                message =
                    "Çok fazla istek gönderildi. " +
                    "Lütfen kısa bir süre sonra tekrar deneyiniz.",
                data = (object?)null,
                errors = (object?)null
            };

            await context.HttpContext.Response
                .WriteAsJsonAsync(
                    response,
                    cancellationToken);
        };
});
// =========================================================
// HEALTH CHECKS
// =========================================================

builder.Services
    .AddHealthChecks()

    /*
     * Uygulama prosesinin çalıştığını gösteren basit kontrol.
     */
    .AddCheck(
        "application",
        () => HealthCheckResult.Healthy(
            "API uygulaması çalışıyor."),
        tags: ["live"])

    /*
     * SQLite bağlantısının çalışıp çalışmadığını kontrol eder.
     */
    .AddCheck<DatabaseHealthCheck>(
        "database",
        tags: ["ready"]);

// =========================================================
// JWT AYARLARI
// =========================================================

var jwtSettings =
    builder.Configuration
        .GetSection(JwtSettings.SectionName)
        .Get<JwtSettings>()
    ?? throw new InvalidOperationException(
        "JWT ayarları okunamadı.");

if (string.IsNullOrWhiteSpace(jwtSettings.Issuer))
{
    throw new InvalidOperationException(
        "Jwt:Issuer ayarı bulunamadı.");
}

if (string.IsNullOrWhiteSpace(jwtSettings.Audience))
{
    throw new InvalidOperationException(
        "Jwt:Audience ayarı bulunamadı.");
}

if (string.IsNullOrWhiteSpace(jwtSettings.SecretKey) ||
    jwtSettings.SecretKey.Length < 32)
{
    throw new InvalidOperationException(
        "Jwt:SecretKey en az 32 karakter olmalıdır.");
}

// =========================================================
// JWT AUTHENTICATION
// =========================================================

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        /*
         * Development ortamında HTTP üzerinden test yapabilmek
         * için false bırakıldı.
         *
         * Production ortamında HTTPS kullanılmalıdır.
         */
        options.RequireHttpsMetadata =
            !builder.Environment.IsDevelopment() &&
            !builder.Environment.IsEnvironment("Testing");

        options.SaveToken = true;

        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,

                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,

                ValidateIssuerSigningKey = true,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            jwtSettings.SecretKey)),

                ValidateLifetime = true,

                /*
                 * Token süresi dolduğunda fazladan tolerans süresi
                 * verilmemesini sağlar.
                 */
                ClockSkew = TimeSpan.Zero
            };
    });

builder.Services.AddAuthorization();

// =========================================================
// SWAGGER / OPENAPI
// =========================================================

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "Project Management API",
            Version = "v1",
            Description =
                "JWT tabanlı proje ve görev yönetim sistemi API'si."
        });

    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",

            Description =
                "JWT access token değerini giriniz. " +
                "Başına 'Bearer' yazmanıza gerek yoktur."
        });

    /*
     * Swashbuckle 10 ve Microsoft.OpenApi 2.x yapısı.
     *
     * Swagger üzerinde tanımlanan Bearer güvenlik şemasını
     * endpointlere uygular.
     */
    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(
                "Bearer",
                document)] = []
        });
});

// =========================================================
// APPLICATION BUILD
// =========================================================

var app = builder.Build();

// =========================================================
// DATABASE INITIALIZATION
// =========================================================

/*
 * Uygulama başlatıldığında:
 *
 * - Bekleyen migration işlemleri uygulanır.
 * - Yapılandırılmışsa ilk Admin kullanıcısı oluşturulur.
 */
await using (var scope = app.Services.CreateAsyncScope())
{
    var databaseInitializer =
        scope.ServiceProvider
            .GetRequiredService<IDatabaseInitializer>();

    await databaseInitializer.InitializeAsync();
}

// =========================================================
// GLOBAL EXCEPTION HANDLING
// =========================================================

/*
 * Pipeline'ın başında bulunmalıdır.
 *
 * Kendisinden sonra çalışan middleware ve controller
 * exceptionlarını yakalar.
 */
app.UseGlobalExceptionHandling();

// =========================================================
// SWAGGER
// =========================================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/swagger/v1/swagger.json",
            "Project Management API v1");
    });
}

// =========================================================
// CORS
// =========================================================

/*
 * Web istemcilerinden gelen cross-origin istekleri
 * yapılandırılmış politika üzerinden kontrol eder.
 */
app.UseCors("ClientApplications");

// =========================================================
// RATE LIMITING
// =========================================================

/*
 * Controller ve endpointlerde tanımlanan rate-limit
 * politikalarını etkinleştirir.
 */
app.UseRateLimiter();

// =========================================================
// AUTHENTICATION
// =========================================================

/*
 * JWT token doğrulanır ve HttpContext.User oluşturulur.
 */
app.UseAuthentication();

// =========================================================
// ACTIVE USER VALIDATION
// =========================================================

/*
 * Kullanıcının veritabanındaki güncel:
 *
 * - Aktiflik
 * - Silinme
 * - Rol
 *
 * durumunu kontrol eder.
 */
app.UseActiveUserValidation();

// =========================================================
// AUTHORIZATION
// =========================================================

/*
 * [Authorize], rol ve policy kontrolleri uygulanır.
 */
app.UseAuthorization();

// =========================================================
// CONTROLLER ENDPOINTLERİ
// =========================================================

app.MapControllers();

// =========================================================
// HEALTH CHECK ENDPOINTLERİ
// =========================================================

/*
 * Liveness kontrolü:
 *
 * Uygulama prosesinin çalışıp çalışmadığını kontrol eder.
 * Veritabanı bağlantısını kontrol etmez.
 */
app.MapHealthChecks(
        "/health/live",
        new HealthCheckOptions
        {
            Predicate = registration =>
                registration.Tags.Contains("live"),

            ResponseWriter =
                HealthCheckResponseWriter.WriteResponseAsync
        })
    .AllowAnonymous()
    .DisableRateLimiting();

/*
 * Readiness kontrolü:
 *
 * API'nin veritabanı dahil olmak üzere istek
 * kabul etmeye hazır olup olmadığını kontrol eder.
 */
app.MapHealthChecks(
        "/health/ready",
        new HealthCheckOptions
        {
            Predicate = registration =>
                registration.Tags.Contains("ready"),

            ResponseWriter =
                HealthCheckResponseWriter.WriteResponseAsync
        })
    .AllowAnonymous()
    .DisableRateLimiting();

/*
 * Bütün health check kontrollerini birlikte çalıştırır.
 */
app.MapHealthChecks(
        "/health",
        new HealthCheckOptions
        {
            Predicate = _ => true,

            ResponseWriter =
                HealthCheckResponseWriter.WriteResponseAsync
        })
    .AllowAnonymous()
    .DisableRateLimiting();

// =========================================================
// API DURUM ENDPOINTİ
// =========================================================

app.MapGet(
        "/",
        () => Results.Ok(new
        {
            message =
                "Project Management API çalışıyor.",

            database = "SQLite",

            authentication = "JWT Bearer",

            activeUserValidation = true,

            cors = true,

            rateLimiting = true,

            healthChecks = true,

            environment =
                app.Environment.EnvironmentName,

            utcTime =
                DateTime.UtcNow
        }))
    .AllowAnonymous()
    .DisableRateLimiting()
    .WithName("ApiStatus")
    .WithTags("System");

// =========================================================
// APPLICATION RUN
// =========================================================

app.Run();

/*
 * WebApplicationFactory<Program> entegrasyon testlerinin
 * uygulamanın giriş noktasına erişebilmesi için Program sınıfını
 * partial olarak dışarı açıyoruz.
 */
public partial class Program
{
    
}