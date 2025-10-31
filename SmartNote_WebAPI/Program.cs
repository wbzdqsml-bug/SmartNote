// SmartNote_WebAPI/Program.cs

using Microsoft.EntityFrameworkCore;
using SmartNote_DAL;
using SmartNote_BLL;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models; // VVVVVV 1. (新增) 引入 OpenAPI Models VVVVVV

var builder = WebApplication.CreateBuilder(args);

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

// --- 添加服务到容器 (DI Container) ---

builder.Services.AddControllers();

// 注册 ApplicationDbContext (已存在)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString, sqlServerOptions =>
    {
        sqlServerOptions.MigrationsAssembly("SmartNote_DAL");
    });
});

// 注册 BLL 服务 (已存在)
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<INoteService, NoteService>(); // (确保 INoteService 也已注册)

// (Swagger/OpenAPI 注册)
builder.Services.AddEndpointsApiExplorer();

// VVVVVV 2. (修改) 配置 SwaggerGen 以支持 JWT VVVVVV
builder.Services.AddSwaggerGen(options =>
{
    // 2.1 添加安全定义 (Security Definition)
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization", // 请求头中的 Key
        Type = SecuritySchemeType.Http, // 类型为 Http
        Scheme = "Bearer", // 方案名 (小写)
        BearerFormat = "JWT",
        In = ParameterLocation.Header, // 位置在 Header
        Description = "请输入 'Bearer' [空格] 然后输入你的 Token。\r\n\r\n例如: \"Bearer eyJhbGciOiJIUzI1Ni...\""
    });

    // 2.2 添加安全需求 (Security Requirement)
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer" // 必须与 AddSecurityDefinition 中的 Id ("Bearer") 一致
                }
            },
            new string[] {} // 作用域，对于 JWT 通常为空
        }
    });
});
// ^^^^^^ Swagger JWT 配置完毕 ^^^^^^


// 配置 JWT 认证服务 (已存在)
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// 添加 CORS 服务 (已存在)
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:5173")
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});


var app = builder.Build();

// --- 配置 HTTP 请求管道 (Middleware) ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(MyAllowSpecificOrigins); // 启用 CORS

app.UseAuthentication(); // 启用认证
app.UseAuthorization();  // 启用授权

app.MapControllers();

app.Run();