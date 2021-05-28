using System.Data.SqlClient;
using System.Threading.Tasks;
using NServiceBus;
using Shared;

namespace Backend
{
    public class RemoveAdminCommand : ICommand
    {
        public int UserId { get; set; }
    }

    public class RemoveAdminHandler : IHandleMessages<RemoveAdminCommand>
    {
        public Task Handle(RemoveAdminCommand message, IMessageHandlerContext context)
        {
            using var connection = new SqlConnection(Configuration.GetConnectionString());
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = $"UPDATE Users SET IsAdmin = 0 WHERE Id = {message.UserId}";

            command.ExecuteNonQuery();

            Logger.Write($"Removed admin with id '{message.UserId}'");

            return Task.CompletedTask;
        }
    }
}