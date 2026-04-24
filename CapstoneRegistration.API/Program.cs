using System.Reflection;
using CapstoneRegistration.API.Extensions;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddApplicationServices(builder.Configuration);

builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddCorsPolicy(builder.Configuration);

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "Capstone Project Registration Tool API",
        Version     = "v1",
        Description = "Backend API for managing capstone project registrations, " +
                      "supervisors, students, and semester cycles.",
        Contact = new OpenApiContact
        {
            Name  = "Capstone Dev Team",
            Email = "dev@capstone.local"
        }
    });

    var jwtScheme = new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        Scheme       = "bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "Enter your JWT token. Example: **eyJhbGci...**"
    };

    options.AddSecurityDefinition("Bearer", jwtScheme);

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var db = scope.ServiceProvider.GetRequiredService<CapstoneRegistration.API.Data.ApplicationDbContext>();
    try
    {
        logger.LogInformation("Connecting to database...");
        await db.Database.CanConnectAsync();
        logger.LogInformation("Database connection established successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to connect to the database.");
    }
}

app.UseExceptionHandling();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Capstone Registration API v1");
    options.RoutePrefix = "swagger";
    options.DocumentTitle = "Capstone Project Registration Tool";
});

// HTTPS redirection is handled by Railway's edge proxy; skip it in container
// app.UseHttpsRedirection();
app.UseCors("DefaultCors");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
