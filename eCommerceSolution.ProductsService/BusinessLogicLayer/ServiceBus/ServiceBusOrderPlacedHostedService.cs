using eComerce.ProductsMicroservice.BusinessLogicLayer.ServiceBus;
using Microsoft.Extensions.Hosting;

namespace eCommerce.ProductsMicroservice.BusinessLogicLayer.ServiceBus;

public class ServiceBusOrderPlacedHostedService : IHostedService
{
  private readonly IServiceBusOrderPlacedConsumer _productNameUpdateConsumer;

  public ServiceBusOrderPlacedHostedService(IServiceBusOrderPlacedConsumer consumer)
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
