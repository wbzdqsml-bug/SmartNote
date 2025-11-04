using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SmartNote.BLL;
using SmartNote.DAL;
using SmartNote.WebAPI.User.Filters;
using SmartNote.WebAPI.User.Middlewares;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ======================================================
// 1️⃣ 数据库配置
// ======================================================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// ======================================================
// 2️⃣ Redis 缓存配置（IDistributedCache）
// ======================================================
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:Configuration"];
});

// ======================================================
// 3️⃣ JWT 认证配置
// ======================================================
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new Exception("Jwt:Key not found in configuration.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "SmartNote.UserAPI";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "SmartNoteClient";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
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
    });

// ======================================================
// 4️⃣ 业务逻辑层服务注册（IAuthService, INoteService等）
// ======================================================
builder.Services.AddBusinessServices();

// ======================================================
// 5️⃣ CORS 配置（允许前端访问）
// ======================================================
builder.Services.AddCors(options =>
{
    var allowedOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? new[] { "http://localhost:5173" };
    options.AddPolicy("default", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ======================================================
// 6️⃣ Swagger 配置（启用 JWT “Authorize” 按钮）
// ======================================================
builder.Services.AddControllers(options =>
{
    options.Filters.Add<GlobalExceptionFilter>(); // ✅ 注册全局异常过滤器
    options.Filters.Add<ValidationFilter>();      // ✅ 模型验证过滤器

});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    // 基本信息
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SmartNote 用户 API",
        Version = "v1",
        Description = "基于 .NET 8 的智能学习笔记系统接口文档"
    });

    // ✅ 添加 JWT 安全定义
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "在下方输入：Bearer {your JWT token}"
    });

    // ✅ 添加全局安全要求
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
            new string[] {}
        }
    });
});

// ======================================================
// 7️⃣ 构建 Web 应用
// ======================================================
var app = builder.Build();

// ======================================================
// 8️⃣ 中间件管道
// ======================================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.DocumentTitle = "SmartNote API 文档";
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartNote 用户端 v1");
    });
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("default");          // 启用跨域
app.UseHttpsRedirection();
app.UseRequestLogging(); // ✅ 请求日志中间件
app.UseAuthentication();         // 启用 JWT 验证
app.UseAuthorization();          // 启用授权

app.MapControllers();            // 映射控制器路由

// 健康检查接口
app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    api = "SmartNote.UserAPI",
    time = DateTime.UtcNow
}));

// ======================================================
// 9️⃣ 启动应用
// ======================================================
app.Run();
