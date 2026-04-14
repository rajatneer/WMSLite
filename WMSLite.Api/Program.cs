using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using WMSLite.Api.Middleware;
using WMSLite.Api.Models;
using WMSLite.Api.Repositories;
using WMSLite.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<ITenantContext, TenantContext>();

builder.Services.AddScoped<IJsonRepository<Tenant>, JsonRepository<Tenant>>();
builder.Services.AddScoped<IJsonRepository<User>, JsonRepository<User>>();
builder.Services.AddScoped<IJsonRepository<Item>, JsonRepository<Item>>();
builder.Services.AddScoped<IJsonRepository<Location>, JsonRepository<Location>>();
builder.Services.AddScoped<IJsonRepository<InventoryRecord>, JsonRepository<InventoryRecord>>();
builder.Services.AddScoped<IJsonRepository<Order>, JsonRepository<Order>>();
builder.Services.AddScoped<IJsonRepository<Subscription>, JsonRepository<Subscription>>();

builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IBillingService, BillingService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ITenantService, TenantService>();

var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key not configured");
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = key
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseMiddleware<TenantResolverMiddleware>();
app.UseMiddleware<SubscriptionValidationMiddleware>();
app.UseAuthorization();

app.MapGet("/", () => Results.Content("<h1>WMSLite API is running</h1><p>Use /swagger in development.</p>", "text/html"));
app.MapControllers();

app.Run();
