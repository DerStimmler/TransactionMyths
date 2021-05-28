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
            using var connection = new SqlConnection(Configuration.GetConnectionString());

            connection.Open();

            var database = connection.Database;

            var command = connection.CreateCommand();

            command.Connection = connection;

            try
            {
                command.CommandText = @$"
                        USE master

                        IF EXISTS (SELECT * FROM sys.databases WHERE name = '{database}')
                        BEGIN
                            ALTER DATABASE {database} SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                            DROP DATABASE {database};  
                        END;
                        CREATE DATABASE {database};
                        ALTER DATABASE {database} SET ALLOW_SNAPSHOT_ISOLATION ON;

                        USE {database};

                        CREATE TABLE Users (
                                Id INT PRIMARY KEY,
                                CompanyId INT,
		                        IsAdmin BIT
                        )

                        CREATE TABLE Companies (
                                Id INT PRIMARY KEY
                        )

                        INSERT INTO Companies (Id)
                        VALUES (1);

                        INSERT INTO Users (Id, CompanyId, IsAdmin)
                        VALUES (1, 1, 1);
                        INSERT INTO Users (Id, CompanyId, IsAdmin)
                        VALUES (2, 1, 1);
                     ";

                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Couldn't reset DB: {0}", ex.Message);
            }
        }
    }
}