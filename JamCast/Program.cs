using JamCast;
using JamCast.Services;
using Protoinject;

namespace JamCast
{
    public static class Program
    {
        internal static void Main(string[] args)
        {
            var kernel = new StandardKernel();

            kernel.Bind<ISiteInfoService>().To<SiteInfoService>().InSingletonScope();
            kernel.Bind<IComputerInfoService>().To<ComputerInfoService>().InSingletonScope();
            kernel.Bind<IMacAddressReportingService>().To<MacAddressReportingService>().InSingletonScope();
            kernel.Bind<IUserInfoService>().To<UserInfoService>().InSingletonScope();
            kernel.Bind<IImageService>().To<ImageService>().InSingletonScope();
            kernel.Bind<IJamHostApiService>().To<JamHostApiService>().InSingletonScope();
            kernel.Bind<IManager>().To<Manager>().InSingletonScope();
            kernel.Bind<IAuthenticator>().To<Authenticator>().InSingletonScope();

            var authenticator = kernel.Get<IAuthenticator>();
            authenticator.EnsureAuthenticated();

            var manager = kernel.Get<IManager>();
            manager.Run();
        }
    }
}

