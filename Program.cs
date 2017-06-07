namespace EventHubSendCore
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.EventHubs;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;

    public class Program
    {
        public static IConfigurationRoot Configuration { get; set; }
        private static EventHubClient eventHubClient;
        private static string EhConnectionString;
        private static string EhEntityPath;

        public static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            Configuration = builder.Build();
            EhConnectionString = Configuration["EventHubConnectionString"];
            EhEntityPath = Configuration["EventHubEntityPath"];

            MainAsync(args).GetAwaiter().GetResult();
        }

        private static async Task MainAsync(string[] args)
        {
            // Creates an EventHubsConnectionStringBuilder object from the connection string, and sets the EntityPath.
            // Typically, the connection string should have the entity path in it, but for the sake of this simple scenario
            // we are using the connection string from the namespace.
            var connectionStringBuilder = new EventHubsConnectionStringBuilder(EhConnectionString)
            {
                EntityPath = EhEntityPath
            };

            eventHubClient = EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());
            await SendMessagesToEventHub(1);
            await eventHubClient.CloseAsync();
        }

        // Creates an event hub client and sends 100 messages to the event hub.
        private static async Task SendMessagesToEventHub(int numMessagesToSend)
        {
            for (var i = 0; i < numMessagesToSend; i++)
            {
                try
                {
                    string now = DateTime.UtcNow.ToString("O");
                    Console.WriteLine(now);
                    double tempF = 60 + Math.Sin(DateTime.UtcNow.Second);
                    var o = new
                    {
                        deviceid = "twin1",
                        timestamp = now,
                        tempF = tempF
                    };
                    string message = JsonConvert.SerializeObject(o);
                    //string message = String.Format("{deviceid':'twin1','timestamp':'{0}','tempF':{1}}", now, tempF);
                    Console.WriteLine($"Sending message: {message}");
                    await eventHubClient.SendAsync(new EventData(Encoding.UTF8.GetBytes(message)));
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"{DateTime.Now} > Exception: {exception.Message}");
                }
                await Task.Delay(10);
            }
            Console.WriteLine($"{numMessagesToSend} messages sent.");
        }
    }
}