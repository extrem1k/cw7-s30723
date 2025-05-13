using TravelAgencyAPI.Controllers;
using TravelAgencyAPI.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddScoped<ITravelService, TravelService>();


builder.Services.AddEndpointsApiExplorer();


var app = builder.Build();




app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
