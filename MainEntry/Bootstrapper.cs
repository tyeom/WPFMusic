using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Services;
using System;
using ViewModels;
using ViewModels.MainSettingViewModels;
using Views.Windows;

namespace MainEntry;

public class Bootstrapper
{
    public Bootstrapper()
    {
        var services = ConfigureServices();
        Ioc.Default.ConfigureServices(services);
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Services
        // 같은 Popup Window 여러개 띄우는 경우가 있을 수 있기 때문에 서비스 수명주기를 AddTransient 사용
        services.AddTransient<IDialogService, DialogService>();
        // 같은 Popup Window 여러개 띄우는 경우가 있을 수 있기 때문에 서비스 수명주기를 AddTransient 사용
        services.AddTransient<IDialog, PopupWindow>();
        
        services.AddSingleton<ISettingService, SettingService>();
        services.AddSingleton<IBassService, BassService>();


        // Viewmodels
        services.AddTransient<ShellViewModel>();
        services.AddTransient<MainViewModel>();

        // 환경설정 - 일반
        services.AddTransient<GeneralViewModel>();
        // 환경설정 - 재생
        services.AddTransient<PlayViewModel>();

        services.AddTransient<AlbumArtInfoViewModel>();
        services.AddTransient<ControlPanelViewModel>();

        return services.BuildServiceProvider();
    }
}
