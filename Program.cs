using SqlInjector;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SqlInjector.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<SqlInjectorDb>(
    options =>
    {
        string connectionString = SqlInjector.DbHelper.GetConnectionString();
        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
    });

var app = builder.Build();

app.MapPost("/reading", async (SqlInjectorDb db, [FromBody] Dictionary<string, string> json) =>
{
    if (!json.ContainsKey("loggerID"))
    {
        return Results.Problem("Missing logger ID.");
    }

    if (!json.ContainsKey("apiKey"))
    {
        return Results.Problem("Missing API key.");
    }

    Logger? logger = await db.Loggers.FirstOrDefaultAsync(logger =>
        logger.LoggerId == json["loggerID"] && logger.ApiKey == json["apiKey"]);

    if (logger == null)
    {
        return Results.Problem("Unknown logger ID or API key.");
    }

    if (!json.ContainsKey("temperature"))
    {
        return Results.Problem("Missing temperature.");
    }

    bool validTemperature = uint.TryParse(json["temperature"], out uint temperature);

    if (!validTemperature)
    {
        return Results.Problem("Invalid temperature.");
    }

    if (!json.ContainsKey("humidity"))
    {
        return Results.Problem("Missing humidity.");
    }

    bool validHumidity = uint.TryParse(json["humidity"], out uint humidity);

    if (!validHumidity)
    {
        return Results.Problem("Invalid humidity.");
    }

    if (!json.ContainsKey("timestamp"))
    {
        return Results.Problem("Missing timestamp.");
    }

    bool timestampValid = DateTime.TryParse(json["timestamp"], out DateTime timeStamp);

    if (!timestampValid)
    {
        return Results.Problem("Timestamp format unparseable.");
    }

    SensorReading newReading = new SensorReading()
    {
        Temperature = temperature,
        Humidity = humidity,
        Timestamp = timeStamp,
        Logger = logger
    };

    await db.SensorReadings.AddAsync(newReading);
    await db.SaveChangesAsync();
    
    return Results.Created("/reading", newReading);
});

app.MapPost("/logger", async (SqlInjectorDb db, [FromBody] Dictionary<string, string> json) =>
{
    if (!json.ContainsKey("adminKey"))
    {
        return Results.Problem("Missing adminKey.");
    }
    
    Admin? admin = await db.Admins.FirstOrDefaultAsync(admin => admin.Key == json["adminKey"]);

    if (admin == null)
    {
        return Results.Problem("Invalid admin key.");
    }
    
    if (!json.ContainsKey("friendlyName"))
    {
        return Results.Problem("Missing friendlyName.");
    }

    Logger newLogger = new Logger
    {
        FriendlyName = json["friendlyName"],
        ApiKey = Guid
            .NewGuid()
            .ToString()
            .Replace("-", "") + Guid
            .NewGuid()
            .ToString()
            .Replace("-", ""),
        CreatedBy = admin
    };

    await db.Loggers.AddAsync(newLogger);
    await db.SaveChangesAsync();
    
    return Results.Created("/logger", newLogger);
});

app.Run();
