#region Using
using Microsoft.Owin;

using Owin;
#endregion

[assembly: OwinStartupAttribute(typeof(MagicMaids.Startup))]
namespace MagicMaids
{
	public partial class Startup
	{
		public void Configuration(IAppBuilder app)
		{
			//ConfigureAuth(app);
		}
	}
}
