﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
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
using Xceed.Wpf.Toolkit;

namespace TestApp
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.WindowState = System.Windows.WindowState.Maximized;
            //GraphComp.SecondPeakSelected += GraphComp_SecondPeakSelected;
        }

        //private void GraphComp_SecondPeakSelected(object sender, EventArgs e)
        //{
        //    Dlg d = new Dlg(GraphComp);
        //    d.ShowDialog();
        //}

        //private void ClrPick_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        //{
        //    Color c = new Color();
        //    c.R = 0;
        //    c.G = 0;
        //    c.B = 0;
        //    GraphComp.PeakColor = new SolidColorBrush(ClrPick.SelectedColor ?? c);
        //}

        //private void AddPeak_Click(object sender, RoutedEventArgs e)
        //{
        //    if (sender is RadioButton)
        //    {
        //        RadioButton r = sender as RadioButton;
        //        if (r == addPeak)
        //        {
        //            GraphComp.CurrentMode = GraphTheory.Mode.AddPeak;
        //        }
        //        if (r == addEdge)
        //        {
        //            GraphComp.CurrentMode = GraphTheory.Mode.AddEdge;
        //        }
        //        if (r == delObj)
        //        {
        //            GraphComp.CurrentMode = GraphTheory.Mode.DeleteSelected;
        //        }
        //    }
        //}
    }
}