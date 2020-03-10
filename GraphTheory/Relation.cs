using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphTheory
{
    public class Relation
    {
        double Weight;
        Peak ConnectedPeak;
        TypeOfRelation Type;

        public Relation(Peak connectedPeak,double weight,TypeOfRelation type)
        {
            ConnectedPeak = connectedPeak;
            Weight = weight;
            Type = type;
        }
    }
}
