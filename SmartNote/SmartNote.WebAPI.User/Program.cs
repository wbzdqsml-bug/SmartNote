using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SmartNote.BLL;
using SmartNote.Common.Configs;
using SmartNote.DAL;
using SmartNote.WebAPI.User.Config;
using SmartNote.WebAPI.User.Filters;
using SmartNote.WebAPI.User.Middlewares;
using SmartNote.WebAPI.User.Hubs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 强类型配置（对应 SmartNote.WebAPI.User/Config 下的类）
var jwtConfig = builder.Configuration.GetSection(Settings.JwtSection).Get<JwtConfig>() ?? new JwtConfig();
jwtConfig.Validate();

var redisConfig = builder.Configuration.GetSection(Settings.RedisSection).Get<RedisConfig>() ?? new RedisConfig();
if (!redisConfig.IsValid)
    throw new InvalidOperationException("Redis:Configuration 未配置。");

var corsConfig = builder.Configuration.GetSection(Settings.CorsSection).Get<CorsConfig>() ?? new CorsConfig();
if (corsConfig.Origins.Length == 0)
{
    corsConfig.Origins = new[] { "http://localhost:5173" };
}

var swaggerConfig = builder.Configuration.GetSection(Settings.SwaggerSection).Get<SwaggerConfig>() ?? new SwaggerConfig();

/* -----------------------------------------------
 * 正确的 401 JSON 输出（修复 HTTP/2 无 body 的 BUG）
 * ---------------------------------------------*/
//static async Task Write401(TokenValidatedContext context, string code, string message)
//{
//    context.Response.StatusCode = 401;
//    context.Response.ContentType = "application/json";

//    // 🔥 阻止 JWT 中间件覆盖我们写的 JSON
//    context.NoResult();

//    var json = $"{{\"code\":\"{code}\",\"message\":\"{message}\"}}";
//    await context.Response.WriteAsync(json);
//}

/* -----------------------------------------------
 * 1️⃣ 数据库配置
 * ---------------------------------------------*/
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

/* -----------------------------------------------
 * 2️⃣ Redis 缓存
 * ---------------------------------------------*/
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConfig.Configuration;
});

/* -----------------------------------------------
 * 3️⃣ JWT + Redis Token 校验
 * ---------------------------------------------*/
var jwtKey = jwtConfig.Key;
var jwtIssuer = jwtConfig.Issuer;
var jwtAudience = jwtConfig.Audience;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // --- JWT 基础验证 ---
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };

        // ⭐ 中间变量：用于传递 Redis 错误原因
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var cache = context.HttpContext.RequestServices.GetRequiredService<IDistributedCache>();

                var userId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    context.Fail("INVALID_TOKEN");
                    return;
                }

                var authHeader = context.Request.Headers["Authorization"].ToString();
                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                {
                    context.Fail("INVALID_HEADER");
                    return;
                }

                var incomingToken = authHeader.Substring("Bearer ".Length).Trim();
                var cachedToken = await cache.GetStringAsync($"token:{userId}");

                if (string.IsNullOrEmpty(cachedToken))
                {
                    context.Fail("TOKEN_EXPIRED");
                    return;
                }

                if (!string.Equals(cachedToken, incomingToken, StringComparison.Ordinal))
                {
                    context.Fail("TOKEN_CHANGED");
                    return;
                }

                // 通过
            },


            // ⭐ 这里才是输出 JSON 的地方（完全不影响 HTTP/2）
            OnChallenge = context =>
            {
                if (!string.IsNullOrEmpty(context.Error))
                {
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";

                    string msg = context.Error switch
                    {
                        "TOKEN_EXPIRED" => "登录状态已失效，请重新登录。",
                        "TOKEN_CHANGED" => "账号已在其他设备登录。",
                        "INVALID_HEADER" => "无效的 Authorization header",
                        "INVALID_TOKEN" => "无效的 Token",
                        _ => "认证失败"
                    };

                    var json = $"{{\"code\":\"{context.Error}\",\"message\":\"{msg}\"}}";

                    context.HandleResponse(); // ⭐ 阻止默认 401 覆盖我们的 JSON
                    return context.Response.WriteAsync(json);
                }

                return Task.CompletedTask;
            }
        };
    });


/* -----------------------------------------------
 * 4️⃣ 注入业务层
 * ---------------------------------------------*/
builder.Services.AddBusinessServices();

/* -----------------------------------------------
 * 5️⃣ CORS
 * ---------------------------------------------*/
builder.Services.AddCors(options =>
{
    options.AddPolicy("default", policy =>
    {
        policy.WithOrigins(corsConfig.Origins)
              .AllowAnyHeader()
              .AllowAnyMethod();

        if (corsConfig.AllowCredentials)
            policy.AllowCredentials();
    });
});

/* -----------------------------------------------
 * 6️⃣ Controller + Filters + Swagger
 * ---------------------------------------------*/
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

// SignalR（实时协作）
builder.Services.AddSignalR();

/* -----------------------------------------------
 * 7️⃣ App 中间件
 * ---------------------------------------------*/
var app = builder.Build();

if (swaggerConfig.Enabled)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("default");
app.UseHttpsRedirection();
app.UseRequestLogging();

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<NoteHub>("/hubs/note");
app.MapControllers();

app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    api = "SmartNote.UserAPI",
    time = DateTime.UtcNow
}));

app.Run();
