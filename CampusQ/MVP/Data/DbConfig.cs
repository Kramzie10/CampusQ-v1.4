using System;
using System.Data.SqlClient;
using System.Diagnostics;

namespace CampusQ.MVP.Data
{
    public static class DbConfig
    {
        public static string ConnectionString { get; set; } = "Data Source=MSI\\SQLEXPRESS;Initial Catalog=CampusQ;Integrated Security=True;";

        public static void EnsureDatabaseAndTables()
        {
            try
            {
                var builder = new SqlConnectionStringBuilder(ConnectionString);
                var database = builder.InitialCatalog;
                if (string.IsNullOrWhiteSpace(database))
                {
                    // fallback name
                    database = "CampusQ";
                    builder.InitialCatalog = database;
                    ConnectionString = builder.ConnectionString;
                }

                var masterBuilder = new SqlConnectionStringBuilder(ConnectionString)
                {
                    InitialCatalog = "master"
                };

                using (var conn = new SqlConnection(masterBuilder.ConnectionString))
                {
                    conn.Open();
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = $"IF DB_ID(N'{database}') IS NULL CREATE DATABASE [{database}];";
                    cmd.ExecuteNonQuery();
                }

                var targetBuilder = new SqlConnectionStringBuilder(ConnectionString)
                {
                    InitialCatalog = database
                };

                using (var conn = new SqlConnection(targetBuilder.ConnectionString))
                {
                    conn.Open();
                    using var cmd = conn.CreateCommand();

                    cmd.CommandText = @"IF OBJECT_ID(N'dbo.Users') IS NULL
BEGIN
CREATE TABLE dbo.Users(
    Username NVARCHAR(100) PRIMARY KEY,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    Salt NVARCHAR(200) NOT NULL,
    Role NVARCHAR(50) NOT NULL,
    CreatedAt DATETIME2 NOT NULL
);
END";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = @"IF OBJECT_ID(N'dbo.Queue') IS NULL
BEGIN
CREATE TABLE dbo.Queue(
    TicketNumber INT IDENTITY(1,1) PRIMARY KEY,
    ServiceTicketNumber INT NOT NULL DEFAULT(0),
    Purpose NVARCHAR(200) NOT NULL,
    Service NVARCHAR(100) NOT NULL,
    TimeAdded DATETIME2 NOT NULL
);
END";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = @"IF OBJECT_ID(N'dbo.QueueHistory') IS NULL
BEGIN
CREATE TABLE dbo.QueueHistory(
    TicketNumber INT PRIMARY KEY,
    ServiceTicketNumber INT NOT NULL DEFAULT(0),
    Purpose NVARCHAR(200) NOT NULL,
    Service NVARCHAR(100) NOT NULL,
    TimeAdded DATETIME2 NOT NULL,
    ServedAt DATETIME2 NOT NULL
);
END";
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EnsureDatabaseAndTables failed: {ex}");
                Trace.TraceError($"EnsureDatabaseAndTables failed: {ex}");
            }
        }
    }
}