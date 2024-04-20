using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using BLL;
using BLL.RepositoryInterfaces;
using DAL.DbContexts;
using DAL.Repositories;
using Microsoft.AspNetCore.Authorization;
using Pazar;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (connectionString == null)
{
    throw new InvalidOperationException("DefaultConnection is not set in the configuration.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySQL(builder.Configuration.GetConnectionString("DefaultConnection")));



builder.Services.AddScoped<IUserDAO, UserDAO>();
builder.Services.AddScoped<UserManager>();

var domain = $"https://{builder.Configuration["Auth0:Domain"]}/";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = domain;
        options.Audience = builder.Configuration["Auth0:Audience"];
        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = ClaimTypes.NameIdentifier
        };
    });


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Description = "Standard Authorization header using the Bearer scheme (\"bearer {token}\")",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    c.OperationFilter<SecurityRequirementsOperationFilter>();
});

var token = builder.Configuration["AppSettings:Token"];
if (token == null)
{
    throw new InvalidOperationException("AppSettings:Token is not set in the configuration.");
}

// builder.Services.AddCors(options =>
// {
//     options.AddPolicy("AllowSwaggerUI",
//         builder =>
//         {
//             builder.WithOrigins("http://localhost:7176") // Replace with the actual domain of your Swagger UI
//                 .AllowAnyHeader()
//                 .AllowAnyMethod();
//         });
// });


//not needed
// builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//     .AddJwtBearer(options =>
//     {
//         options.TokenValidationParameters = new TokenValidationParameters
//         {
//             ValidateIssuerSigningKey = true,
//             IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(token)),
//             ValidateIssuer = false,
//             ValidateAudience = false
//         };
//     });



builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("read:messages", policy => policy.Requirements.Add(new 
        HasScopeRequirement("read:messages", domain)));
});
builder.Services.AddSingleton<IAuthorizationHandler, HasScopeHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API v1");
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
// app.UseCors("AllowSwaggerUI");

//not needed
// app.UseEndpoints(endpoints =>
// {
//     endpoints.MapControllers();
// });

app.MapControllers();

app.Run();