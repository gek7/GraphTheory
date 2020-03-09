﻿using System;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GraphTheory
{
    /// <summary>
    /// Логика взаимодействия для UserControl1.xaml
    /// </summary>
    public partial class GraphControl : UserControl
    {
        public static DependencyProperty PeekColorProperty;
        public int PeekNum { get; set; }
        public Brush PeekColor
        {
            get { return (Brush)GetValue(PeekColorProperty); }
            set { SetValue(PeekColorProperty, value); }
        }
        double _peekWidth;
        public double PeekWidth
        {
            get
            {
                return _peekWidth;
            }
            set
            {
                if (value > 0 && value != _peekWidth)
                {
                    _peekWidth = value;

                    foreach (var item in canv.Children)
                    {
                        if (item is Grid)
                        {
                            Ellipse el = ((item as Grid).Children[0] as Ellipse);
                            DoubleAnimation da = new DoubleAnimation(value, TimeSpan.FromSeconds(0.3));
                            el.BeginAnimation(Ellipse.WidthProperty, da);
                            el.BeginAnimation(Ellipse.HeightProperty, da);
                            //el.Width = value;
                            //el.Height = value;
                        }
                    }
                }
            }
        }

        static GraphControl()
        {
            PeekColorProperty =
            DependencyProperty.Register("PeekColor", typeof(Brush), typeof(GraphControl),
                new PropertyMetadata(Brushes.White, new PropertyChangedCallback(PeekColorChanged)), new ValidateValueCallback(ValidatePeekColor));
        }
        
        public GraphControl()
        {
            InitializeComponent();
            _peekWidth = 1;
            PeekNum = 0;
        }


        public static bool ValidatePeekColor(object color)
        {
            if(color is Brush && color !=null)
            {
                return true;
            }
            return false;
        }

        public static void PeekColorChanged(DependencyObject depobj,DependencyPropertyChangedEventArgs args)
        {
            if(depobj is GraphControl)
            {
                GraphControl c = depobj as GraphControl;
                foreach (var item in (c.Content as Canvas).Children)
                {
                    if(item is Ellipse)
                    {
                        (item as Ellipse).Fill = (Brush)args.NewValue;
                    }
                }
            }
        }

        private void createPeek(object sender, MouseButtonEventArgs e)
        {
            Grid g = new Grid();
            double x = e.GetPosition(canv).X;
            double y = e.GetPosition(canv).Y;
            Ellipse el = new Ellipse();
            g.Children.Add(el);
            el.Stroke = Brushes.Black;
            el.Fill = PeekColor ?? Brushes.White;
            ////// Создание надписи в эллипсе /////
            Label l = new Label();
            l.Content = PeekNum.ToString();
            l.VerticalAlignment = VerticalAlignment.Center;
            l.HorizontalAlignment = HorizontalAlignment.Center;
            g.Children.Add(l);
            // Установка зависимости размера шрифта от размеров эллипса
            Binding b = new Binding();
            b.Source = el;
            b.Path = new PropertyPath(Ellipse.WidthProperty);
            b.Converter = new FontSizeConverter();
            l.SetBinding(Label.FontSizeProperty, b);
            // Добавление созданных эл-ов на canvas
            canv.Children.Add(g);

            // Анимация
            DoubleAnimation da = new DoubleAnimation(0.1,PeekWidth, TimeSpan.FromSeconds(0.3));
            DoubleAnimation da2 = new DoubleAnimation(x,x - PeekWidth / 2, TimeSpan.FromSeconds(0.3));
            DoubleAnimation da3 = new DoubleAnimation(y,y - PeekWidth / 2, TimeSpan.FromSeconds(0.3));

            el.BeginAnimation(Ellipse.WidthProperty, da);
            el.BeginAnimation(Ellipse.HeightProperty, da);
            g.BeginAnimation(Canvas.LeftProperty, da2);
            g.BeginAnimation(Canvas.TopProperty, da3);
            PeekNum++;
            new Peak(g);
        }

        private void canv_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }
    }
}