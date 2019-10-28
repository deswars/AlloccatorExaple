using AllocatorInterface;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

namespace AllocatorExampleGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Assemblies (*.dll)|*.dll"
            };
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    Assembly assembly = Assembly.LoadFile(dialog.FileName);
                    var type = typeof(IAllocatorBuilder);
                    var list = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes()).Where(p => type.IsAssignableFrom(p) && p.IsClass);
                    LstbxBuilders.Items.Clear();
                    foreach (var builder in list)
                    {
                        LstbxBuilders.Items.Add(builder);
                    }

                    type = typeof(IAllocatorReallocableBuilder);
                    list = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes()).Where(p => type.IsAssignableFrom(p) && p.IsClass);
                    LstbxReallocBuilders.Items.Clear();
                    foreach (var builder in list)
                    {
                        LstbxReallocBuilders.Items.Add(builder);
                    }

                    BtnRun.IsEnabled = false;
                    BtnReallocRun.IsEnabled = false;
                }
                catch
                {
                    MessageBox.Show("Cannot load assembly", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnRun_Click(object sender, RoutedEventArgs e)
        {
            ParamWindow paramWindow = new ParamWindow
            {
                Builder = (Type)LstbxBuilders.SelectedItem,
                Main = this,
                IsReallocable = false
            };

            paramWindow.Show();
            Visibility = Visibility.Hidden;
        }

        private void LbBuilders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BtnRun.IsEnabled = true;
        }

        private void LstbxRealocBuilders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BtnReallocRun.IsEnabled = true;
        }

        private void BtnRealocRun_Click(object sender, RoutedEventArgs e)
        {
            ParamWindow ParamWindow = new ParamWindow
            {
                Builder = (Type)LstbxReallocBuilders.SelectedItem,
                Main = this,
                IsReallocable = true
            };

            ParamWindow.Show();
            Visibility = Visibility.Hidden;
        }
    }
}
