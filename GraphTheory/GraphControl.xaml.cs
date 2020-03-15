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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GraphTheory
{
    public enum Mode
    {
        AddPeak,
        AddEdge
    }

    /// <summary>
    /// Компонент 'теория графов'
    /// </summary>
    public partial class GraphControl : UserControl
    {
        public static readonly DependencyProperty PeakColorProperty;
        public static readonly DependencyProperty PeakWidthProperty;
        private Peak FirstSelectedPeak;
        private Peak SecondSelectedPeak;
        private int PeakNum { get; set; }
        public Mode CurrentMode { get; set; }
        #region CLR свойства для свойств зависимости
        public Brush PeakColor
        {
            get { return (Brush)GetValue(PeakColorProperty); }
            set { SetValue(PeakColorProperty, value); }
        }

        private double PeakWidth
        {
            get { return (double)GetValue(PeakWidthProperty); }
            set { SetValue(PeakWidthProperty, value); }
        }
        #endregion

        static GraphControl()
        {
            PeakColorProperty =
            DependencyProperty.Register(
            "PeakColor",
            typeof(Brush),
            typeof(GraphControl),
            new UIPropertyMetadata(Brushes.White, new PropertyChangedCallback(PeakColorChanged)),
            new ValidateValueCallback(ValidatePeakColor));

            PeakWidthProperty =
            DependencyProperty.Register("PeakWidth", typeof(double), typeof(GraphControl),
            new UIPropertyMetadata(0.1, new PropertyChangedCallback(PeakWidthChanged)), new ValidateValueCallback(ValidatePeakWidth));
        }
        
        public GraphControl()
        {
            InitializeComponent();
            PeakNum = 0;
            PeakWidth = 50;
        }

        #region Методы для проверки корректности новых значений для свойств зависимости
        public static bool ValidatePeakColor(object color)
        {
            if(color is Brush && color !=null)
            {
                return true;
            }
            return false;
        }

        public static bool ValidatePeakWidth(object width)
        {
            double value = (double)width;
            if (value > 0 && value <= 35000)
            {
                return true;
            }
            return false;
        }
        #endregion

        #region Методы для изменения значений для свойств зависимости
        public static void PeakColorChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs args)
        {
            if (depobj is GraphControl)
            {
                GraphControl c = depobj as GraphControl;
                foreach (var item in (c.Content as Canvas).Children)
                {
                    if (item is Grid)
                    {
                        ((item as Grid).Children[0] as Ellipse).Fill = (Brush)args.NewValue;
                    }
                }
            }
        }
        public static void PeakWidthChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs args)
        {
            double value = (double)args.NewValue;
            Canvas c = (depobj as UserControl).Content as Canvas;
            foreach (var item in c.Children)
            {
                if (item is Grid)
                {
                    Ellipse el = ((item as Grid).Children[0] as Ellipse);
                    DoubleAnimation da = new DoubleAnimation(value, TimeSpan.FromSeconds(0.3));
                    el.BeginAnimation(Ellipse.WidthProperty, da);
                    el.BeginAnimation(Ellipse.HeightProperty, da);
                }
            }
        }
        #endregion

        public void AddNewRelation(Peak firstPeak, Peak secondPeak, TypeOfRelation type, double weight = 0)
        {
            Relation rel = new Relation(secondPeak, weight, type);
            DoubleAnimation daX = new DoubleAnimation(Canvas.GetLeft(firstPeak.El) + PeakWidth / 2, Canvas.GetLeft(secondPeak.El) + PeakWidth / 2, TimeSpan.FromSeconds(0.3));
            DoubleAnimation daY = new DoubleAnimation(Canvas.GetTop(firstPeak.El) + PeakWidth / 2, Canvas.GetTop(secondPeak.El) + PeakWidth / 2, TimeSpan.FromSeconds(0.3));
            switch (type)
            {
                case TypeOfRelation.Oriented:
                    firstPeak.Relations.Add(rel);
                    ArrowLine ar = new ArrowLine();
                    ar.X1 = Canvas.GetLeft(firstPeak.El) + PeakWidth / 2;
                    ar.Y1 = Canvas.GetTop(firstPeak.El) + PeakWidth / 2;
                    ar.Stroke = Brushes.Green;
                    ar.StrokeThickness = 4;
                    Canvas.SetZIndex(ar, -1);
                    canv.Children.Add(ar);
                    ar.BeginAnimation(ArrowLine.X2Property, daX);
                    ar.BeginAnimation(ArrowLine.Y2Property, daY);
                    break;
                case TypeOfRelation.NonOriented:
                    firstPeak.Relations.Add(rel);
                    secondPeak.Relations.Add(new Relation(firstPeak, weight, type));
                    Line l = new Line();
                    l.X1 = Canvas.GetLeft(firstPeak.El)+PeakWidth/2;
                    l.Y1= Canvas.GetTop(firstPeak.El)+ PeakWidth / 2;
                    l.X2= Canvas.GetLeft(firstPeak.El) + PeakWidth / 2;
                    l.Y2= Canvas.GetTop(firstPeak.El) + PeakWidth / 2;
                    l.Stroke = Brushes.Green;
                    l.StrokeThickness = 4;
                    Canvas.SetZIndex(l, -1);
                    canv.Children.Add(l);
                    l.BeginAnimation(Line.X2Property, daX);
                    l.BeginAnimation(Line.Y2Property, daY);
                    break;
            }
            SecondSelectedPeak.El.BeginAnimation(OpacityProperty, null);
            FirstSelectedPeak.El.BeginAnimation(OpacityProperty, null);
            FirstSelectedPeak = null;
            SecondSelectedPeak = null;
        }
        private void createPeak(object sender, MouseButtonEventArgs e)
        {
            Grid g = new Grid();
            double x = e.GetPosition(canv).X;
            double y = e.GetPosition(canv).Y;
            Ellipse el = new Ellipse();
            g.Children.Add(el);
            el.Stroke = Brushes.Black;
            el.Fill = PeakColor ?? Brushes.White;
            ////// Создание надписи в эллипсе /////
            Label l = new Label();
            l.Content = PeakNum.ToString();
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
            // Для отладки
            //el.Opacity = 0.5;
            // Анимация
            DoubleAnimation da = new DoubleAnimation(0.1,PeakWidth, TimeSpan.FromSeconds(0.3));
            DoubleAnimation da2 = new DoubleAnimation(x,x - PeakWidth / 2, TimeSpan.FromSeconds(0.3));
            DoubleAnimation da3 = new DoubleAnimation(y,y - PeakWidth / 2, TimeSpan.FromSeconds(0.3));

            el.BeginAnimation(Ellipse.WidthProperty, da);
            el.BeginAnimation(Ellipse.HeightProperty, da);
            g.BeginAnimation(Canvas.LeftProperty, da2);
            g.BeginAnimation(Canvas.TopProperty, da3);
            PeakNum++;
            new Peak(g);
        }
        private void SelectPeakForEdge(object sender, MouseButtonEventArgs e)
        {
            DoubleAnimation da = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.3));
            da.AutoReverse = true;
            da.RepeatBehavior = RepeatBehavior.Forever;
            if ((e.Source as FrameworkElement).Parent is Grid && FirstSelectedPeak == null)
            {
                FirstSelectedPeak = new Peak((e.Source as FrameworkElement).Parent as Grid);
                FirstSelectedPeak.El.BeginAnimation(OpacityProperty, da);
            }
            else if((e.Source as FrameworkElement).Parent is Grid && SecondSelectedPeak==null)
            {
                SecondSelectedPeak = new Peak((e.Source as FrameworkElement).Parent as Grid);
                SecondSelectedPeak.El.BeginAnimation(OpacityProperty, da);
                FirstSelectedPeak.El.BeginAnimation(OpacityProperty, da);
                AddNewRelation(FirstSelectedPeak, SecondSelectedPeak, TypeOfRelation.Oriented);
            }
            else if((e.Source as FrameworkElement).Parent is Grid)
            {
                FirstSelectedPeak.El.BeginAnimation(OpacityProperty, null);
                SecondSelectedPeak.El.BeginAnimation(OpacityProperty, null);
                SecondSelectedPeak = null;
                //SecondSelectedPeak
                FirstSelectedPeak = new Peak((e.Source as FrameworkElement).Parent as Grid);
                FirstSelectedPeak.El.BeginAnimation(Grid.OpacityProperty, da);
            }
        }
        public void CancelSelectionPeaks()
        {
            FirstSelectedPeak.El.BeginAnimation(OpacityProperty, null);
            SecondSelectedPeak.El.BeginAnimation(OpacityProperty, null);
            FirstSelectedPeak = null;
            SecondSelectedPeak = null;
        }
        private void canv_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            switch (CurrentMode)
            {
                case Mode.AddPeak:
                    createPeak(sender, e);
                    break;
                case Mode.AddEdge:
                    SelectPeakForEdge(sender, e);
                    break;
                default:
                    break;
            }
        }
    }
}
