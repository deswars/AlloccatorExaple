using AllocatorInterface;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AllocatorExampleGUI
{
    /// <summary>
    /// Interaction logic for ParamWindow.xaml
    /// </summary>
    public partial class ParamWindow : Window
    {
        public Window Main;
        public Type Builder;

        public ParamWindow()
        {
            InitializeComponent();
        }

        private IAllocatorBuilder _builder;
        private Dictionary<string, string> _paramList;
        private Grid _paramGrid;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _builder = (IAllocatorBuilder)Activator.CreateInstance(Builder);
            _paramList = _builder.GetParameterList();
            _paramGrid = new Grid();
            ColumnDefinition firstColumn = new ColumnDefinition
            {
                Width = new GridLength(100)
            };
            _paramGrid.ColumnDefinitions.Add(firstColumn);
            ColumnDefinition secondColumn = new ColumnDefinition
            {
                Width = new GridLength(200)
            };
            _paramGrid.ColumnDefinitions.Add(secondColumn);

            int row = 0;
            foreach (var param in _paramList)
            {
                RowDefinition gridRow = new RowDefinition
                {
                    Height = new GridLength(40)
                };
                _paramGrid.RowDefinitions.Add(gridRow);
                Label paramName = new Label
                {
                    Content = param.Key,
                    Margin = new Thickness(5)
                };
                Grid.SetColumn(paramName, 0);
                Grid.SetRow(paramName, row);
                _paramGrid.Children.Add(paramName);
                TextBox paramValue = new TextBox
                {
                    Text = param.Value,
                    Margin = new Thickness(5)
                };
                Grid.SetColumn(paramValue, 1);
                Grid.SetRow(paramValue, row);
                _paramGrid.Children.Add(paramValue);
                row++;
            }
            SwParams.Content = _paramGrid;
        }

        private void BtnCreate_Click(object sender, RoutedEventArgs e)
        {
            int i = 0;
            Dictionary<string, string> resultList = new Dictionary<string, string>();
            while (i < _paramGrid.Children.Count)
            {
                Label paramName = (Label)_paramGrid.Children[i];
                string key = paramName.Content.ToString();
                TextBox paramValue = (TextBox)_paramGrid.Children[i + 1];
                string value = paramValue.Text; 
                i += 2;
                resultList.Add(key, value);
            }
            _builder.SetParameterList(resultList);

            TestWindow testWindow = new TestWindow
            {
                Main = this.Main,
                TestAllocator = _builder.Build(),
                TestAnalizer = _builder.BuildAnalizer()
            };

            testWindow.Show();
            this.Close();
        }
    }
}
