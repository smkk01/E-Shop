using Microsoft.Extensions.Hosting;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.RabbitMQ;

public class RabbitMQProductUpdateHostedService : IHostedService
{
  private readonly IRabbitMQProductUpdateConsumer _productNameUpdateConsumer;

  public RabbitMQProductUpdateHostedService(IRabbitMQProductUpdateConsumer consumer)
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
