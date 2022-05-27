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

namespace Views.UI
{
    /// <summary>
    /// PlayListUI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PlayListUI : UserControl
    {
        public PlayListUI()
        {
            InitializeComponent();
        }

        private void xSearchPlayList_GotFocus(object sender, RoutedEventArgs e)
        {
            this.xSearchBorder.BorderThickness = new Thickness(1);
            this.xSearchPlayListHint.Visibility = Visibility.Collapsed;
        }

        private void xSearchPlayList_LostFocus(object sender, RoutedEventArgs e)
        {
            this.xSearchBorder.BorderThickness = new Thickness(0);
            if (this.xSearchPlayList.Text.Length <= 0)
            {
                this.xSearchPlayListHint.Visibility = Visibility.Visible;
            }
        }

        private void xSearchPlayListHint_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.xSearchPlayList.Focus();
        }

        private void xDeduplicationBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("기능 미구현");
        }

        private void xAddFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openDialog = new Microsoft.Win32.OpenFileDialog();
            openDialog.Multiselect = true;
            openDialog.Filter = "(*.mp3, *.m4a, *.wav)|*.mp3;*.m4a;*.wav";

            if (openDialog.ShowDialog() is true)
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
}
