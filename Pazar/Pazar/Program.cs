using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using BLL;
using BLL.ManagerInterfaces;
using BLL.RepositoryInterfaces;
using BLL.Services;
using DAL.DbContexts;
using DAL.Repositories;
using MySqlConnector;
using Pazar;

var builder = WebApplication.CreateBuilder(args);

// Add logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

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
builder.Services.AddScoped<IUserDAO, UserDAO>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IUserManager, UserManager>();

// Add CORS policy for React application
builder.Services.AddCors(options =>
{
    options.AddPolicy("NgOrigins",
        policyBuilder => policyBuilder.WithOrigins("http://localhost:3000", "http://fe:3000", "https://localhost:3000", "https://fe:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

// Add authentication
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

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["AppSettings:Token"])),
            ValidateIssuer = false,
            ValidateAudience = false
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var authorizationHeader = context.Request.Headers["Authorization"].ToString();
                if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
                {
                    context.Token = context.Request.Cookies["jwt"];
                }
                else
                {
                    context.Token = authorizationHeader.Substring("Bearer ".Length).Trim();
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddSingleton<IConfiguration>(configuration);


var app = builder.Build();

// Ensure database migrations are applied on startup
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
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
