using System;
using System.Collections.Generic;

namespace MagicMaids.EntityModels
{
	public interface IDataModel
	{
		Guid Id 
		{ 
			get;
			set;
		}

		string UpdatedBy
		{
			get;
			set;
		}

		DateTime CreatedAt
		{
			get;
			set;
		}

		DateTime UpdatedAt
		{
			get;
			set;
		}

		DateTime RowVersion
		{
			get;
			set;
		}

		bool IsActive
		{
			get;
			set;
		}
	}
}
