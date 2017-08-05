#region Using
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
#endregion

namespace MagicMaids.EntityModels
{
	abstract public class BaseModel : IDataModel, INotifyPropertyChanged
	{
        #region Fields
		public event PropertyChangedEventHandler PropertyChanged;

		private Guid _id;
		private DateTime _updatedDate;
		private DateTime _createdDate;
		private String _updatedBy;
		private Boolean _isActive;
		#endregion

		#region Constructors
		public BaseModel()
		{
			Id = Guid.NewGuid();
			IsActive = true;
		}
        #endregion

        #region Properties, Public
        [Key]
		[Column("Id")]
		public Guid Id
		{
			get
			{
				return _id;	
			}
			set
			{
				if (value != _id)
				{
					_id = value;
					NotifyPropertyChanged();
				}
			}
		}

		public DateTime CreatedAt
		{
			get
			{
				return _createdDate;
			}
			set
			{
				if (value != _createdDate)
				{
					_createdDate = value;
					NotifyPropertyChanged();
				}
			}
		}

		public DateTime UpdatedAt
		{
			get
			{
				return _updatedDate;
			}
			set
			{
				if (value != _updatedDate)
				{
					_updatedDate = value;
					NotifyPropertyChanged();
				}
			}
		}

		public string UpdatedBy
		{
			get
			{
				if (String.IsNullOrWhiteSpace(_updatedBy))
				{
					return "TODO";
				}
				return _updatedBy;
			}
			set
			{
				if (value != _updatedBy)
				{
					_updatedBy = value;
					NotifyPropertyChanged();
				}
			}
		}

        //https://docs.microsoft.com/en-us/aspnet/mvc/overview/getting-started/getting-started-with-ef-using-mvc/handling-concurrency-with-the-entity-framework-in-an-asp-net-mvc-application
        public DateTime RowVersion
        {
            get;
            set;
        }

        public bool IsActive
		{
			get
			{
				return _isActive ;
			}
			set
			{
				if (value != _isActive )
				{
					_isActive = value;
					NotifyPropertyChanged();
				}
			}
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
				_output.Append(this.UpdatedBy);

				if (_output.Length > 0) _output.Append("<br/>");
				_output.Append("<span>Created:</span>&nbsp;");
				_output.Append(this.CreatedAt.ToString());

				if (_output.Length > 0) _output.Append("<br/>");
				_output.Append("<span>Updated:</span>&nbsp;");
				_output.Append(this.UpdatedAt.ToString());

				if (_output.Length > 0) _output.Append("<br/>");
				_output.Append("<span>Active:</span>&nbsp;");
				_output.Append(this.IsActive.ToString());

				_formattedMetadata = "<small>" + _output.ToString() + "</small>";
				return _formattedMetadata;
			}
		}
		#endregion

		#region Methods, Private
		private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
		{
			if (PropertyChanged != null)
			{
				_formattedMetadata = string.Empty;
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
		#endregion 

	}
}

