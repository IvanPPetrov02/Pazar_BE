using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using BLL;
using BLL.CategoryRelated;
using BLL.ItemRelated;
using BLL.ManagerInterfaces;
using BLL.Managers;
using BLL.RepositoryInterfaces;
using BLL.Services;
using DAL.CategoryRelated;
using DAL.DbContexts;
using DAL.Repositories;
using MySqlConnector;
using Pazar;
using CustomAuthorization;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

var configuration = builder.Configuration;
var environment = builder.Environment;

string connectionString;

if (environment.IsDevelopment())
{
    connectionString = configuration.GetConnectionString("DefaultConnection");
}
else
{
    connectionString = configuration.GetConnectionString("DockerConnection");
}

try
{
    Console.WriteLine($"Attempting to connect to the database using connection string: {connectionString}");
    using (var connection = new MySqlConnection(connectionString))
    {
        connection.Open();
        Console.WriteLine("Successfully connected to the database.");
    }
}
catch (Exception e)
{
    Console.WriteLine($"Failed to connect to the database: {e.Message}");
    throw;
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Add repositories and managers
// Auth
builder.Services.AddScoped<IJwtService, JwtService>();
// User
builder.Services.AddScoped<IUserDAO, UserDAO>();
builder.Services.AddScoped<IAddressDAO, AddressDAO>();
builder.Services.AddScoped<IUserManager, UserManager>();
// Item
builder.Services.AddScoped<IItemDAO, ItemDAO>();
builder.Services.AddScoped<IItemManager, ItemManager>();
// Category
builder.Services.AddScoped<ICategoryDAO, CategoryDAO>();
builder.Services.AddScoped<ICategoryManager, CategoryManager>();
// Chat
builder.Services.AddScoped<IChatDAO, ChatDAO>();
builder.Services.AddScoped<IMessageDAO, MessageDAO>();
builder.Services.AddScoped<IChatManager, ChatManager>();

// Add CORS policy for React application
builder.Services.AddCors(options =>
{
    options.AddPolicy("NgOrigins",
        policyBuilder => policyBuilder.WithOrigins("http://localhost:5173", "http://localhost:3000", "http://fe:3000", "https://localhost:3000", "https://fe:3000", "https://localhost:7176")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

// Add controllers and Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "PazarAPI", Version = "v1" });
    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Description = "Standard Authorization header using the Bearer scheme (\"bearer {token}\")",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    c.OperationFilter<SecurityRequirementsOperationFilter>();
});

// Add authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Validation parameters
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["AppSettings:Token"])),
            ValidateIssuer = false,
            ValidateAudience = false
        };

        // Customize the token retrieval
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var authorizationHeader = context.Request.Headers["Authorization"].ToString();
                if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    context.Token = authorizationHeader.Substring("Bearer ".Length).Trim();
                }

                return Task.CompletedTask;
            }
        };
    });

// Add custom authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOrOwnerPolicy", policy =>
        policy.Requirements.Add(new AdminOrOwnerRequirement()));
    options.AddPolicy("IsTheUserOrAdminPolicy", policy =>
        policy.Requirements.Add(new IsTheUserOrAdminRequirement()));
    options.AddPolicy("IsSellerOrBuyerPolicy", policy =>
        policy.Requirements.Add(new IsSellerOrBuyerRequirement()));
    options.AddPolicy("IsNotSellerPolicy", policy =>
        policy.Requirements.Add(new IsNotSellerRequirement()));
});

// Register the authorization handlers as scoped
builder.Services.AddScoped<IAuthorizationHandler, AdminOrOwnerHandler>();
builder.Services.AddScoped<IAuthorizationHandler, IsTheUserOrAdminHandler>();
builder.Services.AddScoped<IAuthorizationHandler, IsSellerOrBuyerHandler>();
builder.Services.AddScoped<IAuthorizationHandler, IsNotSellerHandler>();

// Register the attributes in DI container as scoped
builder.Services.AddScoped<AdminOrOwnerAttribute>();
builder.Services.AddScoped<IsTheUserOrAdminAttribute>();
builder.Services.AddScoped<IsSellerOrBuyerAttribute>();
builder.Services.AddScoped<IsNotSellerAttribute>();


// Register IConfiguration
builder.Services.AddSingleton<IConfiguration>(configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PazarAPI V1");
    });
}

app.UseCors("NgOrigins");

app.UseAuthentication();
app.UseAuthorization();

// Add WebSocket support
var webSocketOptions = new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromMinutes(2)
};

webSocketOptions.AllowedOrigins.Add("http://localhost:5173");
webSocketOptions.AllowedOrigins.Add("http://localhost:3000");

app.UseWebSockets(webSocketOptions);

app.MapControllers();

app.Run();
