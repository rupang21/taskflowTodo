using Microsoft.EntityFrameworkCore;
using ToDoApp.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// ───── Services ─────

// Database — SQLite via Entity Framework Core
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// CORS — Allow React dev server origins
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// Controllers + JSON serialization
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Serialize enums as strings in JSON responses for readability
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "ToDoApp API",
        Version = "v1",
        Description = "A RESTful API for managing to-do tasks. Built with .NET 9 and SQLite."
    });
});

var app = builder.Build();

// ───── Middleware Pipeline ─────

// Global exception handler — returns consistent JSON error responses
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        var errorResponse = new
        {
            status = 500,
            message = "An unexpected error occurred. Please try again later.",
            timestamp = DateTime.UtcNow
        };

        await context.Response.WriteAsJsonAsync(errorResponse);
    });
});

// Swagger (development only)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ToDoApp API v1");
    });
}

app.UseCors("AllowFrontend");

// Production: serve React SPA from wwwroot (built by Dockerfile)
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthorization();

// Request logging middleware
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("→ {Method} {Path}", context.Request.Method, context.Request.Path);

    await next();

    logger.LogInformation("← {Method} {Path} → {StatusCode}",
        context.Request.Method, context.Request.Path, context.Response.StatusCode);
});

// Auto-create database directory and schema if not exists
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Ensure the directory for the SQLite file exists (e.g., /data on Fly.io)
    var connStr = builder.Configuration.GetConnectionString("DefaultConnection") ?? "";
    var dbPath = connStr.Replace("Data Source=", "").Trim();
    if (!string.IsNullOrEmpty(dbPath))
    {
        var dbDir = Path.GetDirectoryName(Path.GetFullPath(dbPath));
        if (!string.IsNullOrEmpty(dbDir))
            Directory.CreateDirectory(dbDir);
    }

    dbContext.Database.EnsureCreated();
}

app.MapControllers();

// SPA fallback: serve index.html for non-API routes (React Router support)
app.MapFallbackToFile("index.html");

app.Run();
