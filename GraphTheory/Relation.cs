using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace GraphTheory
{
    public class Relation
    {
        public double Weight;
        public Peak FromPeak;
        public Peak ToPeak;
        public Label Txt;
        public TypeOfRelation Type;

        public Relation(Peak from,Peak to,double weight,TypeOfRelation type,Label txt=null)
        {
            FromPeak = from;
            ToPeak = to;
            Weight = weight;
            Type = type;
            Txt = txt;
        }
    }
}
