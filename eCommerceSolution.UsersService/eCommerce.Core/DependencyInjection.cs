using Azure.Identity;
using eCommerce.Core.ServiceContracts;
using eCommerce.Core.Services;
using eCommerce.Core.Validators;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;

namespace eCommerce.Core;

public static class DependencyInjection
{
  /// <summary>
  /// Extension method to add core services to the dependency injection container
  /// </summary>
  /// <param name="services"></param>
  /// <returns></returns>
  public static IServiceCollection AddCore(this IServiceCollection services, IConfiguration configuration)
  {
    //TO DO: Add services to the IoC container
    //Core services often include validation, caching and other business components.

    services.AddTransient<IUsersService, UsersService>();
    services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();


    services.AddScoped<GraphServiceClient>(provider => 
    {
      var scopes = new[] { "https://graph.microsoft.com/.default" };

      var options = new ClientSecretCredentialOptions() { AuthorityHost = AzureAuthorityHosts.AzurePublicCloud };

      var clientSecretCredential = new ClientSecretCredential(
        configuration["AzureAdB2C:TenantId"], 
        configuration["AzureAdB2C:ClientId"],
        configuration["AzureAdB2C:ClientSecret"], 
        options);

      var graphClient = new GraphServiceClient(clientSecretCredential, scopes);

      return graphClient;
    });

    return services;
  }
}
