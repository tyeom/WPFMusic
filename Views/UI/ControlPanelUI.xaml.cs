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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Views.UI;
/// <summary>
/// ControlPanelUI.xaml에 대한 상호 작용 논리
/// </summary>
public partial class ControlPanelUI : UserControl
{
    public ControlPanelUI()
    {
        InitializeComponent();
    }

    private void xOpenBtn_Click(object sender, RoutedEventArgs e)
    {
        Microsoft.Win32.OpenFileDialog openDialog = new Microsoft.Win32.OpenFileDialog();
        openDialog.Multiselect = true;
        openDialog.Filter = "(*.mp3, *.m4a, *.wav)|*.mp3;*.m4a;*.wav";

        if(openDialog.ShowDialog() is true)
        {
            Button btn = (Button)sender;
            btn.Tag = openDialog.FileNames;
        }
        else
        {
            Button btn = (Button)sender;
            btn.Tag = null;
        }
    }
}
