using ArpSpoofer.ServiceContracts;
using ArpSpoofer.Services;
using ArpSpoofer.WindowContracts;
using ArpSpoofer.Windows;
using Ninject;
using System.Windows;

namespace ArpSpoofer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            IKernel kernel = new StandardKernel();
            //services
            kernel.Bind<IRegistryService>().To<RegistryService>().InSingletonScope();
            kernel.Bind<IWifiDeviceService>().To<WifiDeviceService>().InSingletonScope();
            kernel.Bind<IARPSpoofingService>().To<ARPSpoofingService>().InSingletonScope();
            kernel.Bind<ICapturingService>().To<CapturingService>().InSingletonScope();
            
            //windows
            kernel.Bind<IArpSpoof>().To<ArpSpoof>().InSingletonScope();
            kernel.Bind<ICapturePackets>().To<CapturePackets>().InSingletonScope();
            kernel.Bind<IMainWindow>().To<MainWindow>().InSingletonScope();
            var mainWindow = kernel.Get<IMainWindow>();
            mainWindow.Show();
            base.OnStartup(e);
        }
    }
}
