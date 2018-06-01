#region Using
using System;

using FluentValidation.Attributes;

using MagicMaids.EntityModels;
using MagicMaids.Validators;
#endregion

namespace MagicMaids.ViewModels
{
	[Validator(typeof(SettingsValidator))]
	public class UpdateSettingsViewModel: BaseViewModel 
	{
		#region Properties, Public
		public String Id
		{
			get;
			set;
		}
		public String SettingName
		{
			get;
			set;
		}

		public String SettingValue
		{
			get;
			set;
		}

		public String CodeIdentifier
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

		#region Methods, Public
		public void PopulateVM(SystemSetting entityModel)
		{
			if (entityModel == null)
				return;

			this.Id = entityModel.Id;
			this.SettingName = entityModel.SettingName;
			this.SettingValue = entityModel.SettingValue;
			this.CodeIdentifier = entityModel.CodeIdentifier;

			FormatMetadata(entityModel);
		}

		#endregion

		#region Methods, Private
		private void FormatMetadata(SystemSetting entityModel)
		{
			if (entityModel == null)
			{
				Metadata = string.Empty;
				return;
			}

			System.Text.StringBuilder _output = new System.Text.StringBuilder();

			if (_output.Length > 0) _output.Append("<br/>");
			_output.Append("<span>Guid:</span>&nbsp;");
			_output.Append(entityModel.Id.ToString());

			if (_output.Length > 0) _output.Append("<br/>");
			_output.Append("<span>Update By:</span>&nbsp;");
			_output.Append(entityModel.UpdatedBy);

			if (_output.Length > 0) _output.Append("<br/>");
			_output.Append("<span>Created:</span>&nbsp;");
			_output.Append(entityModel.CreatedAt.ToUser().FormatUserDateTime());

			if (_output.Length > 0) _output.Append("<br/>");
			_output.Append("<span>Updated:</span>&nbsp;");
			_output.Append(entityModel.UpdatedAt.ToUser().FormatUserDateTime());

			if (_output.Length > 0) _output.Append("<br/>");
			_output.Append("<span>Active:</span>&nbsp;");
			_output.Append(entityModel.IsActive.ToString());

			Metadata = "<small>" + _output.ToString() + "</small>";
		}
		#endregion 

	}
}
