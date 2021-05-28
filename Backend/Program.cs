using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using NServiceBus;
using Shared;

namespace Backend
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            Logger.Write("Backend is starting");

            var endpoint = await ConfigureNServiceBus();

            Logger.Write("Backend is running");
            Console.ReadKey();
            Logger.Write("Backend says goodbye");
        }

        private static async Task<IEndpointInstance> ConfigureNServiceBus()
        {
            var endpointAddress = "Backend";
            var endpointConfiguration = new EndpointConfiguration(endpointAddress);

            endpointConfiguration
                .Recoverability()
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
            endpointConfiguration.LimitMessageProcessingConcurrencyTo(10);

            var transport = endpointConfiguration.UseTransport<LearningTransport>();
            transport.Transactions(TransportTransactionMode.SendsAtomicWithReceive);

            var persistence = endpointConfiguration.UsePersistence<SqlPersistence>();
            persistence.SqlDialect<SqlDialect.MsSqlServer>();

            var databaseConnectionString = Configuration.GetConnectionString();
            persistence.ConnectionBuilder(() => new SqlConnection(databaseConnectionString));

            return await Endpoint.Start(endpointConfiguration).ConfigureAwait(false);
        }
    }
}