using Azure.Messaging.ServiceBus;
using eComerce.ProductsMicroservice.BusinessLogicLayer.ServiceBus;
using eCommerce.BusinessLogicLayer.DTO;
using eCommerce.BusinessLogicLayer.ServiceContracts;
using eCommerce.ProductsMicroservice.BusinessLogicLayer.DTO;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace eCommerce.ProductsMicroservice.BusinessLogicLayer.ServiceBus;

public class ServiceBusOrderPlacedConsumer : IServiceBusOrderPlacedConsumer
{
  private readonly ServiceBusProcessor _serviceBusProcessor;
  private readonly ILogger<ServiceBusOrderPlacedConsumer> _logger;
  private readonly IConfiguration _configuration;
  private readonly IServiceScopeFactory _serviceScopeFactory;

  public ServiceBusOrderPlacedConsumer(ILogger<ServiceBusOrderPlacedConsumer> logger, ServiceBusClient serviceBusClient, IConfiguration configuration, IServiceScopeFactory serviceScopeFactory)
  {
    _logger = logger;
    _configuration = configuration;
    _serviceScopeFactory = serviceScopeFactory;

    _serviceBusProcessor = serviceBusClient.CreateProcessor(
      _configuration["ServiceBus:ServiceBus_Orders_Placed_Topic"],
      _configuration["ServiceBus:ServiceBus_Orders_Placed_Products_Subscription"],
      new ServiceBusProcessorOptions()
      {
        AutoCompleteMessages = false
      });

    _serviceBusProcessor.ProcessMessageAsync += _serviceBusProcessor_ProcessMessageAsync;

    _serviceBusProcessor.ProcessErrorAsync += _serviceBusProcessor_ProcessErrorAsync;
  }



  private async Task _serviceBusProcessor_ProcessMessageAsync(ProcessMessageEventArgs arg)
  {
    string messageBodyJson = arg.Message.Body.ToString();
    OrderResponse? orderResponseMessage = JsonSerializer.Deserialize<OrderResponse>(messageBodyJson);

    if (orderResponseMessage != null)
    {
      //Child scope
      using var scope = _serviceScopeFactory.CreateScope();
      var productsService = scope.ServiceProvider.GetRequiredService<IProductsService>();


      await HandleOrderPlacement(orderResponseMessage, productsService);
    }

    // Complete the message to remove it from the queue
    await arg.CompleteMessageAsync(arg.Message);
  }

  private async Task HandleOrderPlacement(OrderResponse orderResponse, IProductsService productsService)
  {
    _logger.LogInformation($"ServiceBus: Order Placed: {orderResponse.OrderID}, Order Date: {orderResponse.OrderDate.ToLongDateString()} {orderResponse.OrderDate.ToLongTimeString()}");

    foreach (OrderItemResponse orderItemResponse in orderResponse.OrderItems)
    {
      var existingProduct = await productsService.GetProductByCondition(temp => temp.ProductID == orderItemResponse.ProductID);

      if (existingProduct != null)
      {
        ProductUpdateRequest productUpdateRequest = new ProductUpdateRequest()
        {
          ProductID = existingProduct.ProductID,
          ProductName = existingProduct.ProductName,
          Category = existingProduct.Category,
          QuantityInStock = existingProduct.QuantityInStock - orderItemResponse.Quantity,
          UnitPrice = existingProduct.UnitPrice
        };

        await productsService.UpdateProduct(productUpdateRequest);
      }

    }
  }

  private Task _serviceBusProcessor_ProcessErrorAsync(ProcessErrorEventArgs arg)
  {
    _logger.LogError(arg.Exception, "Error while handling message.");
    return Task.CompletedTask;
  }


  public async Task ConsumeAsync()
  {
    await _serviceBusProcessor.StartProcessingAsync();
  }

  public async void Dispose()
  {
    await _serviceBusProcessor.DisposeAsync();
  }
}
