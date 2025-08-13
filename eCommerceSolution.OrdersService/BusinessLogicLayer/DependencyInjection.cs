using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.Validators;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.Mappers;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.ServiceContracts;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.Services;
using StackExchange.Redis;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.RabbitMQ;
using Azure.Messaging.ServiceBus;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.ServiceBus;


namespace eCommerce.OrdersMicroservice.BusinessLogicLayer;

public static class DependencyInjection
{
  public static IServiceCollection AddBusinessLogicLayer(this IServiceCollection services, IConfiguration configuration)
  {
    //TO DO: Add business logic layer services into the IoC container
    services.AddValidatorsFromAssemblyContaining<OrderAddRequestValidator>();

    services.AddAutoMapper(typeof(OrderAddRequestToOrderMappingProfile).Assembly);

    services.AddScoped<IOrdersService, OrdersService>();

    services.AddStackExchangeRedisCache(options =>
    {
      options.Configuration = $"{configuration["REDIS_HOST"]}:{configuration["REDIS_PORT"]}";
    });


    //RabbitMQ
    services.AddTransient<IRabbitMQProductUpdateConsumer, RabbitMQProductUpdateConsumer>();

    services.AddTransient<IRabbitMQProductDeletionConsumer, RabbitMQProductDeletionConsumer>();

    services.AddHostedService<RabbitMQProductUpdateHostedService>();

    services.AddHostedService<RabbitMQProductDeletionHostedService>();


    //ServiceBus
    services.AddSingleton(_ => new ServiceBusClient(configuration["ServiceBus:ecommerce-servicebus-namespace"]));

    services.AddSingleton<IServiceBusProductUpdateConsumer, ServiceBusProductUpdateConsumer>();

    services.AddHostedService<ServiceBusProductUpdateHostedService>();

    services.AddSingleton<IServiceBusProductDeletionConsumer, ServiceBusProductDeletionConsumer>();

    services.AddHostedService<ServiceBusProductDeletionHostedService>();

    services.AddScoped<IServiceBusPublisher, ServiceBusPublisher>();

    return services;
  }
}

