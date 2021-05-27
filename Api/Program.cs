using System;
using System.Reflection;
using System.Threading.Tasks;
using NServiceBus;
using Shared;

namespace Api
{
    internal class Program
    {
        private static IEndpointInstance _endpoint;

        private static async Task Main(string[] args)
        {
            _endpoint = await ConfigureNServiceBus();

            await SendAdminRemovalRequest(1, 1);
            await SendAdminRemovalRequest(2, 1);
            await SendAdminRemovalRequest(2, 1);
            await SendAdminRemovalRequest(1, 1);
            await SendAdminRemovalRequest(1, 1);
            await SendAdminRemovalRequest(2, 1);
            await SendAdminRemovalRequest(1, 1);
        }

        private static async Task SendAdminRemovalRequest(int userId, int companyId)
        {
            var command = new RequestRemoveAdminCommand
            {
                CompanyId = companyId,
                UserId = userId
            };

            await _endpoint.Send(command);
            
            Logger.Write($"user with id '{userId}' in company '{companyId}' should be degraded");
        }

        private static async Task<IEndpointInstance> ConfigureNServiceBus()
        {
            var endpointAddress = "Api";
            var endpointConfiguration = new EndpointConfiguration(endpointAddress);
            
            endpointConfiguration.SendOnly();
            
            endpointConfiguration.Recoverability()
                .AddUnrecoverableException<NullReferenceException>()
                .Immediate(settings => settings.OnMessageBeingRetried(retry =>
                {
                    Logger.Write($"Message {retry.MessageId} will be retried immediately.");
                    return Task.CompletedTask;
                }))
                .Delayed(settings => settings.OnMessageBeingRetried(retry =>
                {
                    Logger.Write($@"Message {retry.MessageId} will be retried after a delay.");
                    return Task.CompletedTask;
                }))
                .Failed(settings => settings.OnMessageSentToErrorQueue(failed =>
                {
                    Logger.Write($@"Message {failed.MessageId} will be sent to the error queue.");
                    return Task.CompletedTask;
                }));
            
            endpointConfiguration.EnableInstallers();

            var transport = endpointConfiguration.UseTransport<LearningTransport>();
            transport.Transactions(TransportTransactionMode.SendsAtomicWithReceive);
            transport.Routing().RouteToEndpoint(typeof(RequestRemoveAdminCommand), "Backend");

            return await Endpoint.Start(endpointConfiguration).ConfigureAwait(false);
        }
    }
}