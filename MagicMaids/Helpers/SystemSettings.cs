using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using FluentValidation.Mvc;
using LazyCache;
using MagicMaids.DataAccess;

namespace MagicMaids
{
	public static class SystemSettings
	{
		#region Enums
		public enum SettingEnum
		{
			MANAGE_FEE_PERC,
			ROYALTY_FEE_PERC,
			GAP_MINS_SAMEZONE,
			GAP_MINS_SECZONE,
			GAP_MINS_OTHER,
			MAX_HRS_WORK_SESSION,
			MAX_DAYS_BOOKING
		}
		#endregion

		#region Properties, Public
		public static decimal ManagementFeePerc
		{
			get
			{
				decimal _val = 0;

				if (Settings.ContainsKey(SettingEnum.MANAGE_FEE_PERC))
				{
					decimal.TryParse(Settings[SettingEnum.MANAGE_FEE_PERC].ToString(), out _val);
				}

				return _val;
			}
			set
			{
				SetSingleProperty(SettingEnum.MANAGE_FEE_PERC, value.ToString());
			}
		}

		public static decimal RoyaltyFeePerc
		{
			get
			{
				decimal _val = 0;

				if (Settings.ContainsKey(SettingEnum.ROYALTY_FEE_PERC))
				{
					decimal.TryParse(Settings[SettingEnum.ROYALTY_FEE_PERC].ToString(), out _val);
				}

				return _val;
			}
			set
			{
				SetSingleProperty(SettingEnum.ROYALTY_FEE_PERC, value.ToString());
			}
		}

		public static Int32 GapSecondaryZoneMinutes
		{
			get
			{
				Int32 _val = 0;

				if (Settings.ContainsKey(SettingEnum.GAP_MINS_SECZONE))
				{
					Int32.TryParse(Settings[SettingEnum.GAP_MINS_SECZONE].ToString(), out _val);
				}

				return _val;
			}
			set
			{
				SetSingleProperty(SettingEnum.GAP_MINS_SECZONE, value.ToString());
			}
		}

		public static Int32 GapSameZoneMinutes
		{
			get
			{
				Int32 _val = 0;

				if (Settings.ContainsKey(SettingEnum.GAP_MINS_SAMEZONE))
				{
					Int32.TryParse(Settings[SettingEnum.GAP_MINS_SAMEZONE].ToString(), out _val);
				}

				return _val;
			}
			set
			{
				SetSingleProperty(SettingEnum.GAP_MINS_SAMEZONE, value.ToString());
			}
		}

		public static Int32 GapOtherZoneMinutes
		{
			get
			{
				Int32 _val = 0;

				if (Settings.ContainsKey(SettingEnum.GAP_MINS_OTHER))
				{
					Int32.TryParse(Settings[SettingEnum.GAP_MINS_OTHER].ToString(), out _val);
				}

				return _val;
			}
			set
			{
				SetSingleProperty(SettingEnum.GAP_MINS_OTHER, value.ToString());
			}
		}

		public static decimal WorkSessionMaxHours
		{
			get
			{
				decimal _val = 0;

				if (Settings.ContainsKey(SettingEnum.MAX_HRS_WORK_SESSION))
				{
					decimal.TryParse(Settings[SettingEnum.MAX_HRS_WORK_SESSION].ToString(), out _val);
				}

				return _val;
			}
			set
			{
				SetSingleProperty(SettingEnum.MAX_HRS_WORK_SESSION, value.ToString());
			}
		}

		public static Int32 BookingsDaysAllowed
		{
			get
			{
				Int32 _val = 0;

				if (Settings.ContainsKey(SettingEnum.MAX_DAYS_BOOKING))
				{
					Int32.TryParse(Settings[SettingEnum.MAX_DAYS_BOOKING].ToString(), out _val);
				}

				return _val;
			}
			set
			{
				SetSingleProperty(SettingEnum.MAX_DAYS_BOOKING, value.ToString());
			}
		}

		#endregion

		#region Properties, Private
		private static Dictionary<SettingEnum, object> Settings
		{
			get
			{
				if (_systemSettings == null)
				{
					_systemSettings = new Dictionary<SettingEnum, object>();
					Init();
				}

				return _systemSettings;
			}

		}
		private static Dictionary<SettingEnum, object> _systemSettings;
		#endregion

		#region Methods, Public
		public static void Reset()
		{
			Init();
		}
		#endregion

		#region Methods, Private
		private static void Init()
		{
			Settings.Clear();

			using (DBManager db = new DBManager())
			{
				List<EntityModels.SystemSetting> _settings = (System.Collections.Generic.List<MagicMaids.EntityModels.SystemSetting>)db.getConnection().GetList<EntityModels.SystemSetting>();

				foreach (EntityModels.SystemSetting _setting in _settings)
				{
					SettingEnum _enum;
					if (Enum.TryParse(_setting.CodeIdentifier.Trim(), true, out _enum)) 
					{
						Settings.Add(_enum, _setting.SettingValue);
					}
				}
			}

			FluentValidationModelValidatorProvider.Configure();


			if (Settings.Count == 0)
			{
				throw new ApplicationException("Error loading global system settings.");
			}
		}

		private static void SetSingleProperty(SettingEnum setting, String value)
		{
			if (Settings.ContainsKey(setting))
			{
				Settings[setting] = value;
			}
			else
			{
				Settings.Add(setting, value);
			}
		}

		#endregion 
	}
}
