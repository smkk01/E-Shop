using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace eCommerce.ProductsService.BusinessLogicLayer.ServiceBus;

public class ServiceBusPublisher : IServiceBusPublisher
{
  private readonly ServiceBusClient _serviceBusClient;
  private readonly IConfiguration _configuration;
  private ServiceBusSender _sender;

  public ServiceBusPublisher(ServiceBusClient serviceBusClient, IConfiguration configuration)
  {
    _serviceBusClient = serviceBusClient;
    _configuration = configuration;
  }


  public async Task Publish<T>(string topicName, Dictionary<string, object> headers, T message)
  {
    _sender = _serviceBusClient.CreateSender(topicName);

    string messageJson = JsonSerializer.Serialize(message);
    ServiceBusMessage serviceBusMessage = new ServiceBusMessage(messageJson);

    foreach (var header in headers)
    {
      serviceBusMessage.ApplicationProperties[header.Key] = header.Value;
    }

    await _sender.SendMessageAsync(serviceBusMessage);
  }
}

