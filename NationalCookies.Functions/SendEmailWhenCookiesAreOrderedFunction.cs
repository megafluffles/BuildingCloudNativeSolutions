using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using NationalCookies.Data;
using SendGrid.Helpers.Mail;

namespace NationalCookies.Functions
{
    public static class SendEmailWhenCookiesAreOrderedFunction
    {
        [FunctionName("SendEmailWhenCookiesAreOrderedFunction")]
        public static async Task Run([CosmosDBTrigger(
            databaseName: "CookiesDatabase",
            collectionName: "Orders",
            ConnectionStringSetting = "CosmosDBConnectionString",
            LeaseCollectionName = "leases",
            CreateLeaseCollectionIfNotExists = true)]
            IReadOnlyList<Document> input,
            [SendGrid(ApiKey = "SendGridKey")] IAsyncCollector<SendGridMessage> messageCollector,
            ExecutionContext context)
        {
            if (input?.Count > 0)
            {
                Order order = (Order)(dynamic)input[0];

                IConfigurationRoot config = new ConfigurationBuilder()
                 .SetBasePath(context.FunctionAppDirectory)
                 .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                 .AddEnvironmentVariables()
                 .Build();

                var websiteUrl = config["NationalCookiesUrl"];

                var content = "You have a new order!</br></br>" +
                                    "Order date: " + order.Date.ToString("ddMMyyyy") + "</br>" +
                                    "Price: �" + order.Price + "</br></br>" +
                                    "More details <a href='" + websiteUrl + "/Order/Detail?id=" + order.Id + "'>here</a>";

                SendGridMessage message = new SendGridMessage();
                message.AddTo("terttech@gmail.com");
                message.AddContent("text/html", content);
                message.SetFrom(new EmailAddress("justin.dean.brown@gmail.com"));
                message.SetSubject("Somebody ordered cookies!");

                await messageCollector.AddAsync(message).ConfigureAwait(false);
            }
        }
    }
}
