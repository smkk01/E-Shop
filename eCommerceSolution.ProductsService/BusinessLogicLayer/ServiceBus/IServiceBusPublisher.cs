namespace eCommerce.ProductsService.BusinessLogicLayer.ServiceBus;

public interface IServiceBusPublisher
{
  Task Publish<T>(string topicName, Dictionary<string, object> headers, T message);
}

