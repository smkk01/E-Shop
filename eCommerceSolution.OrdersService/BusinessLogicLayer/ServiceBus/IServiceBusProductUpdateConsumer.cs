namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.ServiceBus;

public interface IServiceBusProductUpdateConsumer : IDisposable
{
  Task ConsumeAsync();
}
