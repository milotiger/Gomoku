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

namespace Gomoku
{
    /// <summary>
    /// Interaction logic for ModePicker.xaml
    /// </summary>
    public partial class ModePicker : Window
    {
        public PlayMode Mode;
        public string MyName;
        private bool isNameChanged = false;
        public ModePicker()
        {
            InitializeComponent();
            foreach (var item in Enum.GetValues(typeof (PlayMode)))
            {
                ModeBox.Items.Add(item);
            }
            ModeBox.SelectedIndex = 0;
            Mode = (PlayMode)ModeBox.SelectedItem;
        }

        private void Name_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox NameTb = sender as TextBox;
            if (NameTb != null)
            {
                NameTb.Text = "";
                NameTb.Foreground = Brushes.Black;
            }
        }

        private void Name_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox NameTb = sender as TextBox;
            if (NameTb != null && NameTb.Text == "")
            {
                NameTb.Foreground = Brushes.DarkGray;
                NameTb.Text = "Enter Your Name!";
                isNameChanged = false;
            }
            else isNameChanged = true;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Mode = (PlayMode)ModeBox.SelectedItem;
            if (isNameChanged)
                MyName = NameTb.Text;
            else MyName = "WinDev";
            DialogResult = true;
            this.Close();
        }
    }
}
