using System.Security.Cryptography.X509Certificates;
using Couchbase.Extensions.DependencyInjection;
using MailParserMicroService;
using Microsoft.AspNetCore.Server.Kestrel.Https;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
ConfigurationManager configurationManager = builder.Configuration;
IWebHostEnvironment env = builder.Environment;
ConfigureHostBuilder host = builder.Host;
IServiceCollection serviceCollection = builder.Services;
serviceCollection.InitSettings(configurationManager, env);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseCors();

var appLifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
appLifetime.ApplicationStopped.Register(async () =>
    {
        await app.Services.GetRequiredService<ICouchbaseLifetimeService>().CloseAsync()
            .ConfigureAwait(false);
    }
);

// Указываем путь к файлу сертификата
//var cert = new X509Certificate2("C:\\Users\\Xok-s\\OneDrive\\Документы\\MyProject\\старый код\\certificate.pem", "1234");

app.Run();


