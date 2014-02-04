using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StreamingExperience.Views
{
    public partial class PopupToResults : ChildWindow
    {
        public PopupToResults()
        {
            InitializeComponent();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            //this.Close();
            //NavigationService.Navigate(new Uri(@"\Results", UriKind.Relative));
        }
    }
}

