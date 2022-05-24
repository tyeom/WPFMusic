using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MainEntry;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class WPFMusicShell : Window
{
    /// <summary>
    /// 기존 창 크기
    /// </summary>
    private Rect _originWindow;

    public WPFMusicShell()
    {
        InitializeComponent();

        this.Loaded += this.WPFMusicShell_Loaded;
    }

    private void WPFMusicShell_Loaded(object sender, RoutedEventArgs e)
    {
        WindowInteropHelper interopHelper = new WindowInteropHelper(Application.Current.MainWindow);
        this.Tag = interopHelper.Handle;
    }

    private void TitleGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
        {
            this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

            _originWindow.Width = this.Width;
            _originWindow.Height = this.Height;
        }
        else if (e.LeftButton == MouseButtonState.Pressed)
        {
            this.DragMove();
        }
    }

    private void MaximizeToggleButton_Click(object sender, RoutedEventArgs e)
    {
        this.WindowState = this.xMaximizeToggleButton.IsChecked == true ? WindowState.Normal : WindowState.Maximized;
    }

    private void MinimizeToggleButton_Click(object sender, RoutedEventArgs e)
    {
        this.WindowState = WindowState.Minimized;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
        this.Close();
    }

    private void Grid_MouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            this.DragMove();
        }
    }
}