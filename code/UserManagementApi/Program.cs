// using UserManagementApi.Data.CompiledModels; // remove until you have a generated compiled model

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Debugging;
using System.IO;
using System.Text;
using UserManagementApi.Data;
using UserManagementApi.Services;

var builder = WebApplication.CreateSlimBuilder(args);

// Ensure the logs directory exists (absolute path) and enable Serilog internal self-log
var logsPath = @"C:\Git\OakTires\Logs";
Directory.CreateDirectory(logsPath);
SelfLog.Enable(msg => Console.Error.WriteLine(msg));

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File(Path.Combine(logsPath, "log-.txt"), rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// If you want an EF compiled model, generate it with:
// dotnet ef dbcontext optimize --output-dir Data/CompiledModels --namespace UserManagementApi.Data.CompiledModels
// then restore the using above and the .UseModel(...) call.

// Use normal runtime model when a compiled model is not present
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
    // .UseModel(compiledModel) // enable only after generating/importing compiled model
);

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UserService>();
// Register HttpClient for webhook calls
builder.Services.AddHttpClient();

// Register controllers so Swagger discovers your controller endpoints
builder.Services.AddControllers();

// Ensure Jwt:Key is present at startup (throws early if missing)
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Missing configuration: Jwt:Key");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Enable Swagger UI and middleware so controllers are exposed in swagger
app.UseSwagger();
app.UseSwaggerUI();

// Routing + Auth middleware required for controllers
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Map controller routes (this is the required call for [ApiController] controllers)
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    //db.Database.Migrate(); // Ensure DB is created
    DataSeeder.SeedUsers(db); // Seed data
}

app.Run();

