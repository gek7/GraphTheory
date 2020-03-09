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
        private Ellipse El { get; set; }
        private Label Name { get; set; }
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
        public Peak(Ellipse el,Label name)
        {
            El = el;
            Name = name;
            allPeeks.Add(this);
        }
        public void SetName(string name)
        {
            Name.Content = name;
        }
    }
}
