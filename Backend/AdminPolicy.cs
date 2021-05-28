using System.Threading.Tasks;
using Api;
using NServiceBus;
using Shared;

namespace Backend
{
    /// <summary>
    ///     EnsureAtLeastOneCompanyAdminPolicy
    /// </summary>
    public class AdminPolicy :
        Saga<AdminPolicyData>,
        IAmStartedByMessages<RequestRemoveAdminCommand>
    {
        public async Task Handle(RequestRemoveAdminCommand message, IMessageHandlerContext context)
        {
            //SQL Database starts with 2 Admins so we have to initialize the value
            if (Data.AdminCount == 0)
                Data.AdminCount = 2;

            if (Data.AdminCount == 1)
            {
                Logger.Write($"Policy restricted removal of admin with id {message.UserId}");
                return;
            }

            var command = new RemoveAdminCommand {UserId = message.UserId};
            await context.SendLocal(command);

            Logger.Write($"Policy allows removal of admin with id {message.UserId}");

            Data.AdminCount--;
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<AdminPolicyData> mapper)
        {
            mapper
                .ConfigureMapping<RequestRemoveAdminCommand>(command => command.CompanyId)
                .ToSaga(saga => saga.CompanyId);
        }
    }


    public class AdminPolicyData
        : ContainSagaData
    {
        public int AdminCount { get; set; }
        public int CompanyId { get; set; }
    }
}