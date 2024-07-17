using System;
using Microsoft.AspNetCore.Mvc;
using PlugzApi.Interfaces;
using PlugzApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(config => config.Filters.Add(new ProducesAttribute("application/json")));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<IEmailService, EmailService>();

builder.WebHost.UseSentry(o =>
{
    o.Dsn = "https://e3bfbc362cec680624b2da747b9f96c5@o4504622977974272.ingest.us.sentry.io/4507613965058048";
    o.Debug = true;
    o.TracesSampleRate = 0.5;
    o.ProfilesSampleRate = 0.5;
    o.FlushOnCompletedRequest = true;
});

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();
CommonService.Initialize(configuration);

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

