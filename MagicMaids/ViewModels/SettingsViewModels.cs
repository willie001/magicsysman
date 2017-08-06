#region Using
using System;

using MagicMaids.EntityModels;
#endregion

namespace MagicMaids.ViewModels
{
	public class UpdateSettingsViewModel
	{
		#region Fields

		#endregion

		#region Constructors
		public UpdateSettingsViewModel()
		{
			
		}

		public UpdateSettingsViewModel(SystemSetting _model)
		{
			if (_model == null)
				return;

			this.Id = _model.Id;
			this.SettingName = _model.SettingName;
			this.SettingValue = _model.SettingValue;
			this.CodeIdentifier = _model.CodeIdentifier;
			this.RowVersion = _model.RowVersion;
			this.Metadata = _model.Metadata;
		}
		#endregion

		#region Properties, Public
		public Guid Id
		{
			get;
			private set;
		}
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

		public DateTime RowVersion
		{
			get;
		    set;
		}

		public string Metadata
		{
			get;
			private set;
		}
		#endregion
	}
}
