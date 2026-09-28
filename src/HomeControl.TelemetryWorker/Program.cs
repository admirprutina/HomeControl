using HomeControl.Infrastructure.Messaging.Kafka;
using HomeControl.TelemetryWorker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSingleton(KafkaSettings.FromConfiguration(builder.Configuration));
builder.Services.AddSingleton<KafkaTopicInitializer>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
