using System.Text.Json.Serialization;

namespace MemoNotify.ViewModels;

public class MemoViewModel
{
	public required DateTime ScheduleTime { get; set; }

	public required string Title { get; set; }

	public required string Description { get; set; }

	[JsonPropertyName("gpCodes")]
	public required string[] GameProviderCodes { get; set; }
}
