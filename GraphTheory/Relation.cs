using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace GraphTheory
{
    public class Relation
    {
        public static List<Relation> AllRelations = new List<Relation>();
        public double Weight { get; set; }
        public Peak FromPeak { get; set; }
        public Peak ToPeak { get; set; }
        public Label Txt { get; set; }
        public TypeOfRelation Type { get; set; }
        public FrameworkElement LineObj { get; set; }

        public Relation(Peak from, Peak to, double weight, TypeOfRelation type, FrameworkElement line = null, Label txt=null)
        {
            FromPeak = from;
            ToPeak = to;
            Weight = weight;
            Type = type;
            Txt = txt;
            LineObj = line;
            AllRelations.Add(this);
        }

        public static Relation FindByLine(FrameworkElement line) => AllRelations.Where(t => t.LineObj == line).FirstOrDefault();

        public static Relation FindByWeightLabel(Label labelForSearch) => AllRelations.Where(t => t.Txt == labelForSearch).FirstOrDefault();
    }
}
