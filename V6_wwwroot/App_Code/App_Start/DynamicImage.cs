using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using SoundInTheory.DynamicImage;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(ASP.App_Start.DynamicImage), "PreStart")]

namespace ASP.App_Start
{
	public static class DynamicImage
	{
		public static void PreStart()
		{
			DynamicModuleUtility.RegisterModule(typeof(DynamicImageModule));
		}
	}
}