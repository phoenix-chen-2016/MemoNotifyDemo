namespace MemoNotify;

public record TstMemoNotify(
	string Title,
	string Description,
	DateTime ScheduleTime,
	string[] GameProviderCodes);
