#region Using
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using FluentValidation;
using FluentValidation.Attributes;
#endregion

//https://github.com/JeremySkinner/FluentValidation/wiki/h.-ASP.NET-MVC-5-integration

namespace MagicMaids.EntityModels
{
    [Validator(typeof(SettingsValidator))]
    [Table("SystemSettings")]
	public class SystemSetting : BaseModel
	{
		#region Properties, Public
		public String SettingName
		{
			get;
			set;
		}

		public String SettingValue
		{
			get;
			set;
		}

		public String CodeIdentifier
		{
			get;
			set;
		}

		#endregion
	}

    //https://github.com/JeremySkinner/FluentValidation/wiki/c.-Built-In-Validators
    public class SettingsValidator : AbstractValidator<SystemSetting>
    {
        public SettingsValidator()
        {
            RuleFor(x => x.Id).NotNull();

            RuleFor(x => x.SettingName).NotEmpty().WithMessage("Default setting name is required.") ;
            RuleFor(x => x.SettingValue).NotEmpty().WithMessage("Default value is required.");
            RuleFor(x => x.CodeIdentifier).NotEmpty().WithMessage("The Code Identifier is required.");

            RuleFor(x => x.SettingName).Length(5, 25).WithName("Default name");
            RuleFor(x => x.SettingValue).Length(1, 50).WithName("Default value");
        }
    }
}


