using System;
namespace MagicMaids
{
	public class BaseViewModel
	{
		public BaseViewModel()
		{
			//if (!Helpers.IsValidGuid(Id))
			//{
			//	Id = Guid.NewGuid().ToString();
			//}
		}

		public String SqlString { get; set; }

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
