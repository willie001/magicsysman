using System;
namespace MagicMaids
{
	public class BaseViewModel
	{
		public BaseViewModel()
		{
		}

		public String SqlString { get; set; }
		public String DebugData { get; set; }

		public String LocalDate
		{
			get
			{
				return DateTimeWrapper.DisplayLocalNow();
			}
		}

		public String UTCDate
		{
			get
			{
				return DateTimeWrapper.DisplayUtcNow();	
			}
		}
	}
}
