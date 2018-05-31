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
	}
}
