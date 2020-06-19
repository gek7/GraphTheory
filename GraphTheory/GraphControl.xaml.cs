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
    // перечисление режимов компонента
    public enum Mode
    {
        AddPeak,
        AddEdge,
        DeleteSelected,
        SelectForAlghorithm,
        MoveObjects,
        None
    }
    // Тип связи ребра
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
        #region Аргументы для событий и новые "Держатели" для методов событий
        public delegate void PeakHandler(object sender, PeakArgs e);
        // PeakArgs хранит только одну вершину
        public class PeakArgs : EventArgs
        {
            public Peak Peak { get; set; }
            public PeakArgs(Peak peak)
            {
                Peak = peak;
            }
        }

        public delegate void NewPeakHandler(object sender, NewPeakArgs e);
        //NewPeakArgs хранит новую выбраную вершину и ту, что была до неё.
        public class NewPeakArgs : EventArgs
        {
            public Peak NewSelectingPeak { get; set; }
            public Peak OldSelectedPeak { get; set; }
            public NewPeakArgs(Peak oldPeak, Peak newPeak)
            {
                NewSelectingPeak = newPeak;
                OldSelectedPeak = oldPeak;
            }
        }

        public delegate void RelationHandler(object sender, RelationArgs e);
        public class RelationArgs : EventArgs
        {
            public Relation NewRelation { get; set; }
            public RelationArgs(Relation lineObj)
            {
                NewRelation = lineObj;
            }
        }
        #endregion

        #region События для прикладного программиста
        public event PeakHandler FirstPeakSelected;
        public event PeakHandler SecondPeakSelected;
        public event NewPeakHandler NewFirstPeakSelected;
        public event RelationHandler RelationCreated;
        public event PeakHandler PeakCreated;
        public event PeakHandler PeakDeleted;
        public event RelationHandler RelationDeleted;
        #endregion

        #region Методы для вызова обработчиков событий, указанных прикладным программистом
        protected virtual void OnFirstPeakSelected(PeakArgs e)
        {
            PeakHandler handler = FirstPeakSelected;
            handler?.Invoke(this, e);
        }
        protected virtual void OnSecondPeakSelected(PeakArgs e)
        {
            PeakHandler handler = SecondPeakSelected;
            handler?.Invoke(this, e);
        }
        protected virtual void OnNewFirstPeakSelected(NewPeakArgs e)
        {
            NewPeakHandler handler = NewFirstPeakSelected;
            handler?.Invoke(this, e);
        }
        protected virtual void OnEdgeCreated(RelationArgs e)
        {
            RelationHandler handler = RelationCreated;
            handler?.Invoke(this, e);
        }
        protected virtual void OnPeakCreated(PeakArgs e)
        {
            PeakHandler handler = PeakCreated;
            handler?.Invoke(this, e);
        }
        protected virtual void OnPeakDeleted(PeakArgs e)
        {
            PeakHandler handler = PeakDeleted;
            handler?.Invoke(this, e);
        }
        protected virtual void OnRelationDeleted(RelationArgs e)
        {
            RelationHandler handler = RelationDeleted;
            handler?.Invoke(this, e);
        }
        #endregion

        #region Обычные CLR свойства
        private bool _AutoWork = true;
        //Автономная работа компонента (Да/Нет)
        public virtual bool AutoWork
        {
            get
            {
                return _AutoWork;
            }
            set
            {
                if (value != _AutoWork)
                {
                    // Скрывает или показывает управляющую часть компонента
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
        public static readonly DependencyProperty RelationWidthProperty;
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
            set
            {
                SetValue(PeakColorProperty, value);
            }
        }

        public double PeakWidth
        {
            get { return (double)GetValue(PeakWidthProperty); }
            set
            {
                SetValue(PeakWidthProperty, value);
            }
        }

        public double RelationWidth
        {
            get { return (double)GetValue(RelationWidthProperty); }
            set
            {
                SetValue(RelationWidthProperty, value);
            }
        }

        public new Brush Background
        {
            get { return (Brush)GetValue(BackgroundProperty); }
            set
            {
                SetValue(BackgroundProperty, value);
            }
        }

        public Brush RelationColor
        {
            get { return (Brush)GetValue(RelationColorProperty); }
            set
            {
                SetValue(RelationColorProperty, value);
            }
        }

        public Brush WeightForeground
        {
            get { return (Brush)GetValue(WeightForegroundProperty); }
            set
            {
                SetValue(WeightForegroundProperty, value);
            }
        }

        public Brush PeakForeground
        {
            get { return (Brush)GetValue(PeakForegroundProperty); }
            set
            {
                SetValue(PeakForegroundProperty, value);
            }
        }

        public Brush WeightBackground
        {
            get { return (Brush)GetValue(WeightBackgroundProperty); }
            set
            {
                SetValue(WeightBackgroundProperty, value);
            }
        }

        private Brush ManagePartBackground
        {
            get { return (Brush)GetValue(ManagePartBackgroundProperty); }
            set
            {
                SetValue(ManagePartBackgroundProperty, value);
            }
        }

        private Brush ManagePartTextColor
        {
            get { return (Brush)GetValue(ManagePartTextColorProperty); }
            set
            {
                if (!(value is Brush))
                {
                    value = Brushes.Black;
                }
                SetValue(ManagePartTextColorProperty, value);
            }
        }
        #endregion

        #region Методы для изменения значений для свойств зависимости
        public static void PeakColorChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs args)
        {
            if (depobj is GraphControl)
            {
                // c - компонент, чьё свойство меняет значение
                GraphControl c = depobj as GraphControl;

                // Перебираем все вершины компонента и присваиваем новый цвет их эллипсам
                foreach (var item in c.AllPeaks)
                {
                        item.El.Fill = (Brush)args.NewValue;
                }
            }
        }

        public static void PeakWidthChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs args)
        {
            double value = (double)args.NewValue;
            GraphControl gp = (depobj as GraphControl);
            foreach (var item in gp.AllPeaks)
            {
                    Ellipse el = item.El;
                    TextBox tb = item.Tb;
                    // Сохранение привязок до изменения значений вручную
                    BindingExpression LeftBind = tb.GetBindingExpression(Canvas.LeftProperty);
                    BindingExpression TopBind = tb.GetBindingExpression(Canvas.TopProperty);
                    double left = Canvas.GetLeft(el);
                    double top = Canvas.GetTop(el);
                    // Изменение значения вручную
                    Canvas.SetLeft(tb, left + gp.PeakWidth / 2);
                    Canvas.SetTop(tb, top - gp.PeakWidth);
                    // Запуск анимации изменения размера
                    DoubleAnimation da = new DoubleAnimation(value, TimeSpan.FromSeconds(0.3));
                    el.BeginAnimation(Ellipse.WidthProperty, da);
                    el.BeginAnimation(Ellipse.HeightProperty, da);
                    tb.SetBinding(Canvas.LeftProperty, LeftBind.ParentBindingBase);
                    tb.SetBinding(Canvas.TopProperty, TopBind.ParentBindingBase);
            }
        }

        public static void RelationWidthChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs args)
        {
            double value = (double)args.NewValue;
            GraphControl gp = depobj as GraphControl;
            //перебор всех рёбер и изменение их ширины на новое значение
            foreach (var item in gp.AllRelations)
            {
                if (item.LineObj is Line)
                {
                    (item.LineObj as Line).StrokeThickness = value;
                }
                else if (item.LineObj is Arrow)
                {
                    (item.LineObj as Arrow).StrokeThickness = value;
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
                    c.canv.Background = (args.NewValue as Brush);
                }
            }
        }

        public static void RelationColorChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs args)
        {
            GraphControl gp = depobj as GraphControl;
            if (args.NewValue != args.OldValue)
            {
                // Перебор всех рёбер и изменение их цвета
                gp.AllRelations.ForEach(t => (t.LineObj as Shape).Stroke = (args.NewValue as Brush));
            }
        }

        public static void WeightForegroundChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs args)
        {
            if (depobj is GraphControl)
            {
                GraphControl gp = depobj as GraphControl;
                if (args.NewValue != args.OldValue)
                {
                    gp.AllRelations.ForEach(t => t.Txt.Foreground = args.NewValue as Brush);
                }
            }
        }

        public static void PeakForegroundChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs args)
        {
            if (depobj is GraphControl)
            {
                GraphControl gp = depobj as GraphControl;
                if (args.NewValue != args.OldValue)
                {
                    gp.AllPeaks.ForEach(t => t.Tb.Foreground = args.NewValue as Brush);
                }
            }
        }

        public static void WeightBackgroundChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs args)
        {
            if (depobj is GraphControl)
            {
                GraphControl gp = depobj as GraphControl;
                if (args.NewValue != args.OldValue)
                {
                    gp.AllRelations.ForEach(t => t.Txt.Background = args.NewValue as Brush);
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
                    c.exp.Background = args.NewValue as Brush;
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

        #region Методы для исправления неправильных значений для свойств зависимости
        public static object CoerceColor(DependencyObject depObj, object color)
        {
            if (!(color is Brush) || color == null)
            {
                return Brushes.Black;
            }
            return color;
        }

        public static object CoerceWidth(DependencyObject depObj, object width)
        {
            double value = (double)width;
            if (value <= 0)
            {
                return (double)1;
            }
            if (value > 35000)
            {
                return (double)35000;
            }
            return value;
        }
        #endregion

        static GraphControl()
        {
            // Далее идёт регистрация всех свойств зависимости
            PeakColorProperty =
            DependencyProperty.Register(
            "PeakColor",
            typeof(Brush),
            typeof(GraphControl),
            new UIPropertyMetadata(Brushes.White, new PropertyChangedCallback(PeakColorChanged), new CoerceValueCallback(CoerceColor)));

            PeakWidthProperty =
            DependencyProperty.Register("PeakWidth", typeof(double), typeof(GraphControl),
            new UIPropertyMetadata((double)10, new PropertyChangedCallback(PeakWidthChanged), new CoerceValueCallback(CoerceWidth)));

            RelationWidthProperty =
            DependencyProperty.Register("RelationWidth", typeof(double), typeof(GraphControl),
            new UIPropertyMetadata((double)3, new PropertyChangedCallback(RelationWidthChanged), new CoerceValueCallback(CoerceWidth)));

            BackgroundProperty =
           DependencyProperty.Register(
           "Background",
           typeof(Brush),
           typeof(GraphControl),
           new UIPropertyMetadata(Brushes.White,
           new PropertyChangedCallback(BackgroundChanged), new CoerceValueCallback(CoerceColor)));

            RelationColorProperty =
           DependencyProperty.Register(
           "RelationColor",
           typeof(Brush),
           typeof(GraphControl),
           new UIPropertyMetadata(Brushes.Green, new PropertyChangedCallback(RelationColorChanged), new CoerceValueCallback(CoerceColor)));

            WeightForegroundProperty =
           DependencyProperty.Register(
           "WeightForeground",
           typeof(Brush),
           typeof(GraphControl),
           new UIPropertyMetadata(Brushes.White, new PropertyChangedCallback(WeightForegroundChanged), new CoerceValueCallback(CoerceColor)));

            PeakForegroundProperty =
           DependencyProperty.Register(
           "PeakForeground",
           typeof(Brush),
           typeof(GraphControl),
           new UIPropertyMetadata(Brushes.Black, new PropertyChangedCallback(PeakForegroundChanged), new CoerceValueCallback(CoerceColor)));

            WeightBackgroundProperty =
           DependencyProperty.Register(
           "WeightBackground",
           typeof(Brush),
           typeof(GraphControl),
           new UIPropertyMetadata(Brushes.Black, new PropertyChangedCallback(WeightBackgroundChanged), new CoerceValueCallback(CoerceColor)));

            ManagePartBackgroundProperty =
           DependencyProperty.Register(
           "ManagePartBackground",
           typeof(Brush),
           typeof(GraphControl),
           new UIPropertyMetadata(Brushes.White, new PropertyChangedCallback(ManagePartBackgroundChanged), new CoerceValueCallback(CoerceColor)));
            ManagePartTextColorProperty =
           DependencyProperty.Register(
           "ManagePartTextColor",
           typeof(Brush),
           typeof(GraphControl),
           new UIPropertyMetadata(Brushes.Black, new PropertyChangedCallback(ManagePartTextColorChanged), new CoerceValueCallback(CoerceColor)));
        }

        #endregion

        #region Сохранение/Загрузка
        public void SaveGraph(string filename)
        {
            SavingParameters sp = new SavingParameters(AllPeaks, PeakWidth, this);
            sp.SaveGraphField(filename);
        }
        public void LoadGraph(string filename)
        {
            FirstPeak = null;
            SecondPeak = null;
            SavingParameters sp = new SavingParameters(AllPeaks, PeakWidth, this);
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
                if (File.Exists(filename))
                {
                    using (FileStream FS = new FileStream(filename, FileMode.OpenOrCreate))
                    {
                        owner.ClearField();
                        SavingParameters sp = ((SavingParameters)bf.Deserialize(FS));
                        SavedPeaks = sp.SavedPeaks;
                        PWidth = sp.PWidth;
                        SavedRelations = sp.SavedRelations;
                        foreach (SerializablePeak i in SavedPeaks)
                        {
                            owner.CreatePeak(new Point(i.Left, i.Top), i.Txt, false);
                        }
                        foreach (SerializableRelation i in SavedRelations)
                        {
                            Peak FirstPk = owner.FindPeakByName(i.FromPeak);
                            Peak SecondPk = owner.FindPeakByName(i.ToPeak);
                            owner.AddNewRelation(FirstPk, SecondPk, i.Type, i.weight, false);
                        }
                        MessageBox.Show("Вроде как всё загружено!");
                    }
                }
            }
            public SerializablePeak FindByName(string name)
            {
                return SavedPeaks.Where(t => t.Txt == name).First();
            }
        }

        [Serializable]
        private class SerializablePeak
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
        private class SerializableRelation
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

        //Список всех вершин
        public List<Peak> AllPeaks { get; private set; } = new List<Peak>();
        //Список всех рёбер
        public List<Relation> AllRelations { get; private set; } = new List<Relation>();

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
        public virtual bool AddNewRelation(Peak FirstPeak, Peak SecondPeak, TypeOfRelation type, double weight = 0, bool isAnimated = true)
        {
            if ((FirstPeak != null && SecondPeak != null) && (FirstPeak != SecondPeak))
            {
                if (weight < 0) weight = 0;
                if (weight > 35000) weight = 35000;
                double X1 = Canvas.GetLeft(FirstPeak.El);
                double X2 = Canvas.GetLeft(SecondPeak.El);
                double Y1 = Canvas.GetTop(FirstPeak.El);
                double Y2 = Canvas.GetTop(SecondPeak.El);
                FrameworkElement FrEl = null;
                switch (type)
                {
                    case TypeOfRelation.Oriented:
                        Arrow ar = new Arrow();
                        FrEl = ar;
                        ar.gc = this;
                        ar.X1 = X1;
                        ar.Y1 = Y1;
                        ar.X2 = X1;
                        ar.Y2 = Y1;
                        ar.HeadHeight = 10;
                        ar.HeadWidth = 20;
                        ar.Stroke = RelationColor;
                        ar.StrokeThickness = 4;
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
                        Line l = new Line();
                        FrEl = l;
                        l.X1 = X1;
                        l.Y1 = Y1;
                        l.X2 = X1;
                        l.Y2 = Y1;
                        l.Stroke = RelationColor;
                        l.StrokeThickness = 4;
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

                TextBox tb = new TextBox();
                tb.IsReadOnly = true;
                tb.Background = WeightBackground;
                tb.Foreground = WeightForeground;
                tb.BorderThickness = new Thickness(0);
                tb.MouseDoubleClick += Tb_MouseDoubleClick;
                tb.KeyDown += tb_KeyDown;
                tb.Text = weight.ToString();
                Canvas.SetLeft(tb, (X1 + X2) / 2);
                Canvas.SetTop(tb, (Y1 + Y2) / 2);
                canv.Children.Add(tb);
                Relation rel = new Relation(FirstPeak, SecondPeak, weight, type, FrEl, tb);
                AllRelations.Add(rel);
                FirstPeak.Relations.Add(rel);
                SecondPeak.Relations.Add(rel);
                OnEdgeCreated(new RelationArgs(rel));
                return true;
            }
            else
            {
                return false;
            }
        }
        // Создать вершину
        public virtual void CreatePeak(Point pos, string name = "null value", bool isAnimated = true)
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
                PeakNum++;
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

            Peak p = new Peak(el, tb, this);
            AllPeaks.Add(p);
            OnPeakCreated(new PeakArgs(p));
        }
        // Выбрать вершину для соединения
        public virtual void SelectPeak(Peak SelectingPeak)
        {
            if (SelectingPeak != null && FirstPeak == null)
            {
                FirstPeak = SelectingPeak;
                (FirstPeak.El).Stroke = Brushes.Red;
                OnFirstPeakSelected(new PeakArgs(FirstPeak));
            }
            else if (SelectingPeak != null && SecondPeak == null)
            {
                SecondPeak = SelectingPeak;
                (SecondPeak.El as Ellipse).Stroke = Brushes.Red;
                OnSecondPeakSelected(new PeakArgs(SecondPeak));

                if (AutoWork && Mode.AddEdge == CurrentMode)
                {
                    TheoryDlg dlg = new TheoryDlg(this, FirstPeak, SecondPeak);
                    dlg.ShowDialog();
                }
                if (isFindingMaxFlow)
                {
                    MessageBox.Show(MaxFlow(FirstPeak, SecondPeak).ToString());
                    CancelSelectionPeaks();
                    isFindingMaxFlow = false;
                    CurrentMode = PreviousMode;
                }
            }
            else if (SelectingPeak != null)
            {
                OnNewFirstPeakSelected(new NewPeakArgs(FirstPeak, SelectingPeak));
                CancelSelectionPeaks();
                FirstPeak = SelectingPeak;
                (FirstPeak.El as Ellipse).Stroke = Brushes.Red;
            }
        }
        // Отменить выбор вершин
        public virtual void CancelSelectionPeaks()
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
        public virtual void ClearField()
        {
            canv.Children.Clear();
            AllPeaks.Clear();
            AllRelations.Clear();
            FirstPeak = null;
            SecondPeak = null;
            EditingTextBox = null;
            PeakNum = 0;
        }
        // Удаления объектов
        public virtual void DeleteObject(object src)
        {
            if (src is Ellipse)
            {
                Ellipse el = src as Ellipse;
                Peak p = FindPeakByEllipse(el);
                canv.Children.Remove(el);
                canv.Children.Remove(p.Tb);
                while (p.Relations.Count > 0)
                {
                    canv.Children.Remove(p.Relations[0].LineObj);
                    canv.Children.Remove(p.Relations[0].Txt);
                    AllRelations.Remove(p.Relations[0]);
                    p.Relations.RemoveAt(0);
                }
                AllPeaks.Remove(p);
                if (FirstPeak == p) FirstPeak = null;
                if (SecondPeak == p) SecondPeak = null;
                OnPeakDeleted(new PeakArgs(p));
            }
            else if (src is Line || src is Arrow)
            {
                Relation r = FindRelationByLine(src as FrameworkElement);
                canv.Children.Remove(r.LineObj);
                canv.Children.Remove(r.Txt);
                AllRelations.Remove(r);
                r.FromPeak.Relations.Remove(r);
                r.ToPeak.Relations.Remove(r);
                OnRelationDeleted(new RelationArgs(r));
            }
        }


        #region Обработчики событий компонента

        private void canv_MouseLeave(object sender, MouseEventArgs e)
        {
            if(DraggingEllipse!=null) DraggingEllipse = null;
        }

        private void Canv_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            UnfocusEditingTextBox(e);
            switch (CurrentMode)
            {
                case Mode.AddPeak:
                    CreatePeak(e.GetPosition(canv));
                    break;
                case Mode.AddEdge:
                    if ((e.Source as FrameworkElement) is Ellipse)
                    {
                        Peak p = FindPeakByEllipse((e.Source as FrameworkElement) as Ellipse);
                        SelectPeak(p);
                    }
                    break;
                case Mode.DeleteSelected:
                    DeleteObject(e.Source);
                    break;
                case Mode.SelectForAlghorithm:
                    if ((e.Source as FrameworkElement) is Ellipse)
                    {
                        Peak p = FindPeakByEllipse((e.Source as FrameworkElement) as Ellipse);
                        SelectPeak(p);
                    }
                    break;
                case Mode.MoveObjects:
                    DraggingEllipse = e.Source as Ellipse;
                    break;
            }
        }

        private void canv_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (DraggingEllipse != null) DraggingEllipse = null;
        }

        private void canv_MouseMove(object sender, MouseEventArgs e)
        {
            if (DraggingEllipse != null && CurrentMode == Mode.MoveObjects)
            {
                double x = e.GetPosition(canv).X;
                double y = e.GetPosition(canv).Y;
                // После анимаций свойства блокируется, поэтому сбрасываем анимацию значениями null
                DraggingEllipse.BeginAnimation(Canvas.LeftProperty, null);
                DraggingEllipse.BeginAnimation(Canvas.TopProperty, null);
                Canvas.SetLeft(DraggingEllipse, x);
                Canvas.SetTop(DraggingEllipse, y);
                // Для правильного отображения анимации при смене координат вершины
                if (FindPeakByEllipse(DraggingEllipse) == ConnectingSecondPeak)
                {
                    DoubleAnimation LeftAnim = new DoubleAnimation(Canvas.GetLeft(ConnectingSecondPeak.El),
                                                TimeSpan.FromSeconds(0.1));
                    DoubleAnimation TopAnim = new DoubleAnimation(Canvas.GetTop(ConnectingSecondPeak.El),
                                                    TimeSpan.FromSeconds(0.1));
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
                foreach (Relation rel in FindPeakByEllipse(DraggingEllipse).Relations)
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
                Peak tb = FindPeakByTextBox(EditingTextBox);
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
                    Relation r = FindRelationByTextBox(EditingTextBox);
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
            CancelSelectionPeaks();
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
        // Нахождение максимальной пропускной способности
        public virtual double MaxFlow(Peak FirstPeak, Peak SecondPeak)
        {
            Peak source = FirstPeak;
            Peak destination = SecondPeak;
            List<List<Relation>> AllWays = new List<List<Relation>>();
            // Насыщенные рёбра
            List<Relation> SaturatedRelations = new List<Relation>();
            //Вес рёбер во время работы алгоритма
            List<double> RealRelationsWeight = AllRelations.Select(t => t.Weight).ToList();
            List<Relation> Way = FindWay(ref source, ref destination); ;
            double SumSaturatedRelations = 0;
            while (Way != null)
            {
                double min = Way.Min(t => RealRelationsWeight[AllRelations.IndexOf(t)]);
                SumSaturatedRelations += min;
                AllWays.Add(Way);
                foreach (Relation r in Way)
                {
                    int i = AllRelations.IndexOf(r);
                    RealRelationsWeight[i] -= min;
                }
                SaturatedRelations.AddRange(Way.Where(t => t.Weight == min).ToList());
                Way = FindWay(ref source, ref destination);
            }


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
                    List<Relation> AllOutputRelations = CheckingPeak.Relations.Where(t => ((t.Type == TypeOfRelation.NonOriented) || (t.Type == TypeOfRelation.Oriented && t.FromPeak == CheckingPeak))).ToList();
                    // В outputRel хранятся ненасыщенные соединения, которые ещё не были пройдены и
                    List<Relation> AvaliableOutputRel = AllOutputRelations.Where(t =>
                                                                                            !SaturatedRelations.Contains(t) &&
                                                                                            !CurWay.Contains(t) &&
                                                                                            !CheckedRelations.Contains(t)).ToList();

                    if (CheckingPeak == src && AvaliableOutputRel.Count == 0 && CurWay.Count == 0)
                    {
                        isEnded = true;
                    }

                    if (AvaliableOutputRel.Count != 0)
                    {
                        CurWay.Add(AvaliableOutputRel[0]);
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

            public Ellipse El { get; private set; }
            public TextBox Tb { get; private set; }
            public string Name { get; private set; }
            public List<Relation> Relations { get; private set; }
            public GraphControl Owner { get; private set; }

            public Peak(Ellipse el, TextBox name, GraphControl owner)
            {
                El = el;
                Tb = name;
                Name = name.Text;
                Relations = new List<Relation>();
                Owner = owner;
            }

            public bool SetName(string name)
            {
                Peak p = Owner.FindPeakByName(name);
                if (p == null || name == Name)
                {
                    Name = name;
                    return true;
                }
                return false;
            }
        }

        public Peak FindPeakByEllipse(Ellipse ElForSearch) => AllPeaks.Where(t => t.El == ElForSearch).FirstOrDefault();
        public Peak FindPeakByTextBox(TextBox TbForSearch) => AllPeaks.Where(t => t.Tb == TbForSearch).FirstOrDefault();
        public Peak FindPeakByName(string findingName) => AllPeaks.Where(t => t.Name == findingName).FirstOrDefault();


        public class Relation
        {
            public double Weight { get; set; }
            public Peak FromPeak { get; private set; }
            public Peak ToPeak { get; private set; }
            public TextBox Txt { get; private set; }
            public TypeOfRelation Type { get; private set; }
            public FrameworkElement LineObj { get; private set; }

            public Relation(Peak from, Peak to, double weight, TypeOfRelation type, FrameworkElement line = null, TextBox txt = null)
            {
                FromPeak = from;
                ToPeak = to;
                Weight = weight;
                Type = type;
                Txt = txt;
                LineObj = line;
            }
        }

        public Relation FindRelationByLine(FrameworkElement line) => AllRelations.Where(t => t.LineObj == line).FirstOrDefault();
        public Relation FindRelationByTextBox(TextBox labelForSearch) => AllRelations.Where(t => t.Txt == labelForSearch).FirstOrDefault();
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
