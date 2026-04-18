using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using WMSLite.Middleware;
using WMSLite.Models;
using WMSLite.Repositories;
using WMSLite.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IJsonRepository<Tenant>>(sp =>
    new JsonRepository<Tenant>(Path.Combine(sp.GetRequiredService<IConfiguration>["Storage:RootPath"] ?? "Data", "tenants.json")));
builder.Services.AddSingleton<IJsonRepository<AppUser>>(sp =>
    new JsonRepository<AppUser>(Path.Combine(sp.GetRequiredService<IConfiguration>["Storage:RootPath"] ?? "Data", "users.json")));
builder.Services.AddSingleton<IJsonRepository<Item>>(sp =>
    new JsonRepository<Item>(Path.Combine(sp.GetRequiredService<IConfiguration>["Storage:RootPath"] ?? "Data", "items.json")));
builder.Services.AddSingleton<IJsonRepository<InventoryRecord>>(sp =>
    new JsonRepository<InventoryRecord>(Path.Combine(sp.GetRequiredService<IConfiguration>["Storage:RootPath"] ?? "Data", "inventory.json")));
builder.Services.AddSingleton<IJsonRepository<Order>>(sp =>
    new JsonRepository<Order>(Path.Combine(sp.GetRequiredService<IConfiguration>["Storage:RootPath"] ?? "Data", "orders.json")));
builder.Services.AddSingleton<IJsonRepository<Subscription>>(sp =>
    new JsonRepository<Subscription>(Path.Combine(sp.GetRequiredService<IConfiguration>["Storage:RootPath"] ?? "Data", "subscriptions.json")));

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IBillingService, BillingService>();

var jwt = builder.Configuration.GetSection("Jwt");
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Secret"] ?? throw new InvalidOperationException("Missing JWT Secret")));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = key
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseMiddleware<TenantResolverMiddleware>();
app.UseMiddleware<SubscriptionValidationMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();
