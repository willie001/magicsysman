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
				if (!Helpers.IsValidGuid(_id.ToString()))
				{
					_id = Guid.NewGuid();
				}

				return _id.ToString();
			}
			set
			{
				if (value != _id.ToString() && Helpers.IsValidGuid(value))
				{
					_id = new Guid(value);
				}
			}
		}
		private Guid? _id;

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
