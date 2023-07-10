using Hangfire;
using MemoNotify.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace MemoNotify.Controller;

[Route("api/[controller]")]
[ApiController]
public class SchedulerController : ControllerBase
{
	[HttpPut]
	[Produces("application/json")]
	public string ScheduleNotify(
		IBackgroundJobClient backgroundJobClient,
		MemoViewModel viewModel)
	{
		var payload = new TstMemoNotify(
			viewModel.Title,
			viewModel.Description,
			viewModel.ScheduleTime,
			viewModel.GameProviderCodes);

		return backgroundJobClient.Schedule<NotifyScheduleJob>(
			job => job.ExecuteAsync(
				$"Notify - {viewModel.Title}",
				"tgs.tst.notify",
				viewModel.ScheduleTime,
				payload,
				default),
			viewModel.ScheduleTime);
	}
}
