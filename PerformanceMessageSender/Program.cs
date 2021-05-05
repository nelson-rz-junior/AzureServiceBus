using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using static System.Console;

namespace PerformanceMessageSender
{
    class Program
    {
        private const string ServiceBusConnectionString = "Endpoint=sb://salesteamapp-nrj.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=7ACrl06itL7PNSSNBVgSq4ZK0bB4CvbsZfoLOIypBIE=";
        private static ITopicClient topicClient;

        static async Task Main(string[] args)
        {
            WriteLine("Sending a message to the Sales Performance topic...");

            await SendPerformanceMessageAsync();

            WriteLine("Message was sent successfully.");
        }

        static async Task SendPerformanceMessageAsync()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddUserSecrets<Program>()
                .Build();

            // Get a connection string to our Azure Storage account.
            var connectionString = configuration["ConnectionStrings:StorageAccount"];
            var topicName = configuration["TopicName"];

            topicClient = new TopicClient(connectionString, topicName);

            try
            {
                string messageBody = $"Total sales for Brazil in August: $13m.";
                var message = new Message(Encoding.UTF8.GetBytes(messageBody));

                WriteLine($"Sending message: {messageBody}");

                await topicClient.SendAsync(message);
            }
            catch (Exception exception)
            {
                WriteLine($"{DateTime.Now} :: Exception: {exception.Message}");
            }
            finally
            {
                await topicClient.CloseAsync();
            }
        }
    }
}
