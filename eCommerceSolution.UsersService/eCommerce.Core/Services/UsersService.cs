using AutoMapper;
using eCommerce.Core.DTO;
using eCommerce.Core.Entities;
using eCommerce.Core.RepositoryContracts;
using eCommerce.Core.ServiceContracts;
using Microsoft.Graph;
using System.Text.Json;

namespace eCommerce.Core.Services;

internal class UsersService : IUsersService
{
  private readonly IUsersRepository _usersRepository;
  private readonly IMapper _mapper;
  private readonly GraphServiceClient _graphServiceClient;

  public UsersService(IUsersRepository usersRepository, IMapper mapper, GraphServiceClient graphServiceClient)
  {
    _usersRepository = usersRepository;
    _mapper = mapper;
    _graphServiceClient = graphServiceClient;
  }

  public async Task<UserDTO> GetUserByUserID(Guid userID)
  {
    var existingUser = await _graphServiceClient.Users[Convert.ToString(userID)]
      .GetAsync();
    if (existingUser == null)
    {
      return null;
    }

    Console.WriteLine(JsonSerializer.Serialize(existingUser));

    var user = new UserDTO(userID, existingUser.UserPrincipalName, existingUser.GivenName, existingUser.Surname);
    //UserPrincipalName e.g: ccd82399-5326-4a54-840d-e0bc12ab8f2e@harshawebuniversity.onmicrosoft.com

    return user;
  }

  public async Task<AuthenticationResponse?> Login(LoginRequest loginRequest)
  {
    ApplicationUser? user = await _usersRepository.GetUserByEmailAndPassword(loginRequest.Email, loginRequest.Password);

    if (user == null)
    {
      return null;
    }

    //return new AuthenticationResponse(user.UserID, user.Email, user.PersonName, user.Gender, "token", Success: true);
    return _mapper.Map<AuthenticationResponse>(user) with { Success = true, Token = "token" };
  }


  public async Task<AuthenticationResponse?> Register(RegisterRequest registerRequest)
  {
    //Create a new ApplicationUser object from RegisterRequest
    ApplicationUser user = _mapper.Map<ApplicationUser>(registerRequest);
    ApplicationUser? registeredUser = await _usersRepository.AddUser(user);
    if (registeredUser == null)
    {
      return null;
    }

    //Return success response
    //return new AuthenticationResponse(registeredUser.UserID, registeredUser.Email, registeredUser.PersonName, registeredUser.Gender, "token", Success: true);
    return _mapper.Map<AuthenticationResponse>(registeredUser) with { Success = true, Token = "token" };
  }
}
