using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace eCommerce.ProductsService.BusinessLogicLayer.RabbitMQ;

public class RabbitMQPublisher : IRabbitMQPublisher, IDisposable
{
  private readonly IConfiguration _configuration;
  private IChannel _channel;
  private IConnection _connection;

  public RabbitMQPublisher(IConfiguration configuration)
  {
    _configuration = configuration;
  }


  public async Task Initialize()
  {
    Console.WriteLine($"RabbitMQ_HostName: {_configuration["RabbitMQ_HostName"]}");
    Console.WriteLine($"RabbitMQ_UserName: {_configuration["RabbitMQ_UserName"]}");
    Console.WriteLine($"RabbitMQ_Password: {_configuration["RabbitMQ_Password"]}");
    Console.WriteLine($"RabbitMQ_Port: {Environment.GetEnvironmentVariable("RabbitMQ_Port")}");

    string hostName = _configuration["RabbitMQ_HostName"]!;
    string userName = _configuration["RabbitMQ_UserName"]!;
    string password = _configuration["RabbitMQ_Password"]!;
    string port = Environment.GetEnvironmentVariable("RabbitMQ_Port")!;




    ConnectionFactory connectionFactory = new ConnectionFactory()
    {
      HostName = hostName,
      UserName = userName,
      Password = password,
      Port = Convert.ToInt32(port)
    };
    _connection = await connectionFactory.CreateConnectionAsync();

    _channel = await _connection.CreateChannelAsync();
  }


  public async Task Publish<T>(Dictionary<string, object> headers, T message)
  {
    string messageJson = JsonSerializer.Serialize(message);
    byte[] messageBodyInBytes = Encoding.UTF8.GetBytes(messageJson);

    //Create exchange
    string exchangeName = _configuration["RabbitMQ_Products_Exchange"]!;
    await _channel.ExchangeDeclareAsync(exchange: exchangeName, type: ExchangeType.Headers, durable: true);

    //Publish message
    var basicProperties = new BasicProperties();
    basicProperties.Headers = headers;

    await _channel.BasicPublishAsync(
      exchange: exchangeName, 
      routingKey: string.Empty,
      mandatory: true, 
      basicProperties: basicProperties, 
      body: messageBodyInBytes);
  }

  public void Dispose()
  {
    if (_channel != null && _channel.IsClosed == false)
    {
      _channel.Dispose();
    }

    if (_connection != null && _connection.IsOpen)
    {
      _connection.Dispose();
    }
  }
}
