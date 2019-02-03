using AutoMapper;
using AutoMapper.Configuration;
using MagicMaids.EntityModels;
using MagicMaids.ViewModels;

namespace MagicMaids
{
	public class AutoMapperConfigurator
	{
		public MapperConfigurationExpression GetConfiguration()
		{
			MapperConfigurationExpression cfg = new MapperConfigurationExpression();

			cfg.ShouldMapProperty = p =>
				p.GetMethod.IsPublic ||   // Public of course means a public property.
				p.GetMethod.IsFamily ||   // Family means a protected property.
				p.GetMethod.IsAssembly || // Assembly means an internal property.
				p.GetMethod.IsPrivate;    // Private of course means a private property.

			cfg.CreateMap<Cleaner, CleanerDetailsVM>()
				.ForMember(dest => dest.ApprovedZoneList, source => source.Ignore())
				.ForMember(dest => dest.IsNewItem, source => source.Ignore())
				.ForMember(dest => dest.PrimaryZoneList, source => source.Ignore())
				.ForMember(dest => dest.ApprovedZoneList, source => source.Ignore())
				.ForMember(dest => dest.TeamMembers, source => source.Ignore())
				.ForMember(dest => dest.SqlString, source => source.Ignore())
				.ForMember(dest => dest.FormattedContactNumbers, source => source.Ignore())
				.ForMember(dest => dest.SecondaryZoneList, source => source.Ignore())
				.ReverseMap();

			cfg.CreateMap<Address, UpdateAddressViewModel>()
				.ForMember(dest => dest.IsNewItem, source => source.Ignore())
				.ForMember(dest => dest.FormattedAddress, source => source.Ignore())
				.ForMember(dest => dest.SqlString, source => source.Ignore())
				.ForMember(dest => dest.DebugData, source => source.Ignore())
				.ReverseMap();

			cfg.CreateMap<Client, ClientDetailsVM>()
				.ForMember(dest => dest.IsNewItem, source => source.Ignore())
				.ForMember(dest => dest.FormattedContactNumbers, source => source.Ignore())
				.ForMember(dest => dest.SqlString, source => source.Ignore())
				.ReverseMap();

			cfg.CreateMap<Cleaner, CleanerMatchResultVM>()
				.ForMember(dest => dest.FormattedContactNumbers, source => source.Ignore())
				.ForMember(dest => dest.SqlString, source => source.Ignore())
				.ForMember(dest => dest.PrimaryZoneList, source => source.Ignore())
				.ForMember(dest => dest.SecondaryZoneList, source => source.Ignore())
				.ForMember(dest => dest.ApprovedZoneList, source => source.Ignore())

				.ForMember(dest => dest.DisplayHomeBase, source => source.Ignore())
				.ForMember(dest => dest.StyleHomeBase, source => source.Ignore())
				.ForMember(dest => dest.StyleWeekday, source => source.Ignore())
                .ForMember(dest => dest.StyleWeekdayNextWeek, source => source.Ignore())
                .ForMember(dest => dest.StylePreviousJobLocation, source => source.Ignore())
				.ForMember(dest => dest.PreviousJobLocation, source => source.Ignore())
				.ForMember(dest => dest.StyleNextJobLocation, source => source.Ignore())
				.ForMember(dest => dest.NextJobLocation, source => source.Ignore())
				.ForMember(dest => dest.SelectedServiceDate, source => source.Ignore())

				.ForMember(dest => dest.TeamSize, source => source.Ignore())
				.ForMember(dest => dest.SelectedRosterDay, source => source.Ignore())
				.ForMember(dest => dest.CleanerRosters, source => source.Ignore())
				.ForMember(dest => dest.ScheduledJobs, source => source.Ignore())
                .ForMember(dest => dest.ScheduledJobsNextWeek, source => source.Ignore())
                .ForMember(dest => dest.ScheduledJobsForServiceDayNextWeek, source => source.Ignore())                

                .ForMember(dest => dest.CustomErrorMessage, source => source.Ignore())

				.ForMember(dest => dest.CleanerOnLeave, source => source.Ignore())
				.ForMember(dest => dest.LeaveDates, source => source.Ignore())

				.ReverseMap();

			cfg.CreateMap<CleanerRoster, CleanerRosterVM>()
				.ForMember(dest => dest.StartTime, source => source.Ignore())
				.ForMember(dest => dest.EndTime, source => source.Ignore())
				.ForMember(dest => dest.Weekday, source => source.MapFrom(src => src.Weekday))
				.ForMember(dest => dest.TimeOfDayFrom, source => source.MapFrom(src => src.TimeOfDayFrom))
				.ForMember(dest => dest.TimeOfDayTo, source => source.MapFrom(src => src.TimeOfDayTo))
				.ForMember(dest => dest.TeamMembers, source => source.Ignore())
				.ForMember(dest => dest.DebugData, source => source.Ignore())
				.ForMember(dest => dest.SqlString, source => source.Ignore())
				.ReverseMap();

			return cfg;
		}
	}
}
