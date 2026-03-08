using Api.Configuration;

var builder = WebApplication.CreateBuilder(args);
builder.AddEngineModules(typeof(Program).Assembly);
builder.Services.AddOpenApi();

var app = builder.Build();
if (app.Environment.IsDevelopment()) app.MapOpenApi();

app.UseHttpsRedirection();
app.UseApplication();
app.Run();