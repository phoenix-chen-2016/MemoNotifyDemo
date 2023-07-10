using FluentdForward.OpenTelemetry.MessagePack;
using Hangfire;
using Hangfire.MemoryStorage;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using Hangfire.Redis.StackExchange;
using MemoNotify;
using MemoNotify.MessageQueue;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Telegram.Bot;
using Tgs.GameServer.Clients;

var builder = WebApplication.CreateBuilder(args);

builder.Logging
	.AddOpenTelemetry(options =>
	{
		options.IncludeFormattedMessage = true;
		options.ParseStateValues = true;
		_ = options.AddFluentdForwardExporter(fluentOptions =>
		{
			var nodeIp = builder.Configuration.GetValue<string>("NODE_IP")!;
			fluentOptions.Host = nodeIp;
			fluentOptions.Tag = builder.Configuration.GetValue<string>("OTEL_SERVICE_NAME")!;
			fluentOptions.UseMessagePack(LogRecordFormatterResolver.GetResolverInstanceWithExtendInfo(
				"MachineName",
				() => Environment.MachineName));
		});
	});

builder.Services
	.AddSingleton<IRequestContentHandler, GameServerModifierHandler>()
	.AddGameServerClient(httpBuilder => httpBuilder
		.ConfigureHttpClient(http => http.BaseAddress =
			builder.Configuration.GetValue<Uri>("MemoSettings:GameServerBaseUri")))
	.AddHttpClient<ITelegramBotClient, TelegramBotClient>((httpClient) => new TelegramBotClient(
		builder.Configuration.GetValue<string>("MemoSettings:Token")!,
		httpClient));

builder.Services
	.AddResponseCompression()
	.AddControllers()
	.Services
	.AddHangfire(config =>
	{
		_ = config.UseRecommendedSerializerSettings();

		var storageType = builder.Configuration.GetValue<StorageType>("MemoSettings:StorageType");

		switch (storageType)
		{
			case StorageType.MongoDB:
				_ = config.UseMongoStorage(
					builder.Configuration.GetConnectionString("Hangfire")!,
					new MongoStorageOptions
					{
						MigrationOptions = new MongoMigrationOptions
						{
							MigrationStrategy = new MigrateMongoMigrationStrategy(),
							BackupStrategy = new CollectionMongoBackupStrategy()
						},
						MigrationLockTimeout = TimeSpan.FromMinutes(5),
						CheckQueuedJobsStrategy = CheckQueuedJobsStrategy.TailNotificationsCollection
					});
				break;

			case StorageType.Redis:
				_ = config.UseRedisStorage(
					builder.Configuration.GetConnectionString("Hangfire")!,
					new RedisStorageOptions
					{
						Prefix = "hangfire:TstMemo"
					});
				break;

			case StorageType.Memory:
				_ = config.UseMemoryStorage();
				break;
		}
	})
	.AddHangfireServer();

builder.Services
	.AddMessageQueue()
	.AddNatsMessageQueue(natsConfig => natsConfig
		.ConfigQueueOptions((options, _) => options.Url = builder.Configuration.GetConnectionString("Nats")!)
		.AddHandler<TstMemoNotifyHandler>("tgs.tst.notify", "tgs.tst.notify"))
	.AddNatsGlobPatternExchange("*");

builder.Services
	.AddOpenTelemetry()
	.WithTracing(traceBuilder => traceBuilder
		.SetResourceBuilder(ResourceBuilder.CreateDefault()
			.AddEnvironmentVariableDetector())
		.AddSource("Tgs.*")
		.AddAspNetCoreInstrumentation()
		.AddProfileViewExporter()
		.AddOtlpExporter())
	.WithMetrics(meterBuilder => meterBuilder
		.AddAspNetCoreInstrumentation()
		.AddRuntimeInstrumentation()
		.AddOtlpExporter())
	.Services
	.AddProfileViewer()
	.Configure<AspNetCoreInstrumentationOptions>(options => options.Filter =
		httpContext => !httpContext.Request.Path.HasValue
			|| !httpContext.Request.Path.StartsWithSegments("/swagger")
			&& !httpContext.Request.Path.StartsWithSegments("/health")
			&& !httpContext.Request.Path.StartsWithSegments("/profiler/")
			&& !httpContext.Request.Path.StartsWithSegments("/metrics")
			&& !new[] { "html", "ico", "png", "jpg" }.Any(
				ext => httpContext.Request.Path.Value.EndsWith($".{ext}")));

builder.Services
	.AddHealthChecks()
	.Services
	.AddSwaggerGen(genOptions => genOptions.SwaggerDoc(
		"v1",
		new OpenApiInfo { Title = "Tst Memo Notify", Version = "v1" }));

var app = builder.Build();

app.UseResponseCompression();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();
app.MapHangfireDashboard(options: new DashboardOptions
{
	AsyncAuthorization = new[] { new AnonymousHangfireAuthorization() }
});
app.MapHealthChecks("/healthz");
app.UseSwaggerUI();
app.MapSwagger();

app.Run();
