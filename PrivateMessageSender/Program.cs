using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using static System.Console;

namespace PrivateMessageSender
{
    class Program
    {
        private const string ServiceBusConnectionString = "Endpoint=sb://salesteamapp-nrj.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=7ACrl06itL7PNSSNBVgSq4ZK0bB4CvbsZfoLOIypBIE=";
        private static IQueueClient queueClient;

        static async Task Main(string[] args)
        {
            WriteLine("Sending a message to the Sales Messages queue...");

            await SendSalesMessageAsync();

            WriteLine("Message was sent successfully.");
        }

        static async Task SendSalesMessageAsync()
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

            try
            {
                string messageBody = $"$10,000 order for bicycle parts from retailer Adventure Works.";
                var message = new Message(Encoding.UTF8.GetBytes(messageBody));

                WriteLine($"Sending message: {messageBody}");

                await queueClient.SendAsync(message);
            }
            catch (Exception ex)
            {
                WriteLine($"{DateTime.Now} :: Exception: {ex.Message}");
            }
            finally
            {
                await queueClient.CloseAsync();
            }
        }
    }
}
