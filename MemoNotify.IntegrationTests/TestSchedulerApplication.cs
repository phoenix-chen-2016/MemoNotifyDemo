using System.Runtime.InteropServices;
using HttpOverStream.NamedPipe;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MemoNotify.IntegrationTests;

internal static class TestSchedulerApplication
{
	public static IHostBuilder Builder => new SchedulerApplication().Builder
		.ConfigureLogging(logging => logging.ClearProviders())
		.ConfigureAppConfiguration(configBuilder => configBuilder.Sources.Clear())
		.ConfigureServices(services => services
			.AddFakeNatsMessageQueue());

	internal static IHostBuilder UseTestChannelServer(this IHostBuilder hostBuilder, string channelName)
	{
		var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

		return isWindows
			? hostBuilder
				.ConfigureWebHost(webHostBuilder => webHostBuilder
					.UseHttpOverStreamServer(new NamedPipeListener(channelName)))
			: hostBuilder
				.ConfigureWebHost(webHostBuilder => webHostBuilder
					.UseKestrel(options => options.ListenUnixSocket(
						 $"/tmp/{channelName}.sock")));
	}

	private class SchedulerApplication : WebApplicationFactory<Program>
	{
		private IHostBuilder? _builder;

		public IHostBuilder Builder
		{
			get
			{
				_ = Server;

				return _builder!;
			}
		}

		protected override IHost CreateHost(IHostBuilder builder)
		{
			_builder = builder;

			return Host.CreateDefaultBuilder()
				.ConfigureWebHostDefaults(webBuilder => webBuilder
					.UseTestServer())
				.Build();
		}
	}
}
