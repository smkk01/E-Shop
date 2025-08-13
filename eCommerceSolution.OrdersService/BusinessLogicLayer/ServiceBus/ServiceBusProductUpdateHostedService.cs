using Microsoft.Extensions.Hosting;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.ServiceBus;

public class ServiceBusProductUpdateHostedService : IHostedService
{
  private readonly IServiceBusProductUpdateConsumer _productNameUpdateConsumer;

  public ServiceBusProductUpdateHostedService(IServiceBusProductUpdateConsumer consumer)
  {
    _productNameUpdateConsumer = consumer;
  }


  public async Task StartAsync(CancellationToken cancellationToken)
  {
    await _productNameUpdateConsumer.ConsumeAsync();
  }

  public Task StopAsync(CancellationToken cancellationToken)
  {
    _productNameUpdateConsumer.Dispose();

    return Task.CompletedTask;
  }
}
