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
}
