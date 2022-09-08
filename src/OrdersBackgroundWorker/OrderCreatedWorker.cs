using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace OrdersBackgroundWorker
{
    public static class OrderCreatedWorker
    {
        [FunctionName("OrderCreatedWorker")]
        public static async Task Run(
            [CosmosDBTrigger(
                databaseName: "OrdersDatabase",
                collectionName: "Orders",
                ConnectionStringSetting = "CosmosDBConnectionString",
                CreateLeaseCollectionIfNotExists = true,
                LeaseCollectionName = "order-leases")] IReadOnlyList<Document> input,
            [CosmosDB(
                databaseName: "OrdersDatabase",
                collectionName: "OrdersCreated",
                ConnectionStringSetting = "CosmosDBConnectionString",
                SqlQuery = "select * from OrdersCreated r where r.OrderProcessed = false")] IEnumerable<Document> ordersCreated,
            [CosmosDB(
                databaseName: "OrdersDatabase",
                collectionName: "OrdersCreated",
                ConnectionStringSetting = "CosmosDBConnectionString"
            )] DocumentClient client,
            [ServiceBus(
                "orders", 
                Connection = "ServiceBusConnectionString", 
                EntityType = ServiceBusEntityType.Queue
            )] IAsyncCollector<Order> ordersToProcess,
            ILogger log)
        {
            if (input != null && input.Count > 0)
            {
                log.LogInformation("Documents modified " + input.Count);
                log.LogInformation("First document Id " + input[0].Id);

                // Iterate throught the collection of orders that are ready to be processed
                foreach (var o in ordersCreated)
                {
                    // Send the order to a service bus topic
                    var order = JsonConvert.DeserializeObject<Order>(o.ToString());
                    await ordersToProcess.AddAsync(order);

                    // Update the order processed flag in CosmosDB to complete the outbox transaction
                    o.SetPropertyValue("OrderProcessed", true);
                    await client.ReplaceDocumentAsync(o.SelfLink, o);
                }
               

            }
        }
    }
}
