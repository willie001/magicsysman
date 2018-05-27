#region Using
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
#endregion

namespace MagicMaids.EntityModels
{
	[Table("CleanerRosteredTeam")]
	public class CleanerRosteredTeam
	{
		#region Properties, Public
		[Key]
		[Required]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		[Column("Id")]
		public String Id
		{
			get
			{
				if (_id == null || !Helpers.IsValidGuid(_id))
				{
					_id = Guid.NewGuid().ToString();
				}

				return _id;
			}
			set
			{
				if (value != null && Helpers.IsValidGuid(value))
				{
					_id = value;
				}
			}
		}
		private String _id;

		public Boolean IsPrimary
		{
			get;
			set;
		}

		#endregion 

		#region Properties, Foreign Key
		public String RosterRefId
		{
			get;
			set;
		}

		[ForeignKey("RosterRefId")]
		public virtual CleanerRoster Roster
		{
			get;
			set;
		}

		public String TeamRefId
		{
			get;
			set;
		}

		[ForeignKey("TeamRefId")]
		public virtual CleanerTeam TeamMember
		{
			get;
			set;
		}
		#endregion 
	}
}
