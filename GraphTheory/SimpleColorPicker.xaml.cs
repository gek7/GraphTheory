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

namespace GraphTheory
{
    /// <summary>
    /// Логика взаимодействия для SimpleColorPicker.xaml
    /// </summary>
    public partial class SimpleColorPicker : UserControl
    {
        public delegate void ColorHandler(object sender, ColorArgs e);
        public event ColorHandler ColorChanged;
        public event ColorHandler ColorEnter;
        public event ColorHandler ColorLeave;
        private void OnColorChanged(ColorArgs e)
        {
            ColorChanged?.Invoke(this, e);
        }
        private void OnColorEnter(ColorArgs e)
        {
            ColorEnter?.Invoke(this, e);
        }
        private void OnColorLeave(ColorArgs e)
        {
            ColorLeave?.Invoke(this, e);
        }
        private Brush _selectedColor;
        public Brush selectedColor
        {
            get
            {
                return _selectedColor;
            }
            set
            {
                if (value != _selectedColor)
                {
                    curColor.Fill = value;
                    _selectedColor = value;
                }
            }
        }
        public SimpleColorPicker()
        {
            InitializeComponent();
            PropertyInfo[] _properties = typeof(Brushes).GetProperties();
            foreach (var i in _properties)
            {
                if (i.PropertyType == typeof(SolidColorBrush))
                {
                    SolidColorBrush s = i.GetValue(null, null) as SolidColorBrush;
                    Button btn = new Button();
                    btn.Background = Brushes.Transparent;
                    Rectangle r = new Rectangle();
                    btn.Content = r;
                    btn.BorderBrush = Brushes.Transparent;
                    btn.Click += Btn_Click;
                    btn.MouseEnter += Btn_MouseEnter;
                    btn.MouseLeave += Btn_MouseLeave;
                    r.Width = 10;
                    r.Height = 10;
                    r.Margin = new Thickness(5);
                    r.Fill = s;
                    wp.Children.Add(btn);
                }
            }
        }

        private void Btn_MouseLeave(object sender, MouseEventArgs e)
        {
            OnColorLeave(new ColorArgs(null, selectedColor));
        }

        private void Btn_MouseEnter(object sender, MouseEventArgs e)
        {
            Button btn = e.OriginalSource as Button;
            Brush NewColor = (btn.Content as Rectangle).Fill as Brush;
            OnColorEnter(new ColorArgs(NewColor, selectedColor));
        }

        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            Button btn = e.OriginalSource as Button;
            cmb.SelectedItem = FirstTab;
            Brush NewColor = (btn.Content as Rectangle).Fill as Brush;
            OnColorChanged(new ColorArgs(NewColor, selectedColor));
            selectedColor = NewColor;
            cbi.BringIntoView();
            cmb.IsDropDownOpen = false;
        }
    }
    public class ColorArgs : EventArgs
    {
        public Brush NewColor { get; set; }
        public Brush OldColor { get; set; }
        public ColorArgs(Brush newColor, Brush oldColor)
        {
            NewColor = newColor;
            OldColor = oldColor;
        }
    }
}
