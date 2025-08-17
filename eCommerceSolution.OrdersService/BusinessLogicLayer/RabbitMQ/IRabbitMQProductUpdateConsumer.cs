namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.RabbitMQ;

public interface IRabbitMQProductUpdateConsumer
{
  Task Initialize(int delayMilliseconds);
  Task Consume();
  void Dispose();
}
