using NServiceBus;

namespace Api
{
    public class RequestRemoveAdminCommand : ICommand
    {
        public int UserId { get; set; }
        public int CompanyId { get; set; }
    }
}