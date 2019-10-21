using AllocatorInterface;
using MemoryModel;
using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Interaction logic for TestWindow.xaml
    /// </summary>
    public partial class TestWindow : Window
    {
        public Window Main;
        public IAllocator TestAllocator;
        public IAllocatorAnalizer TestAnalizer;

        public TestWindow()
        {
            InitializeComponent();
        }

        private Dictionary<int, Color> _colors;
        private uint _lastAlloc;
        private uint _lastFree;

        private void Window_Closed(object sender, EventArgs e)
        {
            Main.Visibility = Visibility.Visible;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            List<Color> colorList = new List<Color>{ Colors.White, Colors.Green, Colors.Blue, Colors.DarkRed, Colors.Orange };
            _colors = new Dictionary<int, Color>();
            var names = Enum.GetNames(typeof(MemoryAnalizerStatus)).ToArray();
            for (int i = 0; i < names.Length; i++)
            {
                _colors.Add(i, colorList[i]);
                Grid colorGrid = new Grid();
                ColumnDefinition firstColumn = new ColumnDefinition
                {
                    Width = new GridLength(100)
                };
                colorGrid.ColumnDefinitions.Add(firstColumn);
                ColumnDefinition secondColumn = new ColumnDefinition
                {
                    Width = new GridLength(50)
                };
                colorGrid.ColumnDefinitions.Add(secondColumn);

                Label colorLabel = new Label
                {
                    Content = names[i]
                };
                Grid.SetColumn(colorLabel, 0);
                colorGrid.Children.Add(colorLabel);

                Border colorRectangle = new Border
                {
                    Width = 50,
                    Background = new SolidColorBrush(colorList[i])
                };
                Grid.SetColumn(colorRectangle, 1);
                colorGrid.Children.Add(colorRectangle);

                LstbxColors.Items.Add(colorGrid);
            }
            ShowMemoryStatus();
        }

        private void ShowMemoryStatus()
        {
            MemoryAnalizerStatus[] memory = TestAnalizer.AnalizeMemory();
            CvMemoryStatus.Children.Clear();
            int memoryCellSize = 5;
            int columns = (int)(CvMemoryStatus.ActualWidth / memoryCellSize);
            int x;
            int y;

            if (_lastAlloc != TestAllocator.Null)
            {
                x = (int)(_lastAlloc % columns) * memoryCellSize;
                y = (int)(_lastAlloc / columns) * memoryCellSize;

                Rectangle rect = new Rectangle
                {
                    Fill = new SolidColorBrush(Colors.LightGreen),
                    Width = memoryCellSize + 1,
                    Height = memoryCellSize + 1
                };
                Canvas.SetLeft(rect, x);
                Canvas.SetTop(rect, y);
                CvMemoryStatus.Children.Add(rect);
            }

            if (_lastFree != TestAllocator.Null)
            {
                x = (int)(_lastFree % columns) * memoryCellSize;
                y = (int)(_lastFree / columns) * memoryCellSize;

                Rectangle rect = new Rectangle
                {
                    Fill = new SolidColorBrush(Colors.OrangeRed),
                    Width = memoryCellSize + 1,
                    Height = memoryCellSize + 1
                };
                Canvas.SetLeft(rect, x);
                Canvas.SetTop(rect, y);
                CvMemoryStatus.Children.Add(rect);
            }

            x = 1;
            y = 1;

            foreach (var cell in memory)
            {
                Rectangle rect = new Rectangle
                {
                    Fill = new SolidColorBrush(_colors[(int)cell]),
                    Width = memoryCellSize - 1,
                    Height = memoryCellSize - 1
                };
                Canvas.SetLeft(rect, x);
                Canvas.SetTop(rect, y);
                CvMemoryStatus.Children.Add(rect);

                x += memoryCellSize;
                if (x >= columns * memoryCellSize)
                {
                    x = 1;
                    y += memoryCellSize;
                }
            }
        }

        private void BtnAlloc_Click(object sender, RoutedEventArgs e)
        {
            bool isValid = uint.TryParse(TbAlloc.Text, out uint size);
            if (isValid)
            {
                uint addr = TestAllocator.Alloc(size);
                _lastAlloc = addr;
                _lastFree = TestAllocator.Null;
                if (addr != TestAllocator.Null)
                {
                    LstbxAlloc.Items.Add(addr);
                    LstbxAlloc.IsEnabled = true;
                    ShowMemoryStatus();
                }
                else
                {
                    MessageBox.Show("Cannot allocate", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void LstbxAlloc_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BtnFree.IsEnabled = true;
        }

        private void BtnFree_Click(object sender, RoutedEventArgs e)
        {
            uint address = (uint)LstbxAlloc.SelectedValue;
            _lastAlloc = TestAllocator.Null;
            _lastFree = address;
            LstbxAlloc.Items.Remove(LstbxAlloc.SelectedItem);
            TestAllocator.Free(address);
            BtnFree.IsEnabled = false;
            ShowMemoryStatus();
        }
    }
}
