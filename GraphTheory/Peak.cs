using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace GraphTheory
{
    public enum TypeOfRelation
    {
        Oriented,
        NonOriented
    }

    public class Peak
    {
        private static List<Peak> AllPeaks = new List<Peak>();
        public Ellipse El { get; set; }
        public TextBox Name { get; set; }
        public List<Relation> Relations { get; set; }

        public Peak(Ellipse el, TextBox name)
        {
            El = el;
            Name = name;
            Relations = new List<Relation>();
            AllPeaks.Add(this);
        }
        public void SetName(string name)
        {
            Name.Text = name;
        }

        public static Peak FindByEllipse(Ellipse ElForSearch) => AllPeaks.Where(t => t.El == ElForSearch).FirstOrDefault();
        public static Peak FindByTextBox(TextBox TbForSearch) => AllPeaks.Where(t => t.Name == TbForSearch).FirstOrDefault();
    }
}
