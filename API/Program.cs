using Amazon;
using Amazon.S3;
using Application;
using Application.Auth.Services;
using Application.Behaviors;
using Application.User.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using Infrastructure.BackgroundServices;
using Infrastructure.Data;
using Infrastructure.Hubs;
using Infrastructure.Messaging.RabbitMQ;
using Infrastructure.Repositories;
using Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Services.AddControllers();

// Add Memory Cache for Rate Limiting
builder.Services.AddMemoryCache();

builder.Services.AddSignalR();

// Add CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// JWT Configuration from appsettings
var jwtSecret = builder.Configuration["JwtSettings:Secret"];
var jwtIssuer = builder.Configuration["JwtSettings:Issuer"];
var jwtAudience = builder.Configuration["JwtSettings:Audience"];
var key = Encoding.ASCII.GetBytes(jwtSecret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Add HttpContextAccessor for getting current user
builder.Services.AddHttpContextAccessor();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(ApplicationAssembly.Instance);
});
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(typeof(Application.Auth.Commands.RegisterUserCommand).Assembly);

// AWS S3 Configuration with credentials from appsettings
var awsRegion = builder.Configuration["AWS:S3:Region"];
var awsAccessKey = builder.Configuration["AWS:S3:AccessKey"];
var awsSecretKey = builder.Configuration["AWS:S3:SecretKey"];

builder.Services.AddSingleton<IAmazonS3>(provider =>
{
    var config = new AmazonS3Config
    {
        RegionEndpoint = RegionEndpoint.GetBySystemName(awsRegion)
    };
    
    // Use credentials from configuration if provided
    if (!string.IsNullOrEmpty(awsAccessKey) && !string.IsNullOrEmpty(awsSecretKey))
    {
        return new AmazonS3Client(awsAccessKey, awsSecretKey, config);
    }
    
    // Otherwise use default credential chain (environment variables, IAM roles, etc.)
    return new AmazonS3Client(config);
});

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IWorkspaceRepository, WorkspaceRepository>();
builder.Services.AddScoped<IWorkspaceInvitationRepository, WorkspaceInvitationRepository>();
builder.Services.AddScoped<IBoardRepository, BoardRepository>();
builder.Services.AddScoped<IListRepository, ListRepository>();
builder.Services.AddScoped<ICardRepository, CardRepository>();
builder.Services.AddScoped<ICardStatusRepository, CardStatusRepository>();
builder.Services.AddScoped<IActivityRepository, ActivityRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IRateLimitingService, RateLimitingService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IFileStorageService, S3FileStorageService>();

// RabbitMQ config
var rabbitConfig = builder.Configuration.GetSection("RabbitMQ");

builder.Services.AddSingleton<IRabbitMqPublisher>(sp =>
    new RabbitMqPublisher(
        rabbitConfig["Host"],
        int.Parse(rabbitConfig["Port"]),
        rabbitConfig["Username"],
        rabbitConfig["Password"],
        rabbitConfig["VirtualHost"],
        rabbitConfig["Exchange"]));

builder.Services.AddSingleton<IRabbitMqConsumer>(sp =>
    new RabbitMqConsumer(
        rabbitConfig["Host"],
        int.Parse(rabbitConfig["Port"]),
        rabbitConfig["Username"],
        rabbitConfig["Password"],
        rabbitConfig["VirtualHost"],
        rabbitConfig["Exchange"]));

// Background Services
builder.Services.AddHostedService<ActivityEventConsumerService>();
builder.Services.AddHostedService<NotificationEventConsumerService>();

builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Use CORS - must be called before UseAuthentication and UseAuthorization
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapHub<NotificationHub>("/hubs/notifications");

app.MapGet("/", () => "Hello World");

app.Run();