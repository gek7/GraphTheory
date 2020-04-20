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
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Microsoft.Win32;
using Tomers.WPF.Shapes;

namespace GraphTheory
{
    public enum Mode
    {
        AddPeak,
        AddEdge,
        DeleteSelected,
        SelectForAlghorithm,
        MoveObjects,
        None
    }
    public enum TypeOfRelation
    {
        Oriented,
        NonOriented
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

        #region Методы для вызова обработчиков событий указанных прикладным программистом
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

        #region Обычные CLR свойства
        private bool _AutoWork = true;
        public bool AutoWork
        {
            get
            {
                return _AutoWork;
            }
            set
            {
                if (value != _AutoWork)
                {
                    if (value)
                    {
                        exp.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        exp.Visibility = Visibility.Collapsed;
                    }
                    _AutoWork = value;
                }
            }
        }
        #endregion

        #region Cвойства зависимости

        public static readonly DependencyProperty PeakColorProperty;
        public static readonly DependencyProperty PeakWidthProperty;
        public new static readonly DependencyProperty BackgroundProperty;
        public static readonly DependencyProperty RelationColorProperty;
        public static readonly DependencyProperty WeightForegroundProperty;
        public static readonly DependencyProperty PeakForegroundProperty;
        public static readonly DependencyProperty WeightBackgroundProperty;
        private static readonly DependencyProperty ManagePartBackgroundProperty;
        private static readonly DependencyProperty ManagePartTextColorProperty;

        #region CLR свойства для свойств зависимости
        public Brush PeakColor
        {
            get { return (Brush)GetValue(PeakColorProperty); }
            set { SetValue(PeakColorProperty, value); }
        }

        public double PeakWidth
        {
            get { return (double)GetValue(PeakWidthProperty); }
            set { SetValue(PeakWidthProperty, value); }
        }
        public new Brush Background
        {
            get { return (Brush)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        public Brush RelationColor
        {
            get { return (Brush)GetValue(RelationColorProperty); }
            set { SetValue(RelationColorProperty, value); }
        }

        public Brush WeightForeground
        {
            get { return (Brush)GetValue(WeightForegroundProperty); }
            set { SetValue(WeightForegroundProperty, value); }
        }

        public Brush PeakForeground
        {
            get { return (Brush)GetValue(PeakForegroundProperty); }
            set { SetValue(PeakForegroundProperty, value); }
        }

        public Brush WeightBackground
        {
            get { return (Brush)GetValue(WeightBackgroundProperty); }
            set { SetValue(WeightBackgroundProperty, value); }
        }
        private Brush ManagePartBackground
        {
            get { return (Brush)GetValue(ManagePartBackgroundProperty); }
            set { SetValue(ManagePartBackgroundProperty, value); }
        }

        private Brush ManagePartTextColor
        {
            get { return (Brush)GetValue(ManagePartTextColorProperty); }
            set { SetValue(ManagePartTextColorProperty, value); }
        }
        #endregion

        #region Методы для проверки корректности новых значений для свойств зависимости
        public static bool ValidateColor(object color)
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
                foreach (var item in ((c.Content as Grid).Children[0] as Canvas).Children)
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
            UserControl uc = (depobj as UserControl);
            GraphControl gp = uc as GraphControl;
            //Canvas c = ((uc.Content as Grid).Children[0] as ScrollViewer).Content as Canvas;
            Canvas c = (uc.Content as Grid).Children[0] as Canvas;
            foreach (var item in c.Children)
            {
                if (item is Ellipse)
                {
                    Ellipse el = item as Ellipse;
                    TextBox tb = Peak.FindByEllipse(el).tb;
                    // Сохранение привязок до изменения значений вручную
                    BindingExpression LeftBind = tb.GetBindingExpression(Canvas.LeftProperty);
                    BindingExpression TopBind = tb.GetBindingExpression(Canvas.TopProperty);
                    double left = Canvas.GetLeft(el);
                    double top = Canvas.GetTop(el);
                    // Изменение значения вручную
                    Canvas.SetLeft(tb, left + gp.PeakWidth / 2);
                    Canvas.SetTop(tb, top - gp.PeakWidth);
                    DoubleAnimation da = new DoubleAnimation(value, TimeSpan.FromSeconds(0.3));
                    el.BeginAnimation(Ellipse.WidthProperty, da);
                    el.BeginAnimation(Ellipse.HeightProperty, da);
                    tb.SetBinding(Canvas.LeftProperty, LeftBind.ParentBindingBase);
                    tb.SetBinding(Canvas.TopProperty, TopBind.ParentBindingBase);
                }
            }
        }

        public static void BackgroundChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs args)
        {
            if (depobj is GraphControl)
            {
                GraphControl c = depobj as GraphControl;
                if (args.NewValue != args.OldValue)
                {
                    ((c.Content as Grid).Children[0] as Canvas).Background = (args.NewValue as Brush);
                }
            }
        }

        public static void RelationColorChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue != args.OldValue)
            {
                Relation.AllRelations.ForEach(t => (t.LineObj as Shape).Stroke = (args.NewValue as Brush));
            }
        }

        public static void WeightForegroundChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs args)
        {
            if (depobj is GraphControl)
            {
                GraphControl c = depobj as GraphControl;
                if (args.NewValue != args.OldValue)
                {
                    GraphControl.Relation.AllRelations.ForEach(t => t.Txt.Foreground = args.NewValue as Brush);
                }
            }
        }

        public static void PeakForegroundChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs args)
        {
            if (depobj is GraphControl)
            {
                GraphControl c = depobj as GraphControl;
                if (args.NewValue != args.OldValue)
                {
                    GraphControl.Peak.AllPeaks.ForEach(t => t.tb.Foreground = args.NewValue as Brush);
                }
            }
        }

        public static void WeightBackgroundChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs args)
        {
            if (depobj is GraphControl)
            {
                GraphControl c = depobj as GraphControl;
                if (args.NewValue != args.OldValue)
                {
                    GraphControl.Relation.AllRelations.ForEach(t => t.Txt.Background = args.NewValue as Brush);
                }
            }
        }

        private static void ManagePartBackgroundChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs args)
        {
            if (depobj is GraphControl)
            {
                GraphControl c = depobj as GraphControl;
                if (args.NewValue != args.OldValue)
                {
                    (((c.Content as Grid).Children[1]) as TabControl).Background = args.NewValue as Brush;
                    //((((c.Content as Grid).Children[1]) as TabControl).Items[0] as TabItem).
                }
            }
        }

        private static void ManagePartTextColorChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs args)
        {
            if (depobj is GraphControl)
            {
                GraphControl c = depobj as GraphControl;
                (((((c.Content as Grid).Children[1]) as TabControl).Items[0]) as TabItem).Style = null;
                (((((c.Content as Grid).Children[1]) as TabControl).Items[1]) as TabItem).Style = null;
                Style s = new Style();
                s.Setters.Add(new Setter(Control.ForegroundProperty, args.NewValue));
                (((((c.Content as Grid).Children[1]) as TabControl).Items[0]) as TabItem).Style = s;
                (((((c.Content as Grid).Children[1]) as TabControl).Items[1]) as TabItem).Style = s;
            }
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
            new ValidateValueCallback(ValidateColor));

            PeakWidthProperty =
            DependencyProperty.Register("PeakWidth", typeof(double), typeof(GraphControl),
            new UIPropertyMetadata(0.1, new PropertyChangedCallback(PeakWidthChanged)), new ValidateValueCallback(ValidatePeakWidth));

            BackgroundProperty =
           DependencyProperty.Register(
           "Background",
           typeof(Brush),
           typeof(GraphControl),
           new UIPropertyMetadata(Brushes.White, 
           new PropertyChangedCallback(BackgroundChanged)),
           new ValidateValueCallback(ValidateColor));

            RelationColorProperty =
           DependencyProperty.Register(
           "RelationColor",
           typeof(Brush),
           typeof(GraphControl),
           new UIPropertyMetadata(Brushes.Green, new PropertyChangedCallback(RelationColorChanged)),
           new ValidateValueCallback(ValidateColor));

            WeightForegroundProperty =
           DependencyProperty.Register(
           "WeightForeground",
           typeof(Brush),
           typeof(GraphControl),
           new UIPropertyMetadata(Brushes.White, new PropertyChangedCallback(WeightForegroundChanged)),
           new ValidateValueCallback(ValidateColor));

            PeakForegroundProperty =
           DependencyProperty.Register(
           "PeakForeground",
           typeof(Brush),
           typeof(GraphControl),
           new UIPropertyMetadata(Brushes.Black, new PropertyChangedCallback(PeakForegroundChanged)),
           new ValidateValueCallback(ValidateColor));

            WeightBackgroundProperty =
           DependencyProperty.Register(
           "WeightBackground",
           typeof(Brush),
           typeof(GraphControl),
           new UIPropertyMetadata(Brushes.Black, new PropertyChangedCallback(WeightBackgroundChanged)),
           new ValidateValueCallback(ValidateColor));

            ManagePartBackgroundProperty =
           DependencyProperty.Register(
           "ManagePartBackground",
           typeof(Brush),
           typeof(GraphControl),
           new UIPropertyMetadata(Brushes.White, new PropertyChangedCallback(ManagePartBackgroundChanged)),
           new ValidateValueCallback(ValidateColor));
            ManagePartTextColorProperty =
           DependencyProperty.Register(
           "ManagePartTextColor",
           typeof(Brush),
           typeof(GraphControl),
           new UIPropertyMetadata(Brushes.Black, new PropertyChangedCallback(ManagePartTextColorChanged)),
           new ValidateValueCallback(ValidateColor));
        }

        #endregion

        #region Сохранение/Загрузка
        public void SaveGraph(string filename)
        {
            SavingParameters sp = new SavingParameters(Peak.AllPeaks, PeakWidth, this);
            sp.SaveGraphField(filename);
        }
        public void LoadGraph(string filename)
        {
            FirstPeak = null;
            SecondPeak = null;
            SavingParameters sp = new SavingParameters(Peak.AllPeaks, PeakWidth, this);
            sp.LoadGraphField(filename);
        }

        #region Классы для сохранения/загрузки
        [Serializable]
        private class SavingParameters
        {
            List<SerializablePeak> SavedPeaks;
            List<SerializableRelation> SavedRelations;
            double PWidth;

            [NonSerialized]
            GraphControl owner;

            public SavingParameters(List<Peak> savedPeaks, double pWidth, GraphControl own)
            {
                SavedRelations = new List<SerializableRelation>();
                SavedPeaks = savedPeaks.Select(t => new SerializablePeak(t, SavedRelations)).ToList();
                PWidth = pWidth;
                owner = own;
            }
            public void SaveGraphField(string filename)
            {
                BinaryFormatter bf = new BinaryFormatter();
                if (filename == "") filename = "Save.gsv";
                using (FileStream FS = new FileStream(filename, FileMode.OpenOrCreate))
                {
                    bf.Serialize(FS, this);
                    MessageBox.Show("Сохранено!");
                }
            }
            public void LoadGraphField(string filename)
            {
                BinaryFormatter bf = new BinaryFormatter();
                if (filename == "") filename = "Save.gsv";
                using (FileStream FS = new FileStream(filename, FileMode.OpenOrCreate))
                {
                    SavingParameters sp = ((SavingParameters)bf.Deserialize(FS));
                    SavedPeaks = sp.SavedPeaks;
                    PWidth = sp.PWidth;
                    SavedRelations = sp.SavedRelations;
                    owner.ClearField();
                    foreach (SerializablePeak i in SavedPeaks)
                    {
                        owner.CreatePeak(new Point(i.Left, i.Top), i.Txt, false);
                    }
                    foreach (SerializableRelation i in SavedRelations)
                    {
                        owner.FirstPeak = Peak.FindByName(i.FromPeak);
                        owner.SecondPeak = Peak.FindByName(i.ToPeak);
                        owner.AddNewRelation(i.Type, i.weight, false);
                    }
                    MessageBox.Show("Вроде как всё загружено!");
                }
            }
            public SerializablePeak FindByName(string name)
            {
                return SavedPeaks.Where(t => t.Txt == name).First();
            }
        }

        [Serializable]
        public class SerializablePeak
        {
            public double Left, Top;
            public string Txt;
            List<SerializableRelation> relations;
            public SerializablePeak(Peak p, List<SerializableRelation> sr)
            {
                Left = Canvas.GetLeft(p.El);
                Top = Canvas.GetTop(p.El);
                Txt = p.Name;
                relations = p.Relations.Where(t => t.FromPeak.Name == Txt).Select(m => new SerializableRelation(m)).ToList();
                if (relations != null) sr.AddRange(relations);
            }
        }

        [Serializable]
        public class SerializableRelation
        {
            public string FromPeak;
            public string ToPeak;
            public double weight;
            //0 - ORIENTED
            //1 - NONORIENTED
            public TypeOfRelation Type;
            public SerializableRelation(Relation r)
            {
                FromPeak = r.FromPeak.Name;
                ToPeak = r.ToPeak.Name;
                Type = r.Type;
                weight = r.Weight;
            }
        }
        #endregion

        #endregion

        private Peak FirstPeak;
        private Peak SecondPeak;
        private TextBox EditingTextBox;
        private Ellipse DraggingEllipse = null;
        public int PeakNum { get; private set; }
        public Mode CurrentMode { get; set; }
        private Mode PreviousMode;
        // Сейчас работает алгоритм?
        private bool isFindingMaxFlow = false;
        // Переменные, которые хранят вершины и анимированную между ними грань
        private Peak ConnectingFirstPeak;
        private Peak ConnectingSecondPeak;
        private object AnimatingEdge;

        public GraphControl()
        {
            InitializeComponent();
            PeakNum = 0;
            PeakWidth = 50;
            SetDefaultColors();
            CurrentMode = Mode.None;
        }



        // Добавить ребро
        public bool AddNewRelation(TypeOfRelation type, double weight = 0, bool isAnimated = true)
        {
            if (FirstPeak != null && SecondPeak != null)
            {
                if (weight < 0) weight = 0;
                Relation rel = new Relation(FirstPeak, SecondPeak, weight, type);
                double X1 = Canvas.GetLeft(rel.FromPeak.El);
                double X2 = Canvas.GetLeft(rel.ToPeak.El);
                double Y1 = Canvas.GetTop(rel.FromPeak.El);
                double Y2 = Canvas.GetTop(rel.ToPeak.El);
                switch (type)
                {
                    case TypeOfRelation.Oriented:
                        FirstPeak.Relations.Add(rel);
                        SecondPeak.Relations.Add(rel);
                        Arrow ar = new Arrow();
                        ar.gc = this;
                        ar.X1 = X1;
                        ar.Y1 = Y1;
                        ar.X2 = X1;
                        ar.Y2 = Y1;
                        ar.HeadHeight = 10;
                        ar.HeadWidth = 20;
                        ar.Stroke = RelationColor;
                        ar.StrokeThickness = 4;
                        rel.LineObj = ar;
                        Canvas.SetZIndex(ar, -1);
                        if (isAnimated)
                        {
                            DoubleAnimation LeftAnim = new DoubleAnimation(X2,
                                                                TimeSpan.FromSeconds(0.3));
                            DoubleAnimation TopAnim = new DoubleAnimation(Y2,
                                                                TimeSpan.FromSeconds(0.3));
                            TopAnim.Completed += Da_Completed;
                            LeftAnim.Completed += Da_Completed;

                            // Анимация по X
                            ar.BeginAnimation(Arrow.X2Property, LeftAnim);
                            // Анимация по Y
                            ar.BeginAnimation(Arrow.Y2Property, TopAnim);

                            // Сохранение объектов, которые взаимодествуют каким либо образом с анимированным объектом
                            AnimatingEdge = ar;
                            ConnectingFirstPeak = FirstPeak;
                            ConnectingSecondPeak = SecondPeak;
                        }
                        else
                        {
                            #region Привязка Первой вершины к ребру

                            // Привязка по x
                            Binding b = new Binding();
                            b.Source = FirstPeak.El;
                            b.Path = new PropertyPath(Canvas.LeftProperty);
                            b.Converter = new LineToCenterEllipseConverter();
                            b.ConverterParameter = PeakWidth;
                            ar.SetBinding(Arrow.X1Property, b);
                            // Привязка по y
                            b = new Binding();
                            b.Source = FirstPeak.El;
                            b.Path = new PropertyPath(Canvas.TopProperty);
                            b.Converter = new LineToCenterEllipseConverter();
                            b.ConverterParameter = PeakWidth;
                            ar.SetBinding(Arrow.Y1Property, b);

                            #endregion

                            #region Привязка Второй вершины к ребру
                            // Привязка по x
                            b = new Binding();
                            b.Source = SecondPeak.El;
                            b.Converter = new LineToCenterEllipseConverter();
                            b.ConverterParameter = PeakWidth;
                            b.Path = new PropertyPath(Canvas.LeftProperty);
                            ar.BeginAnimation(Arrow.X2Property, null);
                            ar.SetBinding(Arrow.X2Property, b);
                            // Привязка по y
                            b = new Binding();
                            b.Source = SecondPeak.El;
                            b.Converter = new LineToCenterEllipseConverter();
                            b.ConverterParameter = PeakWidth;
                            b.Path = new PropertyPath(Canvas.TopProperty);
                            ar.SetBinding(Arrow.Y2Property, b);
                            #endregion
                        }
                        canv.Children.Add(ar);
                        break;
                    case TypeOfRelation.NonOriented:
                        FirstPeak.Relations.Add(rel);
                        SecondPeak.Relations.Add(rel);
                        Line l = new Line();
                        l.X1 = X1;
                        l.Y1 = Y1;
                        l.X2 = X1;
                        l.Y2 = Y1;
                        l.Stroke = RelationColor;
                        l.StrokeThickness = 4;
                        rel.LineObj = l;
                        Canvas.SetZIndex(l, -1);
                        if (isAnimated)
                        {
                            DoubleAnimation LeftAnim = new DoubleAnimation(Canvas.GetLeft(SecondPeak.El),
                                                                TimeSpan.FromSeconds(0.3));
                            DoubleAnimation TopAnim = new DoubleAnimation(Canvas.GetTop(SecondPeak.El),
                                                                TimeSpan.FromSeconds(0.3));
                            TopAnim.Completed += Da_Completed;
                            LeftAnim.Completed += Da_Completed;
                            // Анимация по X
                            l.BeginAnimation(Line.X2Property, LeftAnim);
                            // Анимация по Y
                            l.BeginAnimation(Line.Y2Property, TopAnim);
                            // Сохранение объектов, которые взаимодествуют каким либо образом с анимированным объектом
                            AnimatingEdge = l;
                            ConnectingFirstPeak = FirstPeak;
                            ConnectingSecondPeak = SecondPeak;
                        }
                        else
                        {
                            #region Привязка Первой вершины к ребру
                            // Привязка по x
                            Binding b = new Binding();
                            b.Source = FirstPeak.El;
                            b.Path = new PropertyPath(Canvas.LeftProperty);
                            b.Converter = new LineToCenterEllipseConverter();
                            b.ConverterParameter = PeakWidth;
                            l.SetBinding(Line.X1Property, b);
                            // Привязка по y
                            b = new Binding();
                            b.Source = FirstPeak.El;
                            b.Path = new PropertyPath(Canvas.TopProperty);
                            b.Converter = new LineToCenterEllipseConverter();
                            b.ConverterParameter = PeakWidth;
                            l.SetBinding(Line.Y1Property, b);
                            #endregion

                            #region Привязка Второй вершины к ребру
                            // Привязка по x
                            b = new Binding();
                            b.Source = SecondPeak.El;
                            b.Converter = new LineToCenterEllipseConverter();
                            b.ConverterParameter = PeakWidth;
                            b.Path = new PropertyPath(Canvas.LeftProperty);
                            l.SetBinding(Line.X2Property, b);
                            // Привязка по y
                            b = new Binding();
                            b.Source = SecondPeak.El;
                            b.Converter = new LineToCenterEllipseConverter();
                            b.ConverterParameter = PeakWidth;
                            b.Path = new PropertyPath(Canvas.TopProperty);
                            l.SetBinding(Line.Y2Property, b);
                            #endregion
                        }
                        canv.Children.Add(l);
                        break;
                }
                if (weight >= 0)
                {
                    TextBox tb = new TextBox();
                    tb.IsReadOnly = true;
                    tb.Background = WeightBackground;
                    tb.Foreground = WeightForeground;
                    tb.BorderThickness = new Thickness(0);
                    tb.MouseDoubleClick += Tb_MouseDoubleClick;
                    tb.KeyDown += tb_KeyDown;
                    tb.Text = weight.ToString();                    
                    rel.Txt = tb;
                    Canvas.SetLeft(tb, (X1 + X2) / 2);
                    Canvas.SetTop(tb, (Y1 + Y2) / 2);
                    canv.Children.Add(tb);
                }
                CancelSelectionPeaks();
                OnEdgeCreated(new EventArgs());
                return true;
            }
            else
            {
                return false;
            }
        }
        // Создать вершину
        public void CreatePeak(Point pos, string name = "null value", bool isAnimated = true)
        {
            double x = pos.X;
            double y = pos.Y;
            Ellipse el = new Ellipse();
            el.Margin = new Thickness(-1000000);
            el.Stroke = Brushes.Black;
            el.Fill = PeakColor ?? Brushes.White;
            Canvas.SetLeft(el, x);
            Canvas.SetTop(el, y);
            ////// Создание надписи над эллипсом /////
            TextBox tb = new TextBox();
            tb.IsReadOnly = true;
            tb.Foreground = PeakForeground;
            tb.Background = Brushes.Transparent;
            tb.BorderThickness = new Thickness(0);
            tb.MouseDoubleClick += Tb_MouseDoubleClick;
            tb.KeyDown += tb_KeyDown;
            if (name == "null value")
            {
                tb.Text = PeakNum.ToString();
            }
            else
            {
                tb.Text = name;
            }
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            // Установка зависимости размера шрифта от размеров эллипса
            Binding b = new Binding();
            b.Source = el;
            b.Path = new PropertyPath(WidthProperty);
            b.Converter = new FontSizeConverter();
            tb.SetBinding(FontSizeProperty, b);
            // Привязка координат надписи к координатам эллипсам
            b = new Binding();
            b.Source = el;
            b.Path = new PropertyPath(Canvas.LeftProperty);
            b.Converter = new LeftTextBoxConverter();
            b.ConverterParameter = this;

            tb.SetBinding(Canvas.LeftProperty, b);
            b = new Binding();
            b.Source = el;
            b.Path = new PropertyPath(Canvas.TopProperty);
            b.ConverterParameter = this;
            b.Converter = new TopTextBoxConverter();

            tb.SetBinding(Canvas.TopProperty, b);
            Canvas.SetZIndex(tb, 2);
            // События для перемещения эллипса
            canv.MouseLeave += canv_MouseLeave;
            canv.MouseMove += canv_MouseMove;
            // Добавление созданных эл-ов на canvas
            canv.Children.Add(el);
            canv.Children.Add(tb);

            if (isAnimated)
            {
                // Анимация
                DoubleAnimation da = new DoubleAnimation(0.1, PeakWidth, TimeSpan.FromSeconds(0.3));

                // Запуск анимации
                el.BeginAnimation(Ellipse.WidthProperty, da);
                el.BeginAnimation(Ellipse.HeightProperty, da);
            }
            else
            {
                el.Width = PeakWidth;
                el.Height = PeakWidth;
            }

            // Инкремент счётчика названий
            PeakNum++;
            new Peak(el, tb);
        }
        // Выбрать вершину для соединения
        private void SelectPeak(object sender, MouseButtonEventArgs e)
        {
            DoubleAnimation da = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.3));
            da.AutoReverse = true;
            da.RepeatBehavior = RepeatBehavior.Forever;
            Ellipse ClickedEllipse = null;
            if ((e.Source as FrameworkElement) is Ellipse)
                ClickedEllipse = (e.Source as FrameworkElement) as Ellipse;
            if (ClickedEllipse != null && FirstPeak == null)
            {
                FirstPeak = Peak.FindByEllipse(ClickedEllipse);
                (FirstPeak.El).Stroke = Brushes.Red;
                OnFirstPeakSelected(new EventArgs());
            }
            else if (ClickedEllipse != null && SecondPeak == null)
            {
                SecondPeak = Peak.FindByEllipse(ClickedEllipse);
                (SecondPeak.El as Ellipse).Stroke = Brushes.Red;
                OnSecondPeakSelected(new EventArgs());

                if (AutoWork && Mode.AddEdge == CurrentMode)
                {
                    TheoryDlg dlg = new TheoryDlg(this);
                    dlg.ShowDialog();
                }
                if (isFindingMaxFlow && AutoWork)
                {
                    MessageBox.Show(MaxFlow().ToString());
                    CancelSelectionPeaks();
                    isFindingMaxFlow = false;
                    CurrentMode = PreviousMode;
                }
            }
            else if (ClickedEllipse != null)
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
            if (FirstPeak != null)
            {
                FirstPeak.El.Stroke = Brushes.Black;
                FirstPeak = null;
            }
            if (SecondPeak != null)
            {
                SecondPeak.El.Stroke = Brushes.Black;
                SecondPeak = null;
            }
        }
        // Очистить поле
        public void ClearField()
        {
            canv.Children.Clear();
            Peak.AllPeaks.Clear();
            Relation.AllRelations.Clear();
            FirstPeak = null;
            SecondPeak = null;
            EditingTextBox = null;
            PeakNum = 0;
        }

        // Удаления объектов
        private void DeleteObject(object src)
        {
            if (src is Ellipse)
            {
                Ellipse el = src as Ellipse;
                Peak p = Peak.FindByEllipse(el);
                canv.Children.Remove(el);
                canv.Children.Remove(p.tb);
                while (p.Relations.Count > 0)
                {
                    canv.Children.Remove(p.Relations[0].LineObj);
                    canv.Children.Remove(p.Relations[0].Txt);
                    Relation.AllRelations.Remove(p.Relations[0]);
                    p.Relations.RemoveAt(0);
                }
                Peak.AllPeaks.Remove(p);
                if (FirstPeak == p) FirstPeak = null;
                if (SecondPeak == p) SecondPeak = null;
            }
            else if (src is Line || src is Arrow)
            {
                Relation r = Relation.FindByLine(src as FrameworkElement);
                canv.Children.Remove(r.LineObj);
                canv.Children.Remove(r.Txt);
                Relation.AllRelations.Remove(r);
                r.FromPeak.Relations.Remove(r);
                r.ToPeak.Relations.Remove(r);

            }
        }


        #region Обработчики событий компонента
        private void canv_MouseLeave(object sender, MouseEventArgs e)
        {
            DraggingEllipse = null;
        }
        private void Canv_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            switch (CurrentMode)
            {
                case Mode.AddPeak:
                    CreatePeak(e.GetPosition(canv));
                    OnPeakCreated(new EventArgs());
                    break;
                case Mode.AddEdge:
                    SelectPeak(sender, e);
                    break;
                case Mode.DeleteSelected:
                    DeleteObject(e.Source);
                    break;
                case Mode.SelectForAlghorithm:
                    SelectPeak(sender, e);
                    break;
                case Mode.MoveObjects:
                    DraggingEllipse = e.Source as Ellipse;
                    break;
                default:
                    break;
            }
        }
        private void canv_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            DraggingEllipse = null;
        }
        private void canv_MouseMove(object sender, MouseEventArgs e)
        {
            if (DraggingEllipse != null && CurrentMode == Mode.MoveObjects)
            {
                double x = e.GetPosition(canv).X;
                double y = e.GetPosition(canv).Y;
                // После анимаций на свойства блокируется, пока анимация не исчезнет
                DraggingEllipse.BeginAnimation(Canvas.LeftProperty, null);
                DraggingEllipse.BeginAnimation(Canvas.TopProperty, null);
                Canvas.SetLeft(DraggingEllipse, x);
                Canvas.SetTop(DraggingEllipse, y);
                // Для правильного отображения анимации при смене координат вершины
                if (Peak.FindByEllipse(DraggingEllipse) == ConnectingSecondPeak)
                {
                    DoubleAnimation LeftAnim = new DoubleAnimation(Canvas.GetLeft(ConnectingSecondPeak.El),
                                                TimeSpan.FromSeconds(0.3));
                    DoubleAnimation TopAnim = new DoubleAnimation(Canvas.GetTop(ConnectingSecondPeak.El),
                                                    TimeSpan.FromSeconds(0.3));
                    if (AnimatingEdge is Line)
                    {
                        (AnimatingEdge as Line).BeginAnimation(Line.X2Property, LeftAnim);
                        (AnimatingEdge as Line).BeginAnimation(Line.Y2Property, TopAnim);
                    }
                    else if (AnimatingEdge is Arrow)
                    {
                        (AnimatingEdge as Arrow).BeginAnimation(Arrow.X2Property, LeftAnim);
                        (AnimatingEdge as Arrow).BeginAnimation(Arrow.Y2Property, TopAnim);
                    }
                }
                // Изменяет положение всех надписей с весом, относительно грани
                foreach (Relation rel in Peak.FindByEllipse(DraggingEllipse).Relations)
                {
                    if (rel.Txt != null)
                    {
                        double X1 = Canvas.GetLeft(rel.FromPeak.El);
                        double X2 = Canvas.GetLeft(rel.ToPeak.El);
                        double Y1 = Canvas.GetTop(rel.FromPeak.El);
                        double Y2 = Canvas.GetTop(rel.ToPeak.El);
                        Canvas.SetLeft(rel.Txt, (X1 + X2) / 2);
                        Canvas.SetTop(rel.Txt, (Y1 + Y2) / 2);
                    }
                }
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

        // Событие происходящее по окончанию анимации соединения вершмн
        private void Da_Completed(object sender, EventArgs e)
        {
            // Привязка ребра к вершинам
            if (AnimatingEdge is Line)
            {
                Line l = AnimatingEdge as Line;
                l.BeginAnimation(Line.X2Property, null);
                l.BeginAnimation(Line.Y2Property, null);
                #region Привязка Первой вершины к ребру
                // Привязка по x
                Binding b = new Binding();
                b.Source = ConnectingFirstPeak.El;
                b.Path = new PropertyPath(Canvas.LeftProperty);
                b.Converter = new LineToCenterEllipseConverter();
                b.ConverterParameter = PeakWidth;
                l.SetBinding(Line.X1Property, b);
                // Привязка по y
                b = new Binding();
                b.Source = ConnectingFirstPeak.El;
                b.Path = new PropertyPath(Canvas.TopProperty);
                b.Converter = new LineToCenterEllipseConverter();
                b.ConverterParameter = PeakWidth;
                l.SetBinding(Line.Y1Property, b);
                #endregion

                #region Привязка Второй вершины к ребру
                // Привязка по x
                b = new Binding();
                b.Source = ConnectingSecondPeak.El;
                b.Converter = new LineToCenterEllipseConverter();
                b.ConverterParameter = PeakWidth;
                b.Path = new PropertyPath(Canvas.LeftProperty);
                l.SetBinding(Line.X2Property, b);
                // Привязка по y
                b = new Binding();
                b.Source = ConnectingSecondPeak.El;
                b.Converter = new LineToCenterEllipseConverter();
                b.ConverterParameter = PeakWidth;
                b.Path = new PropertyPath(Canvas.TopProperty);
                l.SetBinding(Line.Y2Property, b);
                #endregion
            }

            // Привязка линии со стрелкой
            if (AnimatingEdge is Arrow)
            {
                Arrow ar = AnimatingEdge as Arrow;
                ar.BeginAnimation(Arrow.X2Property, null);
                ar.BeginAnimation(Arrow.Y2Property, null);
                #region Привязка Первой вершины к ребру

                // Привязка по x
                Binding b = new Binding();
                b.Source = ConnectingFirstPeak.El;
                b.Path = new PropertyPath(Canvas.LeftProperty);
                b.Converter = new LineToCenterEllipseConverter();
                b.ConverterParameter = PeakWidth;
                ar.SetBinding(Arrow.X1Property, b);
                // Привязка по y
                b = new Binding();
                b.Source = ConnectingFirstPeak.El;
                b.Path = new PropertyPath(Canvas.TopProperty);
                b.Converter = new LineToCenterEllipseConverter();
                b.ConverterParameter = PeakWidth;
                ar.SetBinding(Arrow.Y1Property, b);

                #endregion

                #region Привязка Второй вершины к ребру
                // Привязка по x
                b = new Binding();
                b.Source = ConnectingSecondPeak.El;
                b.Converter = new LineToCenterEllipseConverter();
                b.ConverterParameter = PeakWidth;
                b.Path = new PropertyPath(Canvas.LeftProperty);
                ar.BeginAnimation(Arrow.X2Property, null);
                ar.SetBinding(Arrow.X2Property, b);
                // Привязка по y
                b = new Binding();
                b.Source = ConnectingSecondPeak.El;
                b.Converter = new LineToCenterEllipseConverter();
                b.ConverterParameter = PeakWidth;
                b.Path = new PropertyPath(Canvas.TopProperty);
                ar.SetBinding(Arrow.Y2Property, b);
                #endregion
            }

            ConnectingFirstPeak = null;
            ConnectingSecondPeak = null;
            AnimatingEdge = null;
        }

        // Убрать каретку с названия текстбокса
        void UnfocusEditingTextBox(MouseButtonEventArgs e)
        {
            if (EditingTextBox != null && e?.Source != EditingTextBox)
            {
                Peak tb = Peak.FindByTextBox(EditingTextBox);
                if (tb != null)
                {
                    if (!tb.SetName(EditingTextBox.Text))
                    {
                        MessageBox.Show("Это имя уже занято");
                        EditingTextBox.Undo();
                    }
                }
                else
                {
                    Relation r = Relation.FindByWeightTextBox(EditingTextBox);
                    try
                    {
                        double buf = double.Parse(r.Txt.Text);
                        r.Weight = buf;
                    }
                    catch
                    {
                        MessageBox.Show("Введите корректное число");
                        EditingTextBox.Undo();
                    }
                }
                EditingTextBox.IsReadOnly = true;
                EditingTextBox.Select(0, 0);
                EditingTextBox = null;
            }
        }

        #region События для автономной работы

        private void addPeak_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentMode != Mode.AddPeak) CurrentMode = Mode.AddPeak;
        }
        private void addEdge_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentMode != Mode.AddEdge) CurrentMode = Mode.AddEdge;
        }
        private void DelObj_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentMode != Mode.DeleteSelected) CurrentMode = Mode.DeleteSelected;
        }
        private void SaveBTN_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Graph Save | *.gsv";
            if (sfd.ShowDialog() == true)
            {
                SaveGraph(sfd.FileName);
            }
        }
        private void LoadBTN_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Graph Save | *.gsv";
            if (ofd.ShowDialog() == true)
            {
                LoadGraph(ofd.FileName);
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            PreviousMode = CurrentMode;
            CurrentMode = Mode.SelectForAlghorithm;
            isFindingMaxFlow = true;
        }
        private void CancelSelectionPeaksBTN_Click(object sender, RoutedEventArgs e)
        {
            CancelSelectionPeaks();
        }
        private void ClearFieldBTN_Click(object sender, RoutedEventArgs e)
        {
            ClearField();
        }
        private void MoveObj_Click(object sender, RoutedEventArgs e)
        {
            CurrentMode = Mode.MoveObjects;
        }

        #endregion

        #endregion

        #region Алгоритмы
        public double MaxFlow()
        {
            Peak source = FirstPeak;
            Peak destination = SecondPeak;
            List<List<Relation>> AllWays = new List<List<Relation>>();
            // Насыщенные грани
            List<Relation> SaturatedRelations = new List<Relation>();
            //Вес граней во время работы алгоритма
            List<double> RealRelationsWeight = new List<double>();
            RealRelationsWeight = Relation.AllRelations.Select(t => t.Weight).ToList();

            List<Relation> Way = null;
            double SumSaturatedRelations = 0;
            do
            {
                Way = FindWay(ref source, ref destination);
                if (Way != null)
                {
                    double min = Way.Min(t => RealRelationsWeight[Relation.AllRelations.IndexOf(t)]);
                    SumSaturatedRelations += min;
                    AllWays.Add(Way);
                    foreach (Relation r in Way)
                    {
                        int i = Relation.AllRelations.IndexOf(r);
                        RealRelationsWeight[i] -= min;
                    }
                    SaturatedRelations.AddRange(Way.Where(t => t.Weight == min).ToList());
                }
            }
            while (Way != null);

            // Функция по нахождению одного пути из истока в сток
            List<Relation> FindWay(ref Peak src, ref Peak dst)
            {
                bool isEnded = false;
                //Путь по которому 'прошёл' алгоритм
                List<Relation> CurWay = new List<Relation>();
                //соединения, которые не ведут к стоку
                List<Relation> CheckedRelations = new List<Relation>();
                Peak CheckingPeak = src;
                while (!isEnded)
                {
                    // В outputRel хранятся ненасыщенные соединения, которые ещё не были пройдены и
                    List<Relation> outputRel = CheckingPeak.GetOutputRelations().Where(t =>
                                                                                            !SaturatedRelations.Contains(t) &&
                                                                                            !CurWay.Contains(t) &&
                                                                                            !CheckedRelations.Contains(t)).ToList();

                    if (CheckingPeak == src && outputRel.Count == 0 && CurWay.Count == 0)
                    {
                        isEnded = true;
                    }

                    if (outputRel.Count != 0)
                    {
                        CurWay.Add(outputRel[0]);
                        CheckingPeak = CurWay.Last().FromPeak == CheckingPeak ? CurWay.Last().ToPeak : CurWay.Last().FromPeak;
                    }
                    else
                    {
                        if (CurWay.Count != 0)
                        {
                            CheckedRelations.Add(CurWay.Last());
                            CheckingPeak = CurWay.Last().FromPeak == CheckingPeak ? CurWay.Last().ToPeak : CurWay.Last().FromPeak;
                            CurWay.Remove(CurWay.Last());
                        }
                    }
                    if (CheckingPeak == dst)
                    {
                        return CurWay;
                    }
                }
                return null;
            }

            return SumSaturatedRelations;
        }
        #endregion

        #region классы Peak/Relation
        public class Peak
        {
            public static List<Peak> AllPeaks { get; private set; } = new List<Peak>();

            public Ellipse El { get; set; }
            public TextBox tb { get; set; }
            public string Name { get; set; }
            public List<Relation> Relations { get; set; }

            public Peak(Ellipse el, TextBox name)
            {
                El = el;
                tb = name;
                Name = name.Text;
                Relations = new List<Relation>();
                AllPeaks.Add(this);
            }
            public bool SetName(string name)
            {
                Peak p = FindByName(name);
                if (p == null || name == Name)
                {
                    Name = name;
                    return true;
                }
                return false;
            }
            public List<Relation> GetOutputRelations()
            {
                return Relations.Where(t => ((t.Type == TypeOfRelation.NonOriented) || (t.Type == TypeOfRelation.Oriented && t.FromPeak == this))).ToList();
            }

            public static Peak FindByEllipse(Ellipse ElForSearch) => AllPeaks.Where(t => t.El == ElForSearch).FirstOrDefault();
            public static Peak FindByTextBox(TextBox TbForSearch) => AllPeaks.Where(t => t.tb == TbForSearch).FirstOrDefault();
            public static Peak FindByName(string findingName) => AllPeaks.Where(t => t.Name == findingName).FirstOrDefault();
        }
        public class Relation
        {
            public static List<Relation> AllRelations { get; private set; } = new List<Relation>() {  };
            public double Weight { get; set; }
            public Peak FromPeak { get; set; }
            public Peak ToPeak { get; set; }
            public TextBox Txt { get; set; }
            public TypeOfRelation Type { get; set; }
            public FrameworkElement LineObj { get; set; }

            public Relation(Peak from, Peak to, double weight, TypeOfRelation type, FrameworkElement line = null, TextBox txt = null)
            {
                FromPeak = from;
                ToPeak = to;
                Weight = weight;
                Type = type;
                Txt = txt;
                LineObj = line;
                AllRelations.Add(this);
            }

            public static Relation FindByLine(FrameworkElement line) => AllRelations.Where(t => t.LineObj == line).FirstOrDefault();

            public static Relation FindByWeightTextBox(TextBox labelForSearch) => AllRelations.Where(t => t.Txt == labelForSearch).FirstOrDefault();
        }
        #endregion

        #region Управление цветами компонента

        private void SetDefaultColors()
        {
            clrPeak.selectedColor = PeakColor;
            clrTextPeak.selectedColor = PeakForeground;
            clrField.selectedColor = Background;
            clrManagePart.selectedColor = ManagePartBackground;
            clrTextManagePart.selectedColor = ManagePartTextColor;
            clrRelation.selectedColor = RelationColor;
            clrTextRelation.selectedColor = WeightForeground;
            clrBackgroundTextRelation.selectedColor = WeightBackground;

        }

        #region События смены цвета ColorPicker-в
        private void clrPeak_ColorChanged(object sender, ColorArgs e)
        {
            PeakColor = e.NewColor;
        }

        private void clrTextPeak_ColorChanged(object sender, ColorArgs e)
        {
            PeakForeground = e.NewColor;
        }

        private void clrRelation_ColorChanged(object sender, ColorArgs e)
        {
            RelationColor = e.NewColor;
        }

        private void clrTextRelation_ColorChanged(object sender, ColorArgs e)
        {
            WeightForeground = e.NewColor;
        }

        private void clrField_ColorChanged(object sender, ColorArgs e)
        {
            Background = e.NewColor;
        }

        private void clrManagePart_ColorChanged(object sender, ColorArgs e)
        {
            ManagePartBackground = e.NewColor;
        }

        private void clrBackgroundTextRelation_ColorChanged(object sender, ColorArgs e)
        {
            WeightBackground = e.NewColor;
        }

        private void clrTextManagePart_ColorChanged(object sender, ColorArgs e)
        {
            ManagePartTextColor = e.NewColor;
        }

        #endregion

        #region События, срабатывающие при наведении курсора на цвет
        private void clrPeak_ColorEnter(object sender, ColorArgs e)
        {
            PeakColor = e.NewColor;
        }

        private void clrTextPeak_ColorEnter(object sender, ColorArgs e)
        {
            PeakForeground = e.NewColor;
        }

        private void clrRelation_ColorEnter(object sender, ColorArgs e)
        {
            RelationColor = e.NewColor;
        }

        private void clrTextRelation_ColorEnter(object sender, ColorArgs e)
        {
            WeightForeground = e.NewColor;
        }

        private void clrBackgroundTextRelation_ColorEnter(object sender, ColorArgs e)
        {
            WeightBackground = e.NewColor;
        }

        private void clrField_ColorEnter(object sender, ColorArgs e)
        {
            Background = e.NewColor;
        }

        private void clrManagePart_ColorEnter(object sender, ColorArgs e)
        {
            ManagePartBackground = e.NewColor;
        }

        private void clrTextManagePart_ColorEnter(object sender, ColorArgs e)
        {
            ManagePartTextColor = e.NewColor;
        }
        #endregion

        #region События, срабатывающие, когда пользователь уводит курсора с цвета
        private void clrPeak_ColorLeave(object sender, ColorArgs e)
        {
            PeakColor = e.OldColor;
        }

        private void clrTextPeak_ColorLeave(object sender, ColorArgs e)
        {
            PeakForeground = e.OldColor;
        }

        private void clrRelation_ColorLeave(object sender, ColorArgs e)
        {
            RelationColor = e.OldColor;
        }

        private void clrTextRelation_ColorLeave(object sender, ColorArgs e)
        {
            WeightForeground = e.OldColor;
        }

        private void clrBackgroundTextRelation_ColorLeave(object sender, ColorArgs e)
        {
            WeightBackground = e.OldColor;
        }

        private void clrField_ColorLeave(object sender, ColorArgs e)
        {
            Background = e.OldColor;
        }

        private void clrManagePart_ColorLeave(object sender, ColorArgs e)
        {
            ManagePartBackground = e.OldColor;
        }

        private void clrTextManagePart_ColorLeave(object sender, ColorArgs e)
        {
            ManagePartTextColor = e.OldColor;
        }
        #endregion

        #endregion
    }
}
