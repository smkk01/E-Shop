using Azure.Messaging.ServiceBus;
using eCommerce.ProductsService.BusinessLogicLayer.RabbitMQ;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.ServiceBus;

public class ServiceBusProductDeletionConsumer : IServiceBusProductDeletionConsumer
{
  private readonly ServiceBusProcessor _serviceBusProcessor;
  private readonly IDistributedCache _distributedCache;
  private readonly ILogger<ServiceBusProductDeletionConsumer> _logger;
  private readonly IConfiguration _configuration;

  public ServiceBusProductDeletionConsumer(IDistributedCache distributedCache, ILogger<ServiceBusProductDeletionConsumer> logger, ServiceBusClient serviceBusClient, IConfiguration configuration)
  {
    _distributedCache = distributedCache;
    _logger = logger;
    _configuration = configuration;

    _serviceBusProcessor = serviceBusClient.CreateProcessor(
      _configuration["ServiceBus:ServiceBus_Products_Deletions_Topic"], 
      _configuration["ServiceBus:ServiceBus_Products_Deletions_Topic_Orders_Subscriptions"], 
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
    ProductDeletionMessage? productDeletionMessage = JsonSerializer.Deserialize<ProductDeletionMessage>(messageBodyJson);

    if (productDeletionMessage != null)
    {
      await HandleProductDeletion(productDeletionMessage);
    }

    // Complete the message to remove it from the queue
    await arg.CompleteMessageAsync(arg.Message);
  }

  private async Task HandleProductDeletion(ProductDeletionMessage productDeletionMessage)
  {
    _logger.LogInformation($"ServiceBus: Product deleted: {productDeletionMessage.ProductID}, New name: {productDeletionMessage.ProductName}");


    string cacheKeyToDelete = $"product:{productDeletionMessage.ProductID}";

    await _distributedCache.RemoveAsync(cacheKeyToDelete);
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
