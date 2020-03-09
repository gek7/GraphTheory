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

    class Peak
    {
        static List<Peak> allPeeks;
        private Grid El { get; set; }
        public List<Relation> Relations { get; set; }

         public enum TypeOfRelation
    {
        Oriented,
        NonOriented
    }
        static Peak()
        {
           allPeeks = new List<Peak>();
        }
        public Peak(Grid el)
        {
            El = el;
            allPeeks.Add(this);
        }
        public void SetName(string name)
        {
            (El.Children[1] as Label).Content = name;
        }
    }
}
