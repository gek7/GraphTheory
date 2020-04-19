using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace Tomers.WPF.Shapes
{
	public sealed class Arrow : Shape
	{
		#region Dependency Properties

		public static readonly DependencyProperty X1Property = DependencyProperty.Register("X1", typeof(double), typeof(Arrow), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));
		public static readonly DependencyProperty Y1Property = DependencyProperty.Register("Y1", typeof(double), typeof(Arrow), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));
		public static readonly DependencyProperty X2Property = DependencyProperty.Register("X2", typeof(double), typeof(Arrow), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));
		public static readonly DependencyProperty Y2Property = DependencyProperty.Register("Y2", typeof(double), typeof(Arrow), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure)); 
		public static readonly DependencyProperty HeadWidthProperty = DependencyProperty.Register("HeadWidth", typeof(double), typeof(Arrow), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));
		public static readonly DependencyProperty HeadHeightProperty = DependencyProperty.Register("HeadHeight", typeof(double), typeof(Arrow), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure)); 

		#endregion

		#region CLR Properties

		public GraphTheory.GraphControl gc { set; get; }

		[TypeConverter(typeof(LengthConverter))]
		public double X1
		{
			get { return (double)base.GetValue(X1Property); }
			set { base.SetValue(X1Property, value); }
		}

		[TypeConverter(typeof(LengthConverter))]
		public double Y1
		{
			get { return (double)base.GetValue(Y1Property); }
			set { base.SetValue(Y1Property, value); }
		}

		[TypeConverter(typeof(LengthConverter))]
		public double X2
		{
			get { return (double)base.GetValue(X2Property); }
			set { base.SetValue(X2Property, value); }
		}

		[TypeConverter(typeof(LengthConverter))]
		public double Y2
		{
			get { return (double)base.GetValue(Y2Property); }
			set { base.SetValue(Y2Property, value); }
		}

		[TypeConverter(typeof(LengthConverter))]
		public double HeadWidth
		{
			get { return (double)base.GetValue(HeadWidthProperty); }
			set { base.SetValue(HeadWidthProperty, value); }
		}

		[TypeConverter(typeof(LengthConverter))]
		public double HeadHeight
		{
			get { return (double)base.GetValue(HeadHeightProperty); }
			set { base.SetValue(HeadHeightProperty, value); }
		}

		#endregion

		#region Overrides

		protected override Geometry DefiningGeometry
		{
			get
			{
				// Create a StreamGeometry for describing the shape
				StreamGeometry geometry = new StreamGeometry();
				geometry.FillRule = FillRule.EvenOdd;

				using (StreamGeometryContext context = geometry.Open())
				{
					InternalDrawArrowGeometry(context);
				}

				// Freeze the geometry for performance benefits
				geometry.Freeze();

				return geometry;
			}
		}		

		#endregion

		#region Privates

		private void InternalDrawArrowGeometry(StreamGeometryContext context)
		{
			double theta = Math.Atan2(Y1 - Y2, X1 - X2);
			double sint = Math.Sin(theta);
			double cost = Math.Cos(theta);
			Point pt1 = new Point(X1, this.Y1);
			double X3 = X2 + (gc.PeakWidth/2) * cost;
			double Y3 = Y2 + (gc.PeakWidth/2) * sint;
			Point pt2 = new Point(X3, Y3);
			theta = Math.Atan2(Y1 - Y3, X1 - X3);
			sint = Math.Sin(theta);
			cost = Math.Cos(theta);
			Point pt3 = new Point(
				X3 + (HeadWidth * cost - HeadHeight * sint),
				Y3 + (HeadWidth * sint + HeadHeight * cost));

			Point pt4 = new Point(
				X3 + (HeadWidth * cost + HeadHeight * sint),
				Y3 - (HeadHeight * cost - HeadWidth * sint));

			
			context.BeginFigure(pt1, true, false);
			context.LineTo(pt2, true, true);
			context.LineTo(pt3, true, true);
			context.LineTo(pt2, true, true);
			context.LineTo(pt4, true, true);
		}
		
		#endregion
	}
}
