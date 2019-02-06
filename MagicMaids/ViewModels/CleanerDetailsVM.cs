#region Using
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentValidation.Attributes;

using MagicMaids.EntityModels;
using MagicMaids.Validators;
#endregion

namespace MagicMaids.ViewModels
{
    [Validator(typeof(CleanerDetailsValidator))]
    public class CleanerDetailsVM : BaseContactVM, IAddressViewModel
    {
        //Primary Cleaner's container view model
        #region Properties, Public
        public Boolean IsNewItem
        {
            get;
            set;
        }

        public Boolean IsActive
        {
            get;
            set;
        }

        public String Id
        {
            get;
            set;
        }

        public String MasterFranchiseRefId
        {
            get;
            set;
        }

        public Int32 CleanerCode
        {
            get;
            set;
        }

        public string Initials
        {
            get;
            set;
        }

        public string FirstName
        {
            get;
            set;
        }

        public string LastName
        {
            get;
            set;
        }

        public string Region
        {
            get;
            set;
        }

        public string PrimaryZone
        {
            get;
            set;
        }

        public string SecondaryZone
        {
            get;
            set;
        }

        public string ApprovedZone
        {
            get;
            set;
        }

        public List<String> PrimaryZoneList
        {
            get;
            set;
        }

        public List<String> SecondaryZoneList
        {
            get;
            set;
        }

        public List<String> ApprovedZoneList
        {
            get;
            set;
        }

        public Int32? Rating
        {
            get;
            set;
        }

        public String GenderFlag
        {
            get;
            set;
        }

        public Boolean Ironing
        {
            get;
            set;
        }

        public List<TeamMemberDetailsVM> TeamMembers
        {
            get;
            set;
        }

        #endregion

        #region Methods, Public
        public void PopulateVM(Cleaner entityModel)
        {
            if (entityModel == null)
                return;

            Id = entityModel.Id;
            CleanerCode = entityModel.CleanerCode;
            Initials = entityModel.Initials;
            FirstName = entityModel.FirstName;
            LastName = entityModel.LastName;
            EmailAddress = entityModel.EmailAddress;
            BusinessPhoneNumber = entityModel.BusinessPhoneNumber;
            OtherNumber = entityModel.OtherNumber;
            MobileNumber = entityModel.MobileNumber;
            Region = entityModel.Region;
            Rating = entityModel.Rating;
            MasterFranchiseRefId = (Helpers.IsValidGuid(entityModel.MasterFranchiseRefId)) ? entityModel.MasterFranchiseRefId.ToString() : "";

            IsActive = entityModel.IsActive;
            Ironing = entityModel.Ironing;
            GenderFlag = entityModel.GenderFlag;
            PrimaryZone = entityModel.PrimaryZone;
            SecondaryZone = entityModel.SecondaryZone;
            ApprovedZone = entityModel.ApprovedZone;

            if (!String.IsNullOrWhiteSpace(this.PrimaryZone))
            {
                PrimaryZoneList = this.PrimaryZone.Split(new char[] { ',', ';' })
                    .Distinct()
                    .ToList();
            };

            if (!String.IsNullOrWhiteSpace(this.SecondaryZone))
            {
                SecondaryZoneList = this.SecondaryZone.Split(new char[] { ',', ';' })
                .Distinct()
                .ToList();
            }

            if (!String.IsNullOrWhiteSpace(this.ApprovedZone))
            {
                ApprovedZoneList = this.ApprovedZone.Split(new char[] { ',', ';' })
                .Distinct()
                .ToList();
            }
            base.FormatContactDetails(entityModel.PhysicalAddress);
        }
        #endregion
    }

    [Validator(typeof(TeamMemberDetailsValidator))]
    public class TeamMemberDetailsVM : BaseContactVM
    {
        //Team members' container view model
        #region Properties, Public
        public Boolean IsNewItem
        {
            get;
            set;
        }

        public Boolean IsActive
        {
            get;
            set;
        }

        public String Id
        {
            get;
            set;
        }

        public String PrimaryCleanerRefId
        {
            get;
            set;
        }

        public string FirstName
        {
            get;
            set;
        }

        public string LastName
        {
            get;
            set;
        }

        public String GenderFlag
        {
            get;
            set;
        }

        public Boolean Ironing
        {
            get;
            set;
        }

        #endregion

        #region Methods, Public
        public void PopulateVM(Guid? cleanerId, CleanerTeam entityModel)
        {
            if (entityModel == null)
                return;

            if (cleanerId.Equals(Guid.Empty))
                return;

            Id = entityModel.Id;
            FirstName = entityModel.FirstName;
            LastName = entityModel.LastName;
            EmailAddress = entityModel.EmailAddress;
            MobileNumber = entityModel.MobileNumber;
            IsActive = entityModel.IsActive;
            Ironing = entityModel.Ironing;
            GenderFlag = entityModel.GenderFlag;
            PrimaryCleanerRefId = (Helpers.IsValidGuid(cleanerId)) ? cleanerId.Value.ToString() : "";

            base.FormatContactDetails(entityModel.PhysicalAddress);
        }
        #endregion
    }

    public class CleanerSearchVM : BaseViewModel
    {
        #region Properties, Public
        public String SelectedFranchiseId
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public string Zone
        {
            get;
            set;
        }

        public int Rating
        {
            get;
            set;
        }

        public Boolean IncludeInactive
        {
            get;
            set;
        }
        #endregion
    }

    public class CleanerMatchResultVM : BaseContactVM
    {
        #region Properties, Public
        public String Id
        {
            get;
            set;
        }

        public string Initials
        {
            get;
            set;
        }

        public string FirstName
        {
            get;
            set;
        }

        public string LastName
        {
            get;
            set;
        }

        public string PrimaryZone
        {
            get;
            set;
        }

        public string SecondaryZone
        {
            get;
            set;
        }

        public string ApprovedZone
        {
            get;
            set;
        }


        public List<String> PrimaryZoneList
        {
            get;
            set;
        }

        public List<String> SecondaryZoneList
        {
            get;
            set;
        }

        public List<String> ApprovedZoneList
        {
            get;
            set;
        }

        public Boolean IsFirstJob
        {
            get
            {
                if (String.IsNullOrWhiteSpace(PreviousJobLocation))
                {
                    return true;
                }
                return false;
            }
        }

        public String StylePreviousJobLocation
        {
            get;
            set;
        }

        public String PreviousJobLocation
        {
            get;
            set;
        }

        public String StyleNextJobLocation
        {
            get;
            set;
        }

        public String NextJobLocation
        {
            get;
            set;
        }

        public String StyleHomeBase
        {
            get;
            set;
        }

        public String StyleWeekday
        {
            get;
            set;
        }

        public String StyleWeekdayNextWeek
        {
            get;
            set;
        }

        public String DisplayHomeBase
        {
            get;
            set;
        }

        public Int32 Rating
        {
            get;
            set;
        }

        public Int32 TeamSize
        {
            get;
            set;
        }

        public IList<CleanerRosterVM> CleanerRosters
        {
            get;
            set;
        }

        public DayOfWeek SelectedRosterDay
        {
            get;
            set;
        }

        public string DisplaySelectedRosterDay
        {
            get
            {
                return SelectedRosterDay.ToString();
            }
        }

        public DateTime? SelectedServiceDate
        {
            get;
            set;            
           
        }

        public DateTime? SelectedServiceDateNextWeek
        {
            get
            {
                return SelectedServiceDate.Value.AddDays(7);
            }

        }

        public String DisplayServiceDate
        {
            get
            {
                return SelectedServiceDate.Value.ToString("dd MMM yyyy");
            }
        }

        public String DisplayServiceDateNextWeek
        {
            get
            {
                return SelectedServiceDateNextWeek.Value.ToString("dd MMM yyyy");
            }
        }

        public String CustomErrorMessage
        {
            get;
            set;
        }

        public IList<JobBookingsVM> ScheduledJobs
        {
            get;
            set;
        }

        public IList<JobBookingsVM> ScheduledJobsNextWeek
        {
            get;
            set;
        }

        public Int32 SheduledClashCount
        {
            get
            {
                return ClashingJobsForServiceDay.Count;
            }
        }

        public IList<JobBookingsVM> ClashingJobsForServiceDay
        {
            get
            {
                if (!SelectedServiceDate.Equals(default(DateTime))
                    && ScheduledJobs != null)
                {
                    return ScheduledJobs.Where<JobBookingsVM>(x =>
                       x.WeekDay.ToLower() == SelectedRosterDay.ToString().ToLower()
                       && x.JobDateUTC.Value.ToUser().Date == SelectedServiceDate.Value.Date
                       && (x.JobStatus == BookingStatus.CONFIRMED || x.JobStatus == BookingStatus.PENDING)
                       && (x.JobType == JobTypeEnum.OneOff || x.JobType == JobTypeEnum.Vacate)
                    )
                    .ToList();
                }

                return new List<JobBookingsVM>();
            }
        }

        public IList<JobBookingsVM> ScheduledJobsForServiceDay
        {
            get
            {
                if (!SelectedServiceDate.Equals(default(DateTime)) && ScheduledJobs != null)
                {
                    var list = ScheduledJobs.Where<JobBookingsVM>(x =>
                       x.WeekDay.ToLower() == SelectedRosterDay.ToString().ToLower()
                       && (
                           ((x.JobType == JobTypeEnum.OneOff || x.JobType == JobTypeEnum.Vacate) &&
                               x.JobDateUTC.Value.ToUser().Date.Equals(SelectedServiceDate.Value.Date))
                           ||
                           (x.JobType == JobTypeEnum.Weekly || x.JobType == JobTypeEnum.Fortnighly)
                       )
                    )
                    .ToList();

                    return list;
                }

                return new List<JobBookingsVM>();
            }
        }

        public IList<JobBookingsVM> ScheduledJobsForServiceDayNextWeek
        {
            get
            {
                if (!SelectedServiceDateNextWeek.Equals(default(DateTime)) && ScheduledJobsNextWeek != null)
                {
                    var list = ScheduledJobsNextWeek.Where<JobBookingsVM>(x =>
                       x.WeekDay.ToLower() == SelectedRosterDay.ToString().ToLower()
                       && (
                           ((x.JobType == JobTypeEnum.OneOff || x.JobType == JobTypeEnum.Vacate) &&
                               x.JobDateUTC.Value.AddDays(7).ToUser().Date.Equals(SelectedServiceDate.Value.AddDays(7).Date))
                           ||
                           (x.JobType == JobTypeEnum.Weekly || x.JobType == JobTypeEnum.Fortnighly)
                       )
                    )
                    .ToList();

                    return list;
                }

                return new List<JobBookingsVM>();
            }
        }

        public bool CleanerOnLeave
        {
            get;
            set;
        }

        public string LeaveDates
        {
            get;
            set;
        }
        #endregion
    }

    [Validator(typeof(CleanerRosterValidator))]
    public class CleanerRosterVM : BaseViewModel
    {
        //Roster view model per day
        #region Properties, Public
        public string Id
        {
            get;
            set;
        }

        public String Weekday
        {
            get;
            set;
        }

        public Int32 TeamCount
        {
            get;
            set;
        }

        public DateTime StartTime
        {
            get;
            set;
        }

        public DateTime EndTime
        {
            get;
            set;
        }

        public long TimeOfDayFrom
        {
            get;
            set;
        }

        public long TimeOfDayTo
        {
            get;
            set;
        }

        public Boolean IsActive
        {
            get;
            set;
        }

        public List<RosterTeamMembersVM> TeamMembers
        {
            get;
            set;
        }


        #endregion

        #region Methods, Public
        public void PopulateVM(Guid? cleanerId, RosterTeamMembersVM entityModel)
        {
            if (entityModel == null)
                return;

            if (cleanerId.Equals(Guid.Empty))
                return;

            Id = (Helpers.IsValidGuid(entityModel.Id)) ? entityModel.Id : "";

            Weekday = entityModel.Weekday;
            TeamCount = entityModel.TeamCount;
            TimeOfDayFrom = entityModel.TimeOfDayFrom;
            TimeOfDayTo = entityModel.TimeOfDayTo;
            StartTime = entityModel.TimeOfDayFrom.ToTime();
            EndTime = entityModel.TimeOfDayTo.ToTime();
            IsActive = true;

            TeamMembers = new List<RosterTeamMembersVM>();
            // add initial member to collection - controller will take it a step further
            // to add teammembers to existing roster items
            if (entityModel != null)
            {
                TeamMembers.Add(entityModel);
            }
        }

        public static List<CleanerRosterVM> PopulateCollection(Guid? cleanerId, List<RosterTeamMembersVM> results)
        {
            Dictionary<String, CleanerRosterVM> parsedTeamList = new Dictionary<String, CleanerRosterVM>();
            if (results == null || results.Count() == 0)
                return new List<CleanerRosterVM>();

            if (cleanerId.Equals(Guid.Empty))
                return new List<CleanerRosterVM>();

            foreach (var item in results)
            {
                var weekDay = item.Weekday.ToUpper();
                if (parsedTeamList.ContainsKey(weekDay))
                {
                    CleanerRosterVM _updateEntry = parsedTeamList[weekDay];
                    _updateEntry.TeamMembers.Add(item);
                    parsedTeamList[weekDay] = _updateEntry;
                }
                else
                {
                    var _newEntry = new CleanerRosterVM();
                    _newEntry.PopulateVM(cleanerId, item);
                    parsedTeamList.Add(weekDay, _newEntry);
                }
            }

            return parsedTeamList.Values.ToList();
        }
        #endregion
    }

    public class RosterTeamMembersVM : BaseViewModel
    {
        //Team member's rostered view model
        public String Id
        {
            get;
            set;
        }

        public String RosterId
        {
            get;
            set;
        }

        public Boolean IsPrimary
        {
            get;
            set;
        }

        public String FirstName
        {
            get;
            set;
        }

        public String LastName
        {
            get;
            set;
        }

        public String DisplayName
        {
            get
            {
                return $"{FirstName} {LastName}";
            }
        }

        public String Weekday
        {
            get;
            set;
        }

        public Int32 TeamCount
        {
            get;
            set;
        }

        public long TimeOfDayFrom
        {
            get;
            set;
        }

        public long TimeOfDayTo
        {
            get;
            set;
        }
    }

    [Validator(typeof(CleanerLeaveValidator))]
    public class CleanerLeaveVM : BaseViewModel
    {
        //Cleaner leave container view model
        #region Properties, Public
        public Boolean IsNewItem
        {
            get;
            set;
        }

        public String Id
        {
            get;
            set;
        }

        public String PrimaryCleanerRefId
        {
            get;
            set;
        }

        public DateTime StartDate
        {
            get
            {
                return _startDate;
            }
            set
            {
                _startDate = value;
            }
        }

        public String StartDateFormatted
        {
            get
            {
                if (_startDate == null || _startDate.Equals(DateTime.MinValue) || _startDate.Equals(DateTime.MaxValue))
                {
                    return String.Empty;
                }

                return _startDate.FormatUserDate();
            }
        }
        private DateTime _startDate;


        public DateTime EndDate
        {
            get
            {
                return _endDate;
            }
            set
            {
                _endDate = value;
            }
        }

        public String EndDateFormatted
        {
            get
            {
                if (_endDate == null || _endDate.Equals(DateTime.MinValue) || _endDate.Equals(DateTime.MaxValue))
                {
                    return String.Empty;
                }

                return _endDate.FormatUserDate();
            }
        }
        private DateTime _endDate;
        #endregion

        #region Methods, Public
        public void PopulateVM(Guid? cleanerId, CleanerLeave entityModel)
        {
            if (entityModel == null)
                return;

            if (cleanerId.Equals(Guid.Empty))
                return;

            Id = entityModel.Id;
            PrimaryCleanerRefId = (Helpers.IsValidGuid(cleanerId)) ? cleanerId.Value.ToString() : "";

            StartDate = entityModel.StartDate.ToUser();
            EndDate = entityModel.EndDate.ToUser();
        }
        #endregion
    }


}
