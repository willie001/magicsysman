#region Using
using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Web;
using MagicMaids.EntityModels;
using NLog;
#endregion

namespace MagicMaids.DataAccess
{
    public class MagicMaidsContext : DbContext
    {
        #region Constructors
        public MagicMaidsContext() 
			: base(nameOrConnectionString: "MagicMaidsDBConn")
		{
			((IObjectContextAdapter)this).ObjectContext.CommandTimeout = 180;

			this.Configuration.LazyLoadingEnabled = false;

            DefaultSettings = Set<SystemSetting>();
            SuburbZones = Set<SuburbZone>();
			Franchises = Set<Franchise>();
			Addresses = Set<Address>();
			Rates = Set<Rate>();
			Cleaners = Set<Cleaner>();
			CleanerTeam = Set<CleanerTeam>();
			CleanerRoster = Set<CleanerRoster>();
			CleanerRosteredTeam = Set<CleanerRosteredTeam>();
			CleanerLeave = Set<CleanerLeave>();
			Clients = Set<Client>();
			ClientMethods = Set<ClientMethod>();
			ClientLeave = Set<ClientLeave>();

            Log = LogManager.GetLogger(GetType().FullName);

            Database.SetInitializer(new MagicMaidsInitialiser());
        }
        #endregion

        #region Properties, Protected
        protected Logger Log { get; private set; }

        public DbSet<SystemSetting> DefaultSettings 
        {
            get;
            set;
        }

		public DbSet<Franchise> Franchises
		{
			get;
			set;
		}

		public DbSet<Address> Addresses
		{
			get;
			set;
		}

        public DbSet<SuburbZone> SuburbZones
        {
            get;
            set;
        }

		public DbSet<Rate> Rates
		{
			get;
			set;
		}

		public DbSet<Cleaner> Cleaners
		{
			get;
			set;
		}

		public DbSet<CleanerTeam> CleanerTeam
		{
			get;
			set;
		}

		public DbSet<CleanerRoster> CleanerRoster
		{
			get;
			set;
		}

		public DbSet<CleanerRosteredTeam> CleanerRosteredTeam
		{
			get;
			set;
		}

		public DbSet<CleanerLeave> CleanerLeave
		{
			get;
			set;
		}

		public DbSet<Client> Clients
		{
			get;
			set;
		}

		public DbSet<ClientMethod> ClientMethods
		{
			get;
			set;
		}

		public DbSet<ClientLeave> ClientLeave
		{
			get;
			set;
		}
        #endregion

        #region Methods, Protected


		public override int SaveChanges()
		{
			foreach (var auditableEntity in ChangeTracker.Entries<IDataModel>())
			{
				if (auditableEntity.State == EntityState.Added || auditableEntity.State == EntityState.Modified)
				{
					string currentUser = HttpContext.Current.User.Identity.Name;
					if (String.IsNullOrWhiteSpace(currentUser))
						currentUser = "TODO";
					
					auditableEntity.Entity.UpdatedAt = DateTime.UtcNow;
					auditableEntity.Entity.RowVersion = DateTime.UtcNow;
					auditableEntity.Entity.UpdatedBy = currentUser;

					if (auditableEntity.State == EntityState.Added)
					{
						auditableEntity.Entity.CreatedAt = DateTime.UtcNow ;
					}
					else
					{
						// we also want to make sure that code is not inadvertly
						// modifying created date and created by columns 
						auditableEntity.Property(p => p.CreatedAt).IsModified = false;
					}
				}
			}

			this.Database.Log = s =>
			{
				// You can put a breakpoint here and examine s with the TextVisualizer
				// Note that only some of the s values are SQL statements
				Console.WriteLine(s);
			};
			return base.SaveChanges();
		}

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // **** USE THIS TO OVERRIDE TABLE NAMES FOR CREATION ****
            //	modelBuilder.Entity<Course>().ToTable("Course");

			modelBuilder.Entity<CleanerRoster>().HasMany(p => p.CleanerRosteredTeam)
					.WithRequired(pc => pc.Roster)
			  		.HasForeignKey(pc => pc.RosterRefId);

			modelBuilder.Entity<CleanerTeam>().HasMany(p => p.CleanerRosteredTeam)
					.WithRequired(pc => pc.TeamMember)
			  		.HasForeignKey(pc => pc.TeamRefId);

            base.OnModelCreating(modelBuilder);
        }
        #endregion

    }
}
