using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(MagicMaids.Startup))]

namespace MagicMaids
{
	public partial class Startup
	{
		// The OWIN middleware will invoke this method when the app starts
		public void Configuration(IAppBuilder app)
		{
			// ConfigureAuth defined in other part of the class
			ConfigureAuth(app);
		}
	}
}
