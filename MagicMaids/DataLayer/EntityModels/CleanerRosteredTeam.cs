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
		public Guid Id
		{
			get
			{
				if (!_id.HasValue)
				{
					_id = Guid.NewGuid();
				}
				return _id.Value;
			}
			set
			{
				if (value != _id)
				{
					_id = value;
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
		public Guid RosterRefId
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

		public Guid TeamRefId
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
