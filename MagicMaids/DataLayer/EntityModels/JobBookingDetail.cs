#region Using
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
#endregion

namespace MagicMaids.EntityModels
{
    [Table("JobBookingDetail")]
    public class JobBookingDetail : BaseModel
    {
        #region Properties, Public
        

        public DateTime JobDate //UTC
        {
            get;
            set;
        }
       

        [Required]
        public long StartTime
        {
            get
            {
                return _startTime;
            }
            set
            {
                var convertedValue = value;
                if (convertedValue != _startTime)
                {
                    _startTime = convertedValue;
                }
            }
        }
        private long _startTime;

        [Required]
        public long EndTime
        {
            get
            {
                return _endTime;
            }
            set
            {
                var convertedValue = value;
                {
                    _endTime = convertedValue;
                }
            }
        }
        private long _endTime;

        
        #endregion

        #region Properties, Foreign Key

        public String JobBookingRefId
        {
            get;
            set;
        }

        [ForeignKey("JobBookingRefId")]
        public Cleaner JobBooking
        {
            get;
            set;
        }
        
        #endregion
    }
}
