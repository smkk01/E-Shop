namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.ServiceBus;

public interface IServiceBusProductDeletionConsumer : IDisposable
{
  Task ConsumeAsync();
}
