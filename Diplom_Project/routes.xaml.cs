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

namespace Diplom_Project
{
    public partial class routes : Window
    {
        public routes()
        {
            InitializeComponent();
        }
        private void Logo_Click(object sender, MouseButtonEventArgs e)
        {
            MainWindow go_windowmain = new MainWindow();
            go_windowmain.Show();
            this.Close();
        }
    }
}
