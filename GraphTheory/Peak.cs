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
        public Grid El { get; set; }
        public List<Relation> Relations { get; set; }

        public Peak(Grid el)
        {
            El = el;
            Relations = new List<Relation>();
        }
        public void SetName(string name)
        {
            (El.Children[1] as Label).Content = name;

        }
    }
}
