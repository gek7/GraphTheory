using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

public class ArrowLine :  Shape
{
    public static readonly DependencyProperty X1Property;
    public static readonly DependencyProperty Y1Property;
    public static readonly DependencyProperty X2Property;
    public static readonly DependencyProperty Y2Property;
    #region clr-свойства
    public double X1
    {
        get { return (double)this.GetValue(X1Property); }
        set { this.SetValue(X1Property, value); }
    }

    public double X2
    {
        get { return (double)this.GetValue(X2Property); }
        set { this.SetValue(X2Property, value); }
    }

    public double Y1
    {
        get { return (double)this.GetValue(Y1Property); }
        set { this.SetValue(Y1Property, value); }
    }

    public double Y2
    {
        get { return (double)this.GetValue(Y2Property); }
        set { this.SetValue(Y2Property, value); }
    }
    #endregion
    static ArrowLine()
    {
        X1Property =
        System.Windows.DependencyProperty.Register(
        "X1",
        typeof(double),
        typeof(ArrowLine),
        new UIPropertyMetadata(0.0, new PropertyChangedCallback(OnX1PropertyChanged)),
        new ValidateValueCallback(ValidateDoubleValue)
        );

            X2Property =
        DependencyProperty.Register(
        "X2",
        typeof(double),
        typeof(ArrowLine),
        new UIPropertyMetadata(0.0, new PropertyChangedCallback(OnX2PropertyChanged)),
        new ValidateValueCallback(ValidateDoubleValue)
        );

            Y1Property =
        DependencyProperty.Register(
        "Y1",
        typeof(double),
        typeof(ArrowLine),
        new UIPropertyMetadata(0.0, new PropertyChangedCallback(OnY1PropertyChanged)),
        new ValidateValueCallback(ValidateDoubleValue)
        );

            Y2Property =
        DependencyProperty.Register(
        "Y2",
        typeof(double),
        typeof(ArrowLine),
        new UIPropertyMetadata(0.0, new PropertyChangedCallback(OnY2PropertyChanged)),
        new ValidateValueCallback(ValidateDoubleValue)
        );
    }
    private static void OnX1PropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        ArrowLine control = sender as ArrowLine;

        if (control != null)
        {

        }
    }

    public static bool ValidateDoubleValue(object value)
    {
        if (value is double)
        {
            double v = (double)value;
            if (v >= 0.0)
            {
                return true;
            }
        }
        return false;
    }


    private static void OnY1PropertyChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
    {
        ArrowLine control = sender as ArrowLine;

        if (control != null)
        {

        }
    }


    private static void OnX2PropertyChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
    {
        ArrowLine control = sender as ArrowLine;

        if (control != null)
        {

        }
    }


 

    private static void OnY2PropertyChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
    {
        ArrowLine control = sender as ArrowLine;

        if (control != null)
        {

        }
    }

    protected override Geometry DefiningGeometry
    {
        get
        {
            // координаты центра отрезка
            double X3 = (this.X1 + this.X2) / 2;
            double Y3 = (this.Y1 + this.Y2) / 2;

            // длина отрезка
            double d = Math.Sqrt(Math.Pow(this.X2 - this.X1, 2) + Math.Pow(this.Y2 - this.Y1, 2));
            // координаты вектора
            double X = this.X2 - this.X1;
            double Y = this.Y2 - this.Y1;

            // координаты точки, удалённой от центра к началу отрезка на 10px
            double X4 = X3 - (X / d) * 10;
            double Y4 = Y3 - (Y / d) * 10;

            // из уравнения прямой {
            // (x - x1)/(x1 - x2) = (y - y1)/(y1 - y2) } получаем вектор перпендикуляра
            // (x - x1)/(x1 - x2) = (y - y1)/(y1 - y2) =>
            // (x - x1)*(y1 - y2) = (y - y1)*(x1 - x2) =>
            // (x - x1)*(y1 - y2) - (y - y1)*(x1 - x2) = 0 =>
            // полученные множители x и y => координаты вектора перпендикуляра
            double Xp = this.Y2 - this.Y1;
            double Yp = this.X1 - this.X2;

            // координаты перпендикуляров, удалённой от точки X4;Y4 на 5px в разные стороны
            double X5 = X4 + (Xp / d) * 5;
            double Y5 = Y4 + (Yp / d) * 5;
            double X6 = X4 - (Xp / d) * 5;
            double Y6 = Y4 - (Yp / d) * 5;

            GeometryGroup geometryGroup = new GeometryGroup();

            LineGeometry lineGeometry = new LineGeometry(new Point(this.X1, this.Y1), new Point(this.X2, this.Y2));
            LineGeometry arrowPart1Geometry = new LineGeometry(new Point(X3, Y3), new Point(X5, Y5));
            LineGeometry arrowPart2Geometry = new LineGeometry(new Point(X3, Y3), new Point(X6, Y6));

            geometryGroup.Children.Add(lineGeometry);
            geometryGroup.Children.Add(arrowPart1Geometry);
            geometryGroup.Children.Add(arrowPart2Geometry);

            return geometryGroup;
        }
    }
}