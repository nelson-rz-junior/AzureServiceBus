using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Console;

namespace PrivateMessageReceiver
{
    class Program
    {
        private const string ServiceBusConnectionString = "Endpoint=sb://salesteamapp-nrj.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=7ACrl06itL7PNSSNBVgSq4ZK0bB4CvbsZfoLOIypBIE=";
        private static IQueueClient queueClient;

        static async Task Main(string[] args)
        {
            await ReceiveSalesMessageAsync();
        }

        static async Task ReceiveSalesMessageAsync()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddUserSecrets<Program>()
                .Build();

            // Get a connection string to our Azure Storage account.
            var connectionString = configuration["ConnectionStrings:StorageAccount"];
            var queueName = configuration["QueueName"];

            queueClient = new QueueClient(connectionString, queueName);

            WriteLine("======================================================");
            WriteLine("Press ENTER key to exit after receiving all the messages.");
            WriteLine("======================================================");

            RegisterMessageHandler();

            Read();

            await queueClient.CloseAsync();
        }

        static void RegisterMessageHandler()
        {
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                MaxConcurrentCalls = 1,
                AutoComplete = false
            };

            queueClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
        }

        static async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            WriteLine($"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");

            await queueClient.CompleteAsync(message.SystemProperties.LockToken);
        }

        static Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");

            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;

            WriteLine("Exception context for troubleshooting:");
            WriteLine($"- Endpoint: {context.Endpoint}");
            WriteLine($"- Entity Path: {context.EntityPath}");
            WriteLine($"- Executing Action: {context.Action}");
            
            return Task.CompletedTask;
        }
    }
}
