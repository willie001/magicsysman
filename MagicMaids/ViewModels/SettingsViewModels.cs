#region Using
using System;

using MagicMaids.EntityModels;
#endregion

namespace MagicMaids.ViewModels
{
	public class UpdateSettingsViewModel: BaseViewModel 
	{
		#region Fields

		#endregion

		#region Constructors
		public UpdateSettingsViewModel(SystemSetting _model): base(_model)
		{
			if (_model == null)
				return;

			this.SettingName = _model.SettingName;
			this.SettingValue = _model.SettingValue;
			this.CodeIdentifier = _model.CodeIdentifier;

		}
		#endregion

		#region Properties, Public
		public String SettingName
		{
			get;
			private set;
		}

		public String SettingValue
		{
			get;
			set;
		}

		public String CodeIdentifier
		{
			get;
			private set;
		}
		#endregion
	}
}
