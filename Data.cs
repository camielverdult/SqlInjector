using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Pomelo.EntityFrameworkCore.MySql;

namespace SqlInjector.Models
{
    public class SensorReading
    {
        [Required] public float Temperature;
        [Required] public float Humidity;
        [Required] public DateTime Timestamp;
        [Required] public Logger Logger;
    }

    public class Logger
    {
        [Required] public string LoggerId;
        [Required] public string FriendlyName;
        [Required] public string ApiKey;
        [Required] public Admin CreatedBy;
    }
    
    public class Admin
    {
        [Required]
        public int AdminId { get; set; }
        
        [Required]
        public string FirstName { get; set; }
        
        [Required]
        public string LastName { get; set; }
        
        [Required]
        public string Key { get; set; }
    }
}

namespace SqlInjector
{
    class DbHelper
    {
        public static string GetConnectionString()
        {   
            // Grab environment variables and see if they are present
            string? server = Environment.GetEnvironmentVariable("DB_SERVER");
            string? database = Environment.GetEnvironmentVariable("DB_DATABASE");
            string? user = Environment.GetEnvironmentVariable("DB_USER");
            string? password = Environment.GetEnvironmentVariable("DB_PASSWORD");

            // Check if any of the needed database credentials are missing
            if (server == null) {
                throw new Exception("SqlInjector: Missing DB_SERVER environment variable needed for database connection!");
            }

            if (database == null) {
                throw new Exception("SqlInjector: Missing DB_DATABASE environment variable needed for database connection!");
            }

            if (user == null) {
                throw new Exception("SqlInjector: Missing DB_USER environment variable needed for database connection!");
            }

            if (password == null) {
                throw new Exception("SqlInjector: Missing DB_PASSWORD environment variable needed for database connection!");
            }

            // Build connection string
            string connectionString = $"Server={server};Database={database};User={user};Password={password};"; 

            return connectionString;
        }
    }
    
    public class SqlInjectorDb : DbContext
    {
        public SqlInjectorDb(DbContextOptions options) : base(options) {}
        
        public DbSet<Models.SensorReading> SensorReadings { get; set; }
        public DbSet<Models.Logger> Loggers { get; set; }
        public DbSet<Models.Admin> Admins { get; set; }
    }

    public class PintAppleDbContextFactory : IDesignTimeDbContextFactory<SqlInjectorDb>
    {
        public SqlInjectorDb CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<SqlInjectorDb>();

            string connectionString = SqlInjector.DbHelper.GetConnectionString();
            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            
            return new SqlInjectorDb(optionsBuilder.Options);
        }
    }
}