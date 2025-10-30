// SmartNote.Api/Program.cs

// VVVVVV 1. 导入命名空间 VVVVVV
using Microsoft.EntityFrameworkCore;
using SmartNote_DAL;
using SmartNote_BLL; // 1.1 (新增) 引入 BLL 命名空间
using Microsoft.AspNetCore.Authentication.JwtBearer; // 1.2 (新增) 引入 JWT 认证
using Microsoft.IdentityModel.Tokens; // 1.3 (新增) 引入 JWT 认证
using System.Text; // 1.4 (新增) 引入 JWT 认证
// ^^^^^^^^^^^^^^^^^^^^^^^^^^^^

var builder = WebApplication.CreateBuilder(args);

// VVVVVV (新增) 定义 CORS 策略名称 VVVVVV
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
// ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

// --- 添加服务到容器 (DI Container) ---

builder.Services.AddControllers();

// VVVVVV 2. 注册 ApplicationDbContext VVVVVV
// ... (这部分代码来自你的文件，保持不变)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString, sqlServerOptions =>
    {
        sqlServerOptions.MigrationsAssembly("SmartNote_DAL");
    });
});
// ^^^^^^ DbContext 注册完毕 ^^^^^^


// VVVVVV 3. (新增) 注册 BLL 服务 VVVVVV
// ... (这部分代码来自你的文件，保持不变)
builder.Services.AddScoped<IAuthService, AuthService>();
// ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^


// (Swagger/OpenAPI 注册 - 已存在)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// VVVVVV 4. (新增) 配置 JWT 认证服务 VVVVVV
// ... (这部分代码来自你的文件，保持不变)
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

// (可选但推荐) 添加授权服务 (已存在)
builder.Services.AddAuthorization();
// ^^^^^^ JWT 认证服务添加完毕 ^^^^^^


// VVVVVV 5. (新增) 添加 CORS 服务 VVVVVV
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          // 允许来自你的 Vue 前端 (http://localhost:5173) 的请求
                          policy.WithOrigins("http://localhost:5173")
                                .AllowAnyHeader() // 允许所有请求头
                                .AllowAnyMethod(); // 允许所有 HTTP 方法
                      });
});
// ^^^^^^ CORS 服务添加完毕 ^^^^^^


var app = builder.Build();

// --- 配置 HTTP 请求管道 (Middleware) ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// VVVVVV 6. (新增) 启用 CORS 中间件 VVVVVV
// (必须在 UseRouting 之后[如果显式调用的话]，在 UseAuthentication/UseAuthorization 之前)
app.UseCors(MyAllowSpecificOrigins);
// ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

// VVVVVV 5. (修改) 添加认证和授权中间件 (顺序很重要！) VVVVVV
app.UseAuthentication(); // (来自你的文件) 启用认证中间件
app.UseAuthorization();  // (来自你的文件) 启用授权中间件
// ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

app.MapControllers();

app.Run();