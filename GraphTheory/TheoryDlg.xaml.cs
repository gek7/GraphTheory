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

namespace GraphTheory
{
    /// <summary>
    /// Логика взаимодействия для TheoryDlg.xaml
    /// </summary>
    public partial class TheoryDlg : Window
    {
        GraphControl Gc;
        public TheoryDlg()
        {
            InitializeComponent();
        }

        public TheoryDlg(GraphControl gc) : this()
        {
            Gc = gc;
        }

        private void btn_ok_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                double w = double.Parse(tb_weight.Text);
                TypeOfRelation type = TypeOfRelation.NonOriented;
                if (orient.IsChecked ?? false) type = TypeOfRelation.Oriented;
                if (norient.IsChecked ?? false) type = TypeOfRelation.NonOriented;
                if (Gc.AddNewRelation(type, w))
                {
                    Close();
                }
                else
                {
                    MessageBox.Show("Вы не выбрали две вершины");
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
