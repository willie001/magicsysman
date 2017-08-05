#region Using
using System;

using MagicMaids.EntityModels;
#endregion

namespace MagicMaids.ViewModels
{
	public class BaseViewModel
	{
		#region Fields
		private DateTime _createdDate;
		private DateTime _updatedDate;
		private String _updatedBy;
		#endregion

		#region Constructors
		public BaseViewModel(BaseModel _model)
		{
			this.Id = _model.Id;
			this.IsActive = _model.IsActive;

			_createdDate = _model.CreatedAt;
			_updatedDate = _model.UpdatedAt;
			_updatedBy = _model.UpdatedBy; 
		}
		#endregion

		#region Properties, Public
		public Guid Id 
		{ 
			get; 
			private set; 
		}

		public bool IsActive 
		{ 
			get; 
			private set; 
		}

		private string _formattedMetadata;
		public string Metadata
		{
			get
			{
				if (!String.IsNullOrWhiteSpace(_formattedMetadata))
				{
					return _formattedMetadata;
				}

				System.Text.StringBuilder _output = new System.Text.StringBuilder();

				if (_output.Length > 0) _output.Append("<br/>");
				_output.Append("<span>Guid:</span>&nbsp;");
				_output.Append(this.Id.ToString());

				if (_output.Length > 0) _output.Append("<br/>");
				_output.Append("<span>Update By:</span>&nbsp;");
				_output.Append(this._updatedBy);

				if (_output.Length > 0) _output.Append("<br/>");
				_output.Append("<span>Created:</span>&nbsp;");
				_output.Append(this._createdDate.ToString());

				if (_output.Length > 0) _output.Append("<br/>");
				_output.Append("<span>Updated:</span>&nbsp;");
				_output.Append(this._updatedDate.ToString());

				if (_output.Length > 0) _output.Append("<br/>");
				_output.Append("<span>Active:</span>&nbsp;");
				_output.Append(this.IsActive.ToString());

				_formattedMetadata = "<small>" + _output.ToString() + "</small>";
				return _formattedMetadata;
			}
		}
		#endregion
	}
}
