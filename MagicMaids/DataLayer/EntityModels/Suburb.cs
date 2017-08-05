#region Using
using System;
using System.Collections.Generic;
using System.Linq;
#endregion 

namespace MagicMaids.EntityModels
{
	public class Suburb: BaseModel 
	{
		#region Fields
		private IList<Int32> linkedZones;
		#endregion

		#region Constructor
		public Suburb()
		{
		}

		public Suburb(string connectedZones)
		{
			if (!String.IsNullOrWhiteSpace(connectedZones))
					linkedZones = connectedZones.Split(',')
									.ToList<string>()
									.ConvertAll<int>(new Converter<string, int>(s => int.Parse(s)));
		}
		#endregion 


		#region Property, Public
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

		public Int32 ZoneID
		{
			get;
			set;
		}

		public IList<Int32> LinkedZones
		{
			get
			{
				if (linkedZones == null)
					linkedZones = new List<Int32>();

				return linkedZones;
			}
		}

		#endregion
	}
}
