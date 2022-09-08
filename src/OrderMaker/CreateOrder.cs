using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace OrderMaker
{
    public static class CreateOrder
    {
        [FunctionName("CreateOrder")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            [CosmosDB(
                databaseName: "OrdersDatabase",
                collectionName: "Orders",
                ConnectionStringSetting = "CosmosDBConnectionString"
            )] IAsyncCollector<object> orders,
            [CosmosDB(
                databaseName: "OrdersDatabase",
                collectionName: "OrdersCreated",
                ConnectionStringSetting = "CosmosDBConnectionString"
            )] IAsyncCollector<object> ordersCreated,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            // Read the request body and deserialize it into a
            // order object so that it can be saved into CosmosDB.
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var incomingOrder = JsonConvert.DeserializeObject<Order>(requestBody);

            // Insert Order item into CosmosDB container
            await orders.AddAsync(incomingOrder);

            // Insert OrderCreated item into CosmosDB container (outbox)
            var orderCreated = new OrderCreated { 
                AccountNumber = incomingOrder.AccountNumber,
                OrderDate = incomingOrder.OrderDate,
                OrderId = incomingOrder.OrderId,
                Quantity = incomingOrder.Quantity,
                RequestedDate = incomingOrder.RequestedDate,
                OrderProcessed = false
            };
            await ordersCreated.AddAsync(orderCreated);


            return new OkObjectResult("success");
        }
    }
}
