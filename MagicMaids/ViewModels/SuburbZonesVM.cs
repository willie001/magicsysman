#region Using
using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Attributes;

using MagicMaids.EntityModels;
using MagicMaids.Validators;
#endregion

namespace MagicMaids.ViewModels
{
	[Validator(typeof(SuburbZoneValidator))]
	public class UpdateSuburbZonesVM
	{
		#region Properties, Public
		public Boolean IsNewItem
		{
			get;
			set;
		}

		public Guid Id
		{
			get;
			set;
		}

		public string SuburbName
		{
			get;
			set;
		}

		public string PostCode
		{
			get;
			set;
		}

		public string Zone
		{
			get;
			set;
		}

		public string LinkedZones
		{
			get;
			set;
		}

		public List<string> LinkedZoneList
		{
			get
			{
				if (String.IsNullOrWhiteSpace(LinkedZones))
				{
					return new List<string>();
				}

				return LinkedZones.Split(',')
								  .ToList<string>();
									//.ConvertAll<int>(new Converter<string, int>(s => int.Parse(s)));
			}
		}

		public Guid? FranchiseId
		{
			get;
			set;
		}
		#endregion

		#region Methods, Public
		public void PopulateVM(SuburbZone entityModel)
		{
			if (entityModel == null)
				return;

			this.Id = entityModel.Id;
			this.SuburbName = entityModel.SuburbName;
			this.PostCode = entityModel.PostCode;
			this.Zone = entityModel.Zone;
			this.LinkedZones = entityModel.LinkedZones;
			this.FranchiseId = entityModel.FranchiseId.HasValue ? entityModel.FranchiseId : null;
		}
		#endregion
	}
}
