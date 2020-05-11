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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static GraphTheory.GraphControl;

namespace GraphTheory
{
    /// <summary>
    /// Логика взаимодействия для TheoryDlg.xaml
    /// </summary>
    public partial class TheoryDlg : Window
    {
        GraphControl Gc;
        Peak FirstPeak;
        Peak SecondPeak;
        public TheoryDlg()
        {
            InitializeComponent();
        }

        public TheoryDlg(GraphControl gc,Peak firstSelPeak, Peak secondSelPeak) : this()
        {
            Gc = gc;
            FirstPeak = firstSelPeak;
            SecondPeak = secondSelPeak;
        }

        private void btn_ok_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                double w = double.Parse(tb_weight.Text);
                if (w >= 0)
                {
                    TypeOfRelation type = TypeOfRelation.NonOriented;
                    if (orient.IsChecked ?? false) type = TypeOfRelation.Oriented;
                    if (norient.IsChecked ?? false) type = TypeOfRelation.NonOriented;
                    if (Gc.AddNewRelation(FirstPeak,SecondPeak,type, w))
                    {
                        Close();
                    }
                    else
                    {
                        MessageBox.Show("Вы не выбрали две вершины");
                    }
                }
                else
                {
                    MessageBox.Show("Вес должен быть положительным");
                }
            }
            catch
            {
                MessageBox.Show("Поле вес - должно быть числом");
            }
        }

        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
