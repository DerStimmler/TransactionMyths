using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;

namespace Shared
{
    public class Transactions
    {
        public static void RemoveAdmin(int userId, int delayInSeconds, string txName, int isAdmin = 0)
        {
            using (var connection = new SqlConnection(Configuration.GetConnectionString()))
            {
                connection.Open();

                var transaction = connection.BeginTransaction(IsolationLevel.Snapshot, txName );
                Logger.Write($"{txName} started transaction scope");

                var command = connection.CreateCommand();
                command.Transaction = transaction;

                try
                {
                    command.CommandText = "SELECT COUNT(*) FROM Users Where IsAdmin = 1";
                    var admins = (int) command.ExecuteScalar();
                    Logger.Write($"{txName} found {admins} admins");

                    Thread.Sleep(delayInSeconds * 1000);


                    if (admins > 1)
                    {
                        command.CommandText = $"UPDATE Users SET IsAdmin = {isAdmin} WHERE Id = {userId}";
                        command.ExecuteNonQuery();
                        Logger.Write($"{txName} updated user with id {userId} to admin={isAdmin}");
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    Logger.Write($"Commit Exception Type: {ex.GetType()}");
                    Logger.Write($"Transaction Name: {txName}");
                    Logger.Write($"Message: {ex.Message}");

                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception ex2)
                    {
                        Logger.Write($"Rollback Exception Type: {ex2.GetType()}");
                        Logger.Write($"Transaction Name: {txName}");
                        Logger.Write($"Message: {ex2.Message}");
                    }
                }
            }
        }
    }
}