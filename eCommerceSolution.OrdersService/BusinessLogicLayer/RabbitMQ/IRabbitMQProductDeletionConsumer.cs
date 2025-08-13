namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.RabbitMQ;

public interface IRabbitMQProductDeletionConsumer
{
  Task Initialize(int delayMilliseconds);
  Task Consume();
  void Dispose();
}

