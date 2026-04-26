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

// COMPANY
using Frock_backend.transport_Company.Application.Internal.CommandServices;
using Frock_backend.transport_Company.Application.Internal.QueryServices;
using Frock_backend.transport_Company.Domain.Repositories;
using Frock_backend.transport_Company.Domain.Services;
using Frock_backend.transport_Company.Infrastructure.Repositories;

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
builder.Services.AddControllers(options => options.Conventions.Add(new KebabCaseRouteNamingConvention()));
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
builder.Services.AddScoped<IDriverProfileRepository, DriverProfileRepository>();
builder.Services.AddScoped<IUserCommandService, UserCommandService>();
builder.Services.AddScoped<IUserQueryService, UserQueryService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IHashingService, HashingService>();
builder.Services.AddScoped<IIamContextFacade, IamContextFacade>();

// ============================================================
// Dependency Injection — Company
// ============================================================
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<ICompanyCommandService, CompanyCommandService>();
builder.Services.AddScoped<ICompanyQueryService, CompanyQueryService>();

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

// ============================================================
// OSRM Routing Service
// ============================================================
builder.Services.AddHttpClient("osrm", client =>
{
    var baseUrl = builder.Configuration["Osrm:BaseUrl"] ?? "http://localhost:5001";
    var timeout = int.TryParse(builder.Configuration["Osrm:TimeoutSeconds"], out var t) ? t : 10;
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(timeout);
});
builder.Services.AddScoped<IOsrmRoutingService, OsrmRoutingService>();

// ============================================================
// External Services
// ============================================================
builder.Services.AddHttpClient<IGeoImportService, GeoImportService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["GeoApi:BaseUrl"]!);
});
builder.Services.AddScoped<GeographicDataSeeder>();

// Cloudinary
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("Cloudinary"));
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();

// ============================================================
// CORS
// ============================================================
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
                "https://frock-frontend.vercel.app",
                "https://frock-frontend-git-main-yassers.vercel.app",
                "https://frock-backend-monolito.onrender.com"
            )
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
