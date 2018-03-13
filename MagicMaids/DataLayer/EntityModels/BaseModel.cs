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
		[Required]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
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

		[DataType(DataType.DateTime)]
		[Required]
		[DisplayFormat(DataFormatString="{0:dd/MM/yyyy hh:mm:ss}")]
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

		[DataType(DataType.DateTime)]
		[DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm:ss}")]
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

		[Required]
		[DataType(DataType.Text)]
		public string UpdatedBy
		{
			get
			{
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
		//http://hundeide.net/2015/05/optimistic-concurrency-with-mysql-and-entity-framework/
		[DataType(DataType.DateTime)]
		[Required]
		[DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm:ss}")]
		[ConcurrencyCheck]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
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
		#endregion

		#region Methods, Private
		private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
		#endregion 

	}
}

