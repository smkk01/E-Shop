using eCommerce.API.Middlewares;
using eCommerce.Core;
using eCommerce.Core.Mappers;
using eCommerce.Infrastructure;
using FluentValidation.AspNetCore;
using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://0.0.0.0:9090");

//Add Infrastructure services
builder.Services.AddInfrastructure();
builder.Services.AddCore(builder.Configuration);

// Add controllers to the service collection
builder.Services.AddControllers().AddJsonOptions(options => {
  options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddAutoMapper(typeof(ApplicationUserMappingProfile).Assembly);

//FluentValidations
builder.Services.AddFluentValidationAutoValidation();


//Add API explorer services
builder.Services.AddEndpointsApiExplorer();

//Add swagger generation services to create swagger specification
builder.Services.AddSwaggerGen();

//Add cors services
builder.Services.AddCors(options =>
{
  options.AddDefaultPolicy(builder => {
    builder.WithOrigins("http://localhost:4200")
    .AllowAnyMethod()
    .AllowAnyHeader();
  });
});

//Build the web application
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
  app.UseExceptionHandlingMiddleware();
}

//Routing
app.UseRouting();
app.UseSwagger(); //Adds endpoint that can serve the swagger.json
app.UseSwaggerUI(); //Adds swagger UI (interactive page to explore and test API endpoints)
app.UseCors();


//Auth
app.UseAuthentication();
app.UseAuthorization();

//Controller routes
app.MapControllers();

app.Run();
