namespace eCommerce.ProductsService.BusinessLogicLayer.RabbitMQ;

public interface IRabbitMQPublisher
{
  Task Initialize();
  Task Publish<T>(Dictionary<string, object> headers, T message);
}
