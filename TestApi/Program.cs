using Intercessor;
using Intercessor.Abstractions;
using RabbitMQ.Client;
using Transponder.Abstractions;
using Transponder.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddRabbitMq(cfg => cfg.RegisterFromAssembly(typeof(Program).Assembly));

builder.Services.AddIntercessor(cfg => cfg.RegisterFromAssembly(typeof(Program).Assembly));

builder.Services.AddSingleton(_ => new ConnectionFactory
{
    HostName = "localhost",
    Port = 5672,
    UserName = "guest",
    Password = "guest",
    AutomaticRecoveryEnabled = true,
    NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
});

builder.Services.AddSingleton<IRabbitMqConnectionFactory, RabbitMqConnectionFactory>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();