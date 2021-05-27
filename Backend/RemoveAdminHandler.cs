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
            Transactions.RemoveAdmin(message.UserId);

            Logger.Write($"Removed admin with id '{message.UserId}'");

            return Task.CompletedTask;
        }
    }
}