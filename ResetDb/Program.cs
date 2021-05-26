using System;
using System.Data.SqlClient;
using Shared;

namespace ResetDb
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Resetting DB...");
            ResetDatabase();
            Console.WriteLine("Reset successful");
        }

        private static void ResetDatabase()
        {
            using (var connection = new SqlConnection(Configuration.GetConnectionString()))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.Connection = connection;

                try
                {
                    command.CommandText = @"
USE master

IF EXISTS (SELECT * FROM sys.databases WHERE name = 'TestDB')
BEGIN
    ALTER DATABASE TestDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE TestDB;  
END;
CREATE DATABASE TestDB;
ALTER DATABASE TestDB SET ALLOW_SNAPSHOT_ISOLATION ON;

USE TestDB;

CREATE TABLE Users (
        Id INT PRIMARY KEY,
		IsAdmin BIT
)

INSERT INTO Users (Id, IsAdmin)
VALUES (1, 1);
INSERT INTO Users (Id, IsAdmin)
VALUES (2, 1);
";
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Commit Exception Type: {0}", ex.GetType());
                    Console.WriteLine("  Message: {0}", ex.Message);
                }
            }
        }
    }
}