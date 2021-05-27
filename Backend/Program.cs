using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus;
using Shared;

namespace Backend
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            Console.WriteLine("Backend is starting");

            var endpoint = await ConfigureNServiceBus();

            Console.WriteLine("Backend is running");
            Console.ReadKey();
            Console.WriteLine("Backend says goodbye");
            Thread.Sleep(2000);
        }

        private static async Task<IEndpointInstance> ConfigureNServiceBus()
        {
            var endpointAddress = "Backend";
            var endpointConfiguration = new EndpointConfiguration(endpointAddress);

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