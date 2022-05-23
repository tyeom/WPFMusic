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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Views.Windows;

/// <summary>
/// PopupWindow.xaml에 대한 상호 작용 논리
/// </summary>
public partial class PopupWindow : Window, IDialog
{
    public PopupWindow()
    {
        InitializeComponent();
    }

    public Action CloseCallback { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
}