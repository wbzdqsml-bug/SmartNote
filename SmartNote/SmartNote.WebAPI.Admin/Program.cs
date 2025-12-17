using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SmartNote.DAL;
using SmartNote.BLL;
using SmartNote.BLL.Ai;
using SmartNote.Common.Configs;
using SmartNote.WebAPI.Admin.Config;
using SmartNote.WebAPI.Admin.Filters;
using SmartNote.WebAPI.Admin.Middlewares;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 强类型配置（与 User API 保持一致，避免魔法字符串）
var jwtConfig = builder.Configuration.GetSection(Settings.JwtSection).Get<JwtConfig>() ?? new JwtConfig();
jwtConfig.Validate();

var redisConfig = builder.Configuration.GetSection(Settings.RedisSection).Get<RedisConfig>() ?? new RedisConfig();
if (!redisConfig.IsValid)
    throw new InvalidOperationException("Redis:Configuration 未配置。");

var corsConfig = builder.Configuration.GetSection(Settings.CorsSection).Get<CorsConfig>() ?? new CorsConfig();
if (corsConfig.Origins.Length == 0)
    corsConfig.Origins = new[] { "http://localhost:8080" };

var swaggerConfig = builder.Configuration.GetSection(Settings.SwaggerSection).Get<SwaggerConfig>() ?? new SwaggerConfig();

// DbContext
builder.Services.AddDbContext<ApplicationDbContext>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// CORS
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("default", policy =>
    {
        policy.WithOrigins(corsConfig.Origins)
              .AllowAnyHeader()
              .AllowAnyMethod();

        if (corsConfig.AllowCredentials)
            policy.AllowCredentials();
    });
});

// Auth - JWT（与 User 区分 Issuer/Audience）
var jwtKey = jwtConfig.Key;
var jwtIssuer = jwtConfig.Issuer;
var jwtAudience = jwtConfig.Audience;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

// BLL
builder.Services.AddBusinessServices();

// Redis
builder.Services.AddStackExchangeRedisCache(o =>
{
    o.Configuration = redisConfig.Configuration;
});

// AI（Admin 侧目前未使用，但注册依赖以避免后续注入时报错）
var aiOptions = builder.Configuration.GetSection(Settings.AiSection).Get<AiOptions>() ?? new AiOptions();
builder.Services.AddSingleton(aiOptions);
builder.Services.AddHttpClient<OpenAiClient>(client =>
{
    var baseUrl = (aiOptions.BaseUrl ?? "https://api.openai.com/v1").TrimEnd('/') + "/";
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(Math.Max(5, aiOptions.TimeoutSeconds));
});

// 基础
builder.Services.AddControllers(options =>
{
    options.Filters.Add<GlobalExceptionFilter>();
    options.Filters.Add<ValidationFilter>();
});

if (swaggerConfig.Enabled)
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc(swaggerConfig.Version, new OpenApiInfo
        {
            Title = swaggerConfig.Title,
            Version = swaggerConfig.Version,
            Description = swaggerConfig.Description
        });

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "输入：Bearer {token}"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[]{}
            }
        });
    });
}

var app = builder.Build();

if (swaggerConfig.Enabled)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRequestLogging();
app.UseCors("default");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { ok = true, api = "admin" }));

app.Run();
