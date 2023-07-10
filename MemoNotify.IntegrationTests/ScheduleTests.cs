using System.Runtime.InteropServices;
using Hangfire;
using Jering.Javascript.NodeJS;
using MemoNotify.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit.Abstractions;

namespace MemoNotify.IntegrationTests;

public class ScheduleTests
{
	private readonly ITestOutputHelper _testOutputHelper;

	public ScheduleTests(ITestOutputHelper testOutputHelper)
	{
		_testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
	}

	[Fact]
	public async Task 排程一個通知()
	{
		// Arrange
		var channelName = Guid.NewGuid().ToString();

		var fakeBackgroundJobClient = Substitute.For<IBackgroundJobClient>();

		using var web = TestSchedulerApplication.Builder
			.UseTestChannelServer(channelName)
			.ConfigureLogging(logging => logging.AddXUnit(_testOutputHelper))
			.ConfigureServices(services => services
				.AddSingleton(fakeBackgroundJobClient))
			.Build();

		await web.StartAsync();

		var request = new MemoViewModel
		{
			ScheduleTime = DateTime.Now,
			Description = "Memo something",
			Title = "Memo title",
			GameProviderCodes = new[] { "GP1", "GP2" }
		};

		var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "true" : "false";

		var nodeModule = $$"""
			const axiosConfigFactory = require('./test/models/axios-configuration.js').default;
			const MemoNotifyClient = require('./test/services/memo-notify-client.js').MemoNotifyClient;
			const AxiosAdapter = require('./test/axios/axios-adapter.js').AxiosAdapter;

			module.exports = async (memoRequest) =>
			{
				let axiosInstance = axiosConfigFactory("{{channelName}}", {{isWindows}});
				let adp = new AxiosAdapter(axiosInstance);

				let svc = new MemoNotifyClient(adp);

				return await svc.scheduleMemoNotify(memoRequest);
			};
			""";

		// Act
		await StaticNodeJSService.InvokeFromStringAsync(
			nodeModule,
			args: new object[] { request });

		// Assert
		_ = fakeBackgroundJobClient.ReceivedWithAnyArgs(1)
			.Schedule<NotifyScheduleJob>(
				job => job.ExecuteAsync<object>(
					default!,
					default!,
					default,
					default!,
					default),
				request.ScheduleTime);
	}
}
