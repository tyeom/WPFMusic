using Common.Helper;
using LogHelper;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using System;
using System.Windows;
using System.Windows.Media;
using ViewModels;

namespace MainEntry;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private static readonly Lazy<ILog> _log = new Lazy<ILog>(() => LogHelper.LogHelper.Log(LogHelperType.Default));
    private const string _APPLICATION_NAME_ = "WPFMusic";
    private static readonly string _title = "WPF Music";

    public App()
    {
        this.LogHelperConfigSetting();
        new Bootstrapper();

        this.Dispatcher.UnhandledException += this.Dispatcher_UnhandledException;
        this.Dispatcher.UnhandledExceptionFilter += this.Dispatcher_UnhandledExceptionFilter;
    }

    /// <summary>
    /// 프로그램 타이틀
    /// </summary>
    public static string ProductTitle
    {
        get { return _title; }
    }

    public static ILog Log
    {
        get { return _log.Value; }
    }

    #region Application 재 구현

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        Current.ShutdownMode = ShutdownMode.OnLastWindowClose;

        // 중복 실행 됨.
        if (this.CheckDuplicateProcess() is true)
        {
            MessageBox.Show("이미 실행중 입니다.");
        }
        else
        {
            // TODO : 사전 체크 로직
        }

        ShellViewModel shellViewModel = new ShellViewModel();
        shellViewModel.CurrentDataContext = Ioc.Default.GetService<MainViewModel>();

        var shellWindow = new WPFMusicShell();
        shellWindow.DataContext = shellViewModel;
        shellWindow.ShowDialog();

        // ShellViewModel 정리
        shellViewModel.Cleanup();

        App.Log.Write("Program Shutdown");
        if (Current != null)
            Current.Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);

        Log.Dispose();
    }

    #endregion Application 재 구현

    #region Methods
    private bool CheckDuplicateProcess()
    {
        bool result = false;

        // 프로세스 중복 체크
        // 이미 실행 중이라면 해당 Window Activate 처리
        if (ProcessChecker.Do(_APPLICATION_NAME_))
        {
            result = true;

            string processName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            int currentProcess = System.Diagnostics.Process.GetCurrentProcess().Id;
            System.Diagnostics.Process[] processes = System.Diagnostics.Process.GetProcessesByName(processName);
            foreach (System.Diagnostics.Process process in processes)
            {
                if (currentProcess == process.Id)
                {
                    continue;
                }

                // find MainWindow Title
                IntPtr hwnd = ProcessChecker.FindWindow(null, ProductTitle);
                if (hwnd.ToInt32() > 0)
                {
                    //Activate it
                    ProcessChecker.SetForegroundWindow(hwnd);

                    WindowShowStyle command = ProcessChecker.IsIconicNative(hwnd) ? WindowShowStyle.Restore : WindowShowStyle.Show;
                    ProcessChecker.ShowWindow(hwnd, command);
                }
            }

            App.Log.Write("!!DuplicateProcess!!");
            Current.Shutdown();
        }

        return result;
    }

    private void LogHelperConfigSetting()
    {
        System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
        Version version = assembly.GetName().Version;

        // 1. 로그헬퍼 환경설정 읽기
        // 1-1. 클라이언트에서 xml파일로 환경설정 읽기
        //LogHelper.XMLConfiguratorLoader.Loader("LogHelperXMLConfigure_bak.xml");
        // 1-1. 리소스에서 xml 읽기
        XMLConfiguratorLoader.LoaderByXML(global::MainEntry.Properties.Resources.LogHelperXMLConfigure);

        // 2. 프로그램 버전 설정
        XMLConfiguratorLoader.ProgramVersion = version.ToString();

        LogHelper.LogHelper.AddLogHelper(LogHelperType.Default, "__WPFMusic__");
    }
    #endregion  // Methods

    #region Events Handler

    private void Dispatcher_UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        try
        {
            e.Handled = true;

            App.Log.Write("DispatcherUnhandledException_Log.log", "[App Error Catch] {0}", e.Exception);
            App.Log.Write("DispatcherUnhandledException_Log.log", "[App_DispatcherUnhandledException] {0}", e.Exception.Message);
        }
        catch { }
    }

    private void Dispatcher_UnhandledExceptionFilter(object sender, System.Windows.Threading.DispatcherUnhandledExceptionFilterEventArgs e)
    {
        try
        {
            e.RequestCatch = true;

            App.Log.Write("DispatcherUnhandledExceptionFilter_Log.log", "[App Error Catch] {0}", e.Exception);
            App.Log.Write("DispatcherUnhandledExceptionFilter_Log.log", "[App_DispatcherUnhandledExceptionFilter] {0}", e.Exception.Message);
        }
        catch { }
    }

    #endregion Events Handler
}