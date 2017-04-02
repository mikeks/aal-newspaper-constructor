using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VitalConnection.AAL.Builder.Model;

namespace VitalConnection.AAL.Builder.IndesignExport
{
    public abstract class PageGeometry
    {
        protected const float PAPER_WIDTH = 11F;
        protected const float PAPER_HEIGHT = 15F;

        protected double left;
        protected double right;
        protected double top;
        protected double bottom;
        protected double gap;

        public double Gap
        {
            get
            {
                return gap;
            }
        }

        public PageGeometry(NewspaperPage p)
        {
            gap = p.Grid.Gap;
            top = p.Grid.MarginTop;
            bottom = p.Grid.MarginBottom;
            if (p.Number % 2 == 0)
            {
                left = p.Grid.MarginOutside;
                right = p.Grid.MarginInside;
            }
            else
            {
                left = p.Grid.MarginInside;
                right = p.Grid.MarginOutside;
            }
        }
    }
}
