using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using Frock_backend.shared.Infrastructure.Swagger;

// SHARED
using Frock_backend.shared.Infrastructure.Persistences.EFC.Configuration;
using Frock_backend.shared.Infrastructure.Persistences.EFC.Repositories;
using Frock_backend.shared.Infrastructure.Interfaces.ASP.Configuration;
using Frock_backend.shared.Infrastructure.Interfaces.ASP;
using Frock_backend.shared.Domain.Repositories;
using Frock_backend.shared.Domain.Services;
using Frock_backend.shared.Infrastructure.Configuration;
using Frock_backend.shared.Infrastructure.Services;

// IAM
using Frock_backend.IAM.Application.Internal.CommandServices;
using Frock_backend.IAM.Application.Internal.OutboundServices;
using Frock_backend.IAM.Application.Internal.QueryServices;
using Frock_backend.IAM.Domain.Repositories;
using Frock_backend.IAM.Domain.Services;
using Frock_backend.IAM.Infrastructure.Persistence.EFC.Repositories;
using Frock_backend.IAM.Infrastructure.Hashing.BCrypt.Services;
using Frock_backend.IAM.Infrastructure.Pipeline.Middleware.Extensions;
using Frock_backend.IAM.Infrastructure.Tokens.JWT.Configuration;
using Frock_backend.IAM.Infrastructure.Tokens.JWT.Services;
using Frock_backend.IAM.Interfaces.ACL;
using Frock_backend.IAM.Interfaces.ACL.Services;

// DRIVER BC (wired by F4)
using Frock_backend.Driver.Domain.Repositories;
using Frock_backend.Driver.Domain.Services;
using Frock_backend.Driver.Application.Internal.CommandServices;
using Frock_backend.Driver.Application.Internal.QueryServices;
using Frock_backend.Driver.Infrastructure.Repositories;
using Frock_backend.Driver.Interfaces.ACL;
using Frock_backend.Driver.Interfaces.ACL.Services;

// PAYMENTS BC (wired by F4)
using Frock_backend.Payments.Domain.Repositories;
using Frock_backend.Payments.Domain.Services;
using Frock_backend.Payments.Domain.Services.Gateways;
using Frock_backend.Payments.Application.Internal.CommandServices;
using Frock_backend.Payments.Application.Internal.OutboundServices;
using Frock_backend.Payments.Application.Internal.QueryServices;
using Frock_backend.Payments.Infrastructure.Repositories;
using Frock_backend.Payments.Infrastructure.ExternalServices.Gateways;
using Frock_backend.Payments.Infrastructure.ExternalServices.Gateways.PayU;
using Frock_backend.Payments.Infrastructure.Factories;
using Frock_backend.Payments.Interfaces.ACL;
using Frock_backend.Payments.Interfaces.ACL.Services;

// SUBSCRIPTIONS BC (wired by F4)
using Frock_backend.Subscriptions.Domain.Repositories;
using Frock_backend.Subscriptions.Domain.Services;
using Frock_backend.Subscriptions.Application.Internal.CommandServices;
using Frock_backend.Subscriptions.Application.Internal.QueryServices;
using Frock_backend.Subscriptions.Infrastructure.Repositories;
using Frock_backend.Subscriptions.Infrastructure.Seeding;
using Frock_backend.Subscriptions.Interfaces.ACL;
using Frock_backend.Subscriptions.Interfaces.ACL.Services;

// STOPS
using Frock_backend.stops.Application.Internal.CommandServices;
using Frock_backend.stops.Application.Internal.QueryServices;
using Frock_backend.stops.Domain.Repositories;
using Frock_backend.stops.Domain.Services;
using Frock_backend.stops.Infrastructure.Repositories;

// GEOGRAPHIC
using Frock_backend.stops.Application.Internal.CommandServices.Geographic;
using Frock_backend.stops.Application.Internal.QueryServices.Geographic;
using Frock_backend.stops.Domain.Repositories.Geographic;
using Frock_backend.stops.Domain.Services.Geographic;
using Frock_backend.stops.Infrastructure.Repositories.Geographic;
using Frock_backend.stops.Infrastructure.Seeding;

// ROUTES
using Frock_backend.routes.Domain.Repository;
using Frock_backend.routes.Infrastructure.Repositories;
using Frock_backend.routes.Domain.Service;
using Frock_backend.routes.Application.Internal.CommandServices;
using Frock_backend.routes.Application.Internal.QueryServices;
using Frock_backend.routes.Infrastructure.ExternalServices;
using Frock_backend.stops.Application.External;

// RATINGS
using Frock_backend.Ratings.Domain.Repositories;
using Frock_backend.Ratings.Domain.Services;
using Frock_backend.Ratings.Infrastructure.Repositories;
using Frock_backend.Ratings.Application.Internal.CommandServices;
using Frock_backend.Ratings.Application.Internal.QueryServices;

// TRIPS
using Frock_backend.Trips.Domain.Repositories;
using Frock_backend.Trips.Domain.Services;
using Frock_backend.Trips.Infrastructure.Repositories;
using Frock_backend.Trips.Application.Internal.CommandServices;
using Frock_backend.Trips.Application.Internal.QueryServices;
using Frock_backend.Trips.Interfaces.ACL;
using Frock_backend.Trips.Interfaces.ACL.Services;

// COLLECTIONS
using Frock_backend.Collections.Domain.Repositories;
using Frock_backend.Collections.Domain.Services;
using Frock_backend.Collections.Infrastructure.Repositories;
using Frock_backend.Collections.Application.Internal.CommandServices;
using Frock_backend.Collections.Application.Internal.QueryServices;

// NOTIFICATIONS
using Frock_backend.Notifications.Domain.Repositories;
using Frock_backend.Notifications.Domain.Services;
using Frock_backend.Notifications.Infrastructure.Repositories;
using Frock_backend.Notifications.Application.Internal.CommandServices;
using Frock_backend.Notifications.Application.Internal.QueryServices;

// DISCOVERY
using Frock_backend.Discovery.Domain.Services;
using Frock_backend.Discovery.Application.Internal.QueryServices;


// ============================================================
// Serilog Configuration
// ============================================================
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .WriteTo.Console()
    .WriteTo.File("logs/frock-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

// ============================================================
// Configure Services
// ============================================================

builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddControllers(options => options.Conventions.Add(new KebabCaseRouteNamingConvention()))
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();

// Global Exception Handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.EnableAnnotations();
    options.OperationFilter<SwaggerExamplesFilter>();
    options.SwaggerDoc("v1",
        new OpenApiInfo
        {
            Title = "Chapaturuta Backend API",
            Version = "v1",
            Description = "Chapaturuta - Plataforma de transporte colectivo digital",
            Contact = new OpenApiContact
            {
                Name = "Frock Studios",
                Email = "frock@studios.com"
            }
        });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Ingresa el token JWT con el prefijo Bearer. Ejemplo: Bearer eyJhbGciOi...",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme
                }
            },
            Array.Empty<string>()
        }
    });
});

// ============================================================
// Database Context
// ============================================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (connectionString is null)
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

if (builder.Environment.IsDevelopment())
    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        options.UseMySQL(connectionString)
            .LogTo(Console.WriteLine, LogLevel.Warning)
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors();
    });
else if (builder.Environment.IsProduction())
    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        options.UseMySQL(connectionString)
            .LogTo(Console.WriteLine, LogLevel.Error)
            .EnableDetailedErrors();
    });

// ============================================================
// Health Checks
// ============================================================
builder.Services.AddHealthChecks()
    .AddMySql(connectionString, name: "mysql", tags: new[] { "ready" });

// ============================================================
// Dependency Injection — Shared
// ============================================================
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// ============================================================
// Dependency Injection — IAM
// ============================================================
builder.Services.Configure<TokenSettings>(builder.Configuration.GetSection("TokenSettings"));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserCommandService, UserCommandService>();
builder.Services.AddScoped<IUserQueryService, UserQueryService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IHashingService, HashingService>();
builder.Services.AddScoped<IIamContextFacade, IamContextFacade>();

// ============================================================
// Dependency Injection — Driver BC (wired by F4)
// ============================================================
builder.Services.AddScoped<IDriverRepository, DriverRepository>();
builder.Services.AddScoped<ITariffRepository, TariffRepository>();
builder.Services.AddScoped<IRouteDurationRepository, RouteDurationRepository>();
builder.Services.AddScoped<IDriverCommandService, DriverCommandService>();
builder.Services.AddScoped<ITariffCommandService, TariffCommandService>();
builder.Services.AddScoped<IDriverQueryService, DriverQueryService>();
builder.Services.AddScoped<ITariffQueryService, TariffQueryService>();
builder.Services.AddScoped<IDriverContextFacade, DriverContextFacade>();

// ============================================================
// Dependency Injection — Payments BC (wired by F4)
// ============================================================
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IRefundRepository, RefundRepository>();
builder.Services.AddScoped<IPaymentCommandService, PaymentCommandService>();
builder.Services.AddScoped<IPaymentQueryService, PaymentQueryService>();
builder.Services.AddScoped<IRefundCommandService, RefundCommandService>();
builder.Services.AddScoped<IRefundQueryService, RefundQueryService>();
builder.Services.AddScoped<IYapePaymentGateway, YapePaymentGateway>();
builder.Services.AddScoped<IPlinPaymentGateway, PlinPaymentGateway>();

// PayU backs the Card payment method
builder.Services.Configure<PayUSettings>(builder.Configuration.GetSection("PayU"));
// Cap the PayU round-trip so a slow/unresponsive sandbox can't hang the charge request near
// HttpClient's 100s default; the client (axios) gives up at 30s, so stay under that.
builder.Services.AddHttpClient<IPayUPaymentGateway, PayUPaymentGateway>(c => c.Timeout = TimeSpan.FromSeconds(20));
builder.Services.AddScoped<ICardPaymentGateway>(sp => sp.GetRequiredService<IPayUPaymentGateway>());

builder.Services.AddScoped<ICashPaymentHandler, CashPaymentHandler>();
builder.Services.AddScoped<PaymentGatewayFactory>();
builder.Services.AddScoped<IPaymentsContextFacade, PaymentsContextFacade>();
// Orchestrates payment confirmation -> reservation/subscription activation (no DI cycle).
builder.Services.AddScoped<PaymentConfirmationService>();
// Best-effort WhatsApp/N8N notification runs synchronously inside the confirm flow; keep its
// timeout short so a slow webhook never dominates the charge latency (failures are swallowed).
builder.Services.AddHttpClient<IReservationNotificationService, N8NReservationNotificationService>(c => c.Timeout = TimeSpan.FromSeconds(5));

// ============================================================
// Dependency Injection — Subscriptions BC (wired by F4)
// ============================================================
builder.Services.AddScoped<IPlanRepository, PlanRepository>();
builder.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
builder.Services.AddScoped<IPlanCommandService, PlanCommandService>();
builder.Services.AddScoped<IPlanQueryService, PlanQueryService>();
builder.Services.AddScoped<ISubscriptionCommandService, SubscriptionCommandService>();
builder.Services.AddScoped<ISubscriptionQueryService, SubscriptionQueryService>();
builder.Services.AddScoped<ISubscriptionsContextFacade, SubscriptionsContextFacade>();

// ============================================================
// Dependency Injection — Geographic
// ============================================================
builder.Services.AddScoped<IRegionRepository, RegionRepository>();
builder.Services.AddScoped<IRegionCommandService, RegionCommandService>();
builder.Services.AddScoped<IRegionQueryService, RegionQueryService>();
builder.Services.AddScoped<IProvinceRepository, ProvinceRepository>();
builder.Services.AddScoped<IProvinceCommandService, ProvinceCommandService>();
builder.Services.AddScoped<IProvinceQueryService, ProvinceQueryService>();
builder.Services.AddScoped<IDistrictRepository, DistrictRepository>();
builder.Services.AddScoped<IDistrictCommandService, DistrictCommandService>();
builder.Services.AddScoped<IDistrictQueryService, DistrictQueryService>();

// ============================================================
// Dependency Injection — Stops
// ============================================================
builder.Services.AddScoped<IStopRepository, StopRepository>();
builder.Services.AddScoped<IStopCommandService, StopCommandService>();
builder.Services.AddScoped<IStopQueryService, StopQueryService>();

// ============================================================
// Dependency Injection — Routes
// ============================================================
builder.Services.AddScoped<IRouteRepository, RouteRepository>();
builder.Services.AddScoped<IRouteCommandService, RouteCommandService>();
builder.Services.AddScoped<IRouteQueryService, RouteQueryService>();

// ============================================================
// Dependency Injection — Ratings
// ============================================================
builder.Services.AddScoped<IRatingRepository, RatingRepository>();
builder.Services.AddScoped<IRatingCommandService, RatingCommandService>();
builder.Services.AddScoped<IRatingQueryService, RatingQueryService>();

// ============================================================
// Dependency Injection — Trips
// ============================================================
builder.Services.AddScoped<ITripRepository, TripRepository>();
builder.Services.AddScoped<ITripCommandService, TripCommandService>();
builder.Services.AddScoped<ITripQueryService, TripQueryService>();
builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
builder.Services.AddScoped<IReservationCommandService, ReservationCommandService>();
builder.Services.Configure<ReservationHoldOptions>(builder.Configuration.GetSection(ReservationHoldOptions.SectionName));
builder.Services.AddScoped<IReservationQueryService, ReservationQueryService>();
builder.Services.AddScoped<ITripsContextFacade, TripsContextFacade>();

// ============================================================
// Dependency Injection — Collections
// ============================================================
builder.Services.AddScoped<ICollectionRepository, CollectionRepository>();
builder.Services.AddScoped<ICollectionItemRepository, CollectionItemRepository>();
builder.Services.AddScoped<ICollectionCommandService, CollectionCommandService>();
builder.Services.AddScoped<ICollectionQueryService, CollectionQueryService>();

// ============================================================
// Dependency Injection — Notifications
// ============================================================
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationCommandService, NotificationCommandService>();
builder.Services.AddScoped<INotificationQueryService, NotificationQueryService>();

// ============================================================
// Dependency Injection — Discovery
// ============================================================
builder.Services.AddScoped<IDiscoveryQueryService, DiscoveryQueryService>();

// Asistente IA de viajes multi-tramo (Pasajero Premium).
// Grafo = fuente de verdad; el LLM (Ollama local, swappable a Claude) solo narra.
builder.Services.AddScoped<Frock_backend.Discovery.Domain.Services.IJourneyPlanner,
    Frock_backend.Discovery.Application.Internal.Services.JourneyPlannerService>();
builder.Services.AddScoped<Frock_backend.Discovery.Domain.Services.IChatAssistant,
    Frock_backend.Discovery.Infrastructure.ExternalServices.OllamaChatAssistant>();
builder.Services.AddScoped<Frock_backend.Discovery.Domain.Services.IAssistantQueryService,
    Frock_backend.Discovery.Application.Internal.QueryServices.AssistantQueryService>();
builder.Services.AddHttpClient("ollama", client =>
{
    var baseUrl = builder.Configuration["Assistant:BaseUrl"] ?? "http://localhost:11434";
    var timeout = int.TryParse(builder.Configuration["Assistant:TimeoutSeconds"], out var t) ? t : 30;
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(timeout);
});

// ============================================================
// OSRM Routing Service
// ============================================================
builder.Services.AddHttpClient("osrm", client =>
{
    var baseUrl = builder.Configuration["Osrm:BaseUrl"] ?? "http://localhost:5001";
    var timeout = int.TryParse(builder.Configuration["Osrm:TimeoutSeconds"], out var t) ? t : 10;
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(timeout);
    // Public OSRM demo server (router.project-osrm.org) requires a valid User-Agent
    // per its usage policy; without one requests may be blocked (403). Harmless for local OSRM.
    client.DefaultRequestHeaders.UserAgent.ParseAdd("Chapaturuta-Frock/1.0 (+student-project)");
});
builder.Services.AddScoped<IOsrmRoutingService, OsrmRoutingService>();

// ============================================================
// External Services
// ============================================================
builder.Services.AddHttpClient<IGeoImportService, GeoImportService>(client =>
{
    // BaseUrl may be empty (then GeoImportService falls back to the bundled snapshot);
    // new Uri("") throws, so only set BaseAddress when a URL is configured.
    var geoApiUrl = builder.Configuration["GeoApi:BaseUrl"];
    if (!string.IsNullOrWhiteSpace(geoApiUrl))
        client.BaseAddress = new Uri(geoApiUrl);
});
builder.Services.AddScoped<GeographicDataSeeder>();

// Cloudinary
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("Cloudinary"));
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();

// ============================================================
// CORS
// ============================================================
var defaultOrigins = new[]
{
    "http://localhost:5173",
    "https://frock-frontend.vercel.app",
    "https://frock-frontend-git-main-yassers.vercel.app",
    "https://frock-backend-monolito.onrender.com"
};

// Origenes extra via config/env: Cors__AllowedOrigins__0=https://tu-app.pages.dev
var extraOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? Array.Empty<string>();

var allowedOrigins = defaultOrigins.Concat(extraOrigins).Distinct().ToArray();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// ============================================================
// Build App
// ============================================================
var app = builder.Build();

app.UseCors();
app.UseExceptionHandler();
app.UseSerilogRequestLogging();

// Verify Database Objects are created
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();

    try
    {
        var seeder = services.GetRequiredService<GeographicDataSeeder>();
        await seeder.SeedDataAsync();
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error during geographic data seeding");
    }

    try
    {
        await PlansSeeder.SeedAsync(context);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error during Plans seeding");
    }
}

// Swagger
app.UseSwagger(c =>
{
    c.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi2_0;
});
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Chapaturuta API V1");
    c.RoutePrefix = "swagger";
    c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
});

app.UseRouting();
app.UseRequestAuthorization();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
