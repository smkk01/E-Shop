using Microsoft.Extensions.Hosting;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.RabbitMQ;

public class RabbitMQProductDeletionHostedService : IHostedService
{
  private readonly IRabbitMQProductDeletionConsumer _productNameUpdateConsumer;

  public RabbitMQProductDeletionHostedService(IRabbitMQProductDeletionConsumer consumer)
  {
    _productNameUpdateConsumer = consumer;
  }


  public async Task StartAsync(CancellationToken cancellationToken)
  {
    await _productNameUpdateConsumer.Initialize(4000);
    await _productNameUpdateConsumer.Consume();
  }

  public Task StopAsync(CancellationToken cancellationToken)
  {
    _productNameUpdateConsumer.Dispose();

    return Task.CompletedTask;
  }
}
