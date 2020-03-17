using GraphTheory.Converters;
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
        #region События для прикладного программиста
        public event EventHandler FirstPeakSelected;
            public event EventHandler SecondPeakSelected;
            public event EventHandler NewFirstPeakSelected;
            public event EventHandler EdgeCreated;
            public event EventHandler PeakCreated;
        #endregion

        #region вызов обработчиков событий указанных прикладным програмистам
        protected virtual void OnFirstPeakSelected(EventArgs e)
        {
            EventHandler handler = FirstPeakSelected;
            handler?.Invoke(this, e);
        }
        protected virtual void OnSecondPeakSelected(EventArgs e)
        {
            EventHandler handler = SecondPeakSelected;
            handler?.Invoke(this, e);
        }
        protected virtual void OnNewFirstPeakSelected(EventArgs e)
        {
            EventHandler handler = NewFirstPeakSelected;
            handler?.Invoke(this, e);
        }
        protected virtual void OnEdgeCreated(EventArgs e)
        {
            EventHandler handler = EdgeCreated;
            handler?.Invoke(this, e);
        }
        protected virtual void OnPeakCreated(EventArgs e)
        {
            EventHandler handler = PeakCreated;
            handler?.Invoke(this, e);
        }
        #endregion

        #region Свойства зависимости
            public static readonly DependencyProperty PeakColorProperty;
            public static readonly DependencyProperty PeakWidthProperty;
        #endregion

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

        #region Методы для проверки корректности новых значений для свойств зависимости
        public static bool ValidatePeakColor(object color)
        {
            if (color is Brush && color != null)
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
                    if (item is Ellipse)
                    {
                        (item as Ellipse).Fill = (Brush)args.NewValue;
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
                    Ellipse el = item as Ellipse;
                    DoubleAnimation da = new DoubleAnimation(value, TimeSpan.FromSeconds(0.3));
                    el.BeginAnimation(Ellipse.WidthProperty, da);
                    el.BeginAnimation(Ellipse.HeightProperty, da);
                }
            }
        }
        #endregion

        private Peak FirstPeak;
        private Peak SecondPeak;
        private TextBox EditingTextBox;
        private Ellipse DraggingEllipse = null;
        private int PeakNum { get; set; }
        public Mode CurrentMode { get; set; }

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
        // Добавить ребро
        public bool AddNewRelation(TypeOfRelation type, double weight = 0)
        {
            if (FirstPeak != null && SecondPeak != null)
            {
                Peak firstPeak = FirstPeak;
                Peak secondPeak = SecondPeak;
                Relation rel = new Relation(secondPeak, weight, type);
                switch (type)
                {
                    case TypeOfRelation.Oriented:
                        firstPeak.Relations.Add(rel);
                        ArrowLine ar = new ArrowLine();
                        ar.X1 = Canvas.GetLeft(firstPeak.El) + PeakWidth / 2;
                        ar.Y1 = Canvas.GetTop(firstPeak.El) + PeakWidth / 2;
                        ar.X2 = Canvas.GetLeft(secondPeak.El) + PeakWidth / 2;
                        ar.Y2 = Canvas.GetTop(secondPeak.El) + PeakWidth / 2;
                        ar.Stroke = Brushes.Green;
                        ar.StrokeThickness = 4;
                        Canvas.SetZIndex(ar, -1);
                        canv.Children.Add(ar);

                        if (weight > 0)
                        {
                            Label txt = new Label();
                            txt.Content = weight;
                            txt.Foreground = Brushes.White;
                            txt.Background = Brushes.Black;
                            canv.Children.Add(txt);
                            Canvas.SetLeft(txt, (ar.X1 + ar.X2) / 2 - 20);
                            Canvas.SetTop(txt, ((ar.Y1 + ar.Y2) / 2) - 35);
                        }
                        break;
                    case TypeOfRelation.NonOriented:
                        firstPeak.Relations.Add(rel);
                        secondPeak.Relations.Add(new Relation(firstPeak, weight, type));
                        Line l = new Line();
                        l.X1 = Canvas.GetLeft(firstPeak.El) + PeakWidth / 2;
                        l.Y1 = Canvas.GetTop(firstPeak.El) + PeakWidth / 2;
                        l.X2 = Canvas.GetLeft(secondPeak.El) + PeakWidth / 2;
                        l.Y2 = Canvas.GetTop(secondPeak.El) + PeakWidth / 2;
                        l.Stroke = Brushes.Green;
                        l.StrokeThickness = 4;
                        Canvas.SetZIndex(l, -1);
                        canv.Children.Add(l);
                        if (weight > 0)
                        {
                            Label txt = new Label();
                            txt.Content = weight;
                            txt.Foreground = Brushes.White;
                            txt.Background = Brushes.Black;
                            canv.Children.Add(txt);
                            Canvas.SetLeft(txt, (l.X1 + l.X2) / 2 - 20);
                            Canvas.SetTop(txt, ((l.Y1 + l.Y2) / 2) - 35);
                        }
                        break;
                }
                (FirstPeak.El as Ellipse).Stroke = Brushes.Black;
                (SecondPeak.El as Ellipse).Stroke = Brushes.Black;
                FirstPeak = null;
                SecondPeak = null;
                OnEdgeCreated(new EventArgs());
                return true;
            }
            else 
            {
                return false;
            }
        }
        // Создать вершину
        private void CreatePeak(object sender, MouseButtonEventArgs e)
        {
            double x = e.GetPosition(canv).X;
            double y = e.GetPosition(canv).Y;
            Ellipse el = new Ellipse();
            el.Stroke = Brushes.Black;
            el.Fill = PeakColor ?? Brushes.White;
            Canvas.SetLeft(el,x-PeakWidth/2);
            Canvas.SetTop(el, y - PeakWidth / 2);
            ////// Создание надписи над эллипсе /////
            TextBox tb = new TextBox();
            tb.Text = PeakNum.ToString();
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            // Установка зависимости размера шрифта от размеров эллипса
            Binding b = new Binding();
            b.Source = el;
            b.Path = new PropertyPath(WidthProperty);
            b.Converter = new FontSizeConverter();
            tb.SetBinding(FontSizeProperty, b);
            tb.IsReadOnly=true;
            tb.Background = Brushes.Transparent;
            tb.BorderThickness = new Thickness(0);
            tb.MouseDoubleClick += Tb_MouseDoubleClick;
            tb.KeyDown += tb_KeyDown;
            // Привязка координат надписи к координатам эллипсам
            b = new Binding();
            b.Source = el;
            b.Path = new PropertyPath(Canvas.LeftProperty);
            b.Converter = new LeftTextBoxConverter();
            b.ConverterParameter = PeakWidth;
            tb.SetBinding(Canvas.LeftProperty, b);
            b = new Binding();
            b.Source = el;
            b.Path = new PropertyPath(Canvas.TopProperty);
            b.ConverterParameter = PeakWidth;
            b.Converter = new TopTextBoxConverter();
            tb.SetBinding(Canvas.TopProperty, b);
            Canvas.SetZIndex(tb, 2);
            // События для перемещения эллипса
            canv.MouseRightButtonDown += canv_MouseRightDown;
            canv.MouseLeave += canv_MouseLeave;
            canv.MouseRightButtonUp += canv_MouseRightButtonUp;
            canv.MouseMove += canv_MouseMove;
            // Добавление созданных эл-ов на canvas
            canv.Children.Add(el);
            canv.Children.Add(tb);
            
            // Анимация
            DoubleAnimation da = new DoubleAnimation(0.1,PeakWidth, TimeSpan.FromSeconds(0.3));
            DoubleAnimation da2 = new DoubleAnimation(x,x - PeakWidth / 2, TimeSpan.FromSeconds(0.3));
            DoubleAnimation da3 = new DoubleAnimation(y,y - PeakWidth / 2, TimeSpan.FromSeconds(0.3));
            // Запуск анимации
            el.BeginAnimation(Ellipse.WidthProperty, da);
            el.BeginAnimation(Ellipse.HeightProperty, da);
            el.BeginAnimation(Canvas.LeftProperty, da2);
            el.BeginAnimation(Canvas.TopProperty, da3);
            // Инкремент счётчика названий
            PeakNum++;
            new Peak(el,tb);
        }
        // Выбрать вершину для соединения
        private void SelectPeakForEdge(object sender, MouseButtonEventArgs e)
        {
            DoubleAnimation da = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.3));
            da.AutoReverse = true;
            da.RepeatBehavior = RepeatBehavior.Forever;
            Ellipse ClickedEllipse=null;
            if ((e.Source as FrameworkElement) is Ellipse)
                 ClickedEllipse = (e.Source as FrameworkElement) as Ellipse;
            if (ClickedEllipse!=null && FirstPeak == null)
            {
                FirstPeak = Peak.FindByEllipse(ClickedEllipse);
                (FirstPeak.El).Stroke = Brushes.Red;
                OnFirstPeakSelected(new EventArgs());
            }
            else if(ClickedEllipse != null && SecondPeak==null)
            {
                SecondPeak = Peak.FindByEllipse(ClickedEllipse);
                (SecondPeak.El as Ellipse).Stroke = Brushes.Red;
                //AddNewRelation(TypeOfRelation.Oriented);
                OnSecondPeakSelected(new EventArgs());
            }
            else if(ClickedEllipse != null)
            {
                (FirstPeak.El as Ellipse).Stroke = Brushes.Black;
                (SecondPeak.El as Ellipse).Stroke = Brushes.Black;
                SecondPeak = null;
                FirstPeak = Peak.FindByEllipse(ClickedEllipse);
                (FirstPeak.El as Ellipse).Stroke = Brushes.Red;
                OnNewFirstPeakSelected(new EventArgs());
            }
        }
        // Отменить выбор вершин
        public void CancelSelectionPeaks()
        {
            FirstPeak.El.Stroke = Brushes.Black;
            SecondPeak.El.Stroke = Brushes.Black;
            FirstPeak = null;
            SecondPeak = null;
        }
        // Убрать каретку с названия текстбокса
        void UnfocusEditingTextBox(MouseButtonEventArgs e)
        {
            if (EditingTextBox != null && e.Source != EditingTextBox)
            {
                Peak.FindByTextBox(EditingTextBox).SetName(EditingTextBox.Text);
                EditingTextBox.IsReadOnly = true;
                EditingTextBox.Select(0, 0);
                EditingTextBox = null;
            }
        }

        #region Обработчики событий компонента
        private void canv_MouseLeave(object sender, MouseEventArgs e)
            {
                DraggingEllipse = null;
            }
            private void canv_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
            {
                DraggingEllipse = null;
            }
            private void canv_MouseRightDown(object sender, MouseButtonEventArgs e)
            {
                DraggingEllipse = e.Source as Ellipse;
            }
            private void Canv_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
            {
                switch (CurrentMode)
                {
                    case Mode.AddPeak:
                        CreatePeak(sender, e);
                        OnPeakCreated(new EventArgs());
                        break;
                    case Mode.AddEdge:
                        SelectPeakForEdge(sender, e);
                        break;
                    default:
                        break;
                }
            }
            private void canv_MouseMove(object sender, MouseEventArgs e)
            {
                if (DraggingEllipse != null)
                {
                    double x = e.GetPosition(canv).X;
                    double y = e.GetPosition(canv).Y;
                    DraggingEllipse.BeginAnimation(Canvas.LeftProperty, null);
                    DraggingEllipse.BeginAnimation(Canvas.TopProperty, null);
                    Canvas.SetLeft(DraggingEllipse, x - PeakWidth / 2);
                    Canvas.SetTop(DraggingEllipse, y - PeakWidth / 2);
                }
            }
            private void canv_MouseDown(object sender, MouseButtonEventArgs e)
            {
                    UnfocusEditingTextBox(e);
            }
            private void Tb_MouseDoubleClick(object sender, MouseButtonEventArgs e)
            {
                (sender as TextBox).IsReadOnly = false;
                EditingTextBox = (sender as TextBox);
            }
            private void tb_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                UnfocusEditingTextBox(null);
            }
        }
        #endregion
    }
}
