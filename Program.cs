using epico_backend.Controllers.Interfaces;
using epico_backend.Controllers.Services;
using epico_backend.data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ===================== DB =====================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ===================== Controllers =====================
builder.Services.AddControllers();

// ===================== HTTP hosts =====================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy
                .WithOrigins("http://localhost:3000",
                "https://epico-eight.vercel.app/")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});
// ===================== Services =====================
builder.Services.AddScoped<IPlayerServices, PlayerService>();
builder.Services.AddScoped<IJwtServices, JwtServices>();
builder.Services.AddSingleton<RedisService>();

// ===================== Redis =====================
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var config = ConfigurationOptions.Parse(
        builder.Configuration["Redis:ConnectionString"]
    );

    config.Ssl = true;
    config.AbortOnConnectFail = false; // 👈 مهم جدًا

    return ConnectionMultiplexer.Connect(config);
});
// ===================== JWT AUTH =====================
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // 1. حاول من Authorization Header (Unity)
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                context.Token = authHeader.Substring("Bearer ".Length).Trim();
            }
            // 2. لو ما فيه، خذ من Cookie (Web)
            else
            {
                context.Token = context.Request.Cookies["token"];
            }

            return Task.CompletedTask;
        }
    };
});

// ===================== OpenAPI =====================
builder.Services.AddOpenApi();

var app = builder.Build();

// ===================== Pipeline =====================
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// 🔥 مهم جدًا ترتيبهم
app.UseAuthentication();
app.UseAuthorization();

app.UseCors("AllowFrontend");
app.MapControllers();

app.Run();