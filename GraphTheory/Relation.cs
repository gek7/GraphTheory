using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphTheory
{
    class Relation
    {
        double Weight;
        Peak ConnectedPeak;
        TypeOfRelation Type;

        private Relation(Peak connectedPeak,double weight,TypeOfRelation type)
        {
            ConnectedPeak = connectedPeak;
            Weight = weight;
            Type = type;
        }

        public static void AddNewRelation(Peak firstPeak, Peak secondPeak, TypeOfRelation type,double weight=0)
        {
            Relation rel = new Relation(secondPeak, weight, type);
            switch (type)
            {
                case TypeOfRelation.Oriented:
                    firstPeak.Relations.Add(rel);
                    break;
                case TypeOfRelation.NonOriented:
                    firstPeak.Relations.Add(rel);
                    secondPeak.Relations.Add(new Relation(firstPeak, weight, type));
                    break;
            }
        }
    }
}
