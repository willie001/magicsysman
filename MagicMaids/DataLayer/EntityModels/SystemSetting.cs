#region Using
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
#endregion

//https://github.com/JeremySkinner/FluentValidation/wiki/h.-ASP.NET-MVC-5-integration

namespace MagicMaids.EntityModels
{
    [Table("SystemSettings")]
	public class SystemSetting : BaseModel
	{
		#region Properties, Public
		[Required]
		[DataType(DataType.Text)]
		public String SettingName
		{
			get;
			set;
		}

		[Required]
		[DataType(DataType.Text)]
		public String SettingValue
		{
			get;
			set;
		}

		[Required]
		[DataType(DataType.Text)]
		public String CodeIdentifier
		{
			get;
			set;
		}
		#endregion
	}
}
