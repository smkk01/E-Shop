namespace eComerce.ProductsMicroservice.BusinessLogicLayer.ServiceBus;

public interface IServiceBusOrderPlacedConsumer : IDisposable
{
  Task ConsumeAsync();
}

