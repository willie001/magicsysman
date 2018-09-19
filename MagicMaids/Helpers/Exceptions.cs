using System;
namespace MagicMaids
{
	[Serializable]
	public class NoTeamRosteredException: Exception 
	{
		public NoTeamRosteredException(string CleanerName, string WeekDay) : base($"{CleanerName} is not rostered for a {WeekDay}")
		{
			
		}
	}

	[Serializable]
	public class NoSuitableGapAvailable : Exception
	{
		public NoSuitableGapAvailable(string WeekDay, Int32 gapSize) : base($"No {gapSize} minute slots available on {WeekDay}")
		{

		}
	}
}
