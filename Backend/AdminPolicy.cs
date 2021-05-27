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
        Saga<EnsureAtLeastOneCompanyAdminPolicyData>,
        IAmStartedByMessages<RequestRemoveAdminCommand>
    {
        public async Task Handle(RequestRemoveAdminCommand message, IMessageHandlerContext context)
        {
            //SQL Database starts with 2 Admins
            if (Data.AdminCount == 0)
                Data.AdminCount = 2;

            Logger.Write($"Saga found {Data.AdminCount} admins");

            if (Data.AdminCount == 1)
            {
                Logger.Write("Admin will not be removed since he is the last one standing :)");
                return;
            }
            
            var command = new RemoveAdminCommand {UserId = message.UserId};
            await context.SendLocal(command);

            Logger.Write("Removal of admin allowed.");
            Data.AdminCount--;
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<EnsureAtLeastOneCompanyAdminPolicyData> mapper)
        {
            mapper.ConfigureMapping<RequestRemoveAdminCommand>(command => command.CompanyId).ToSaga(saga => saga.CompanyId);
        }
    }

    public class EnsureAtLeastOneCompanyAdminPolicyData
        : ContainSagaData
    {
        public int AdminCount { get; set; }
        public int CompanyId { get; set; }
    }
}