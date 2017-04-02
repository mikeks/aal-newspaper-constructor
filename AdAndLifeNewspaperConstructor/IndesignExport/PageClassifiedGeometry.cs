using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VitalConnection.AAL.Builder.Model;

namespace VitalConnection.AAL.Builder.IndesignExport
{
    public class PageClassifiedGeometry: PageGeometry
    {

        public double GetFrameX(int column)
        {
            return column * (clasColumnWidth + gap) + left;
        }

        public double FrameTop
        {
            get
            {
                return top;
            }
        }

        public double FrameWidth
        {
            get
            {
                return clasColumnWidth;
            }
        }

        public double FrameHeight
        {
            get
            {
                return frameHeight;
            }
        }




        private double clasColumnWidth;
        private double frameHeight;

        public PageClassifiedGeometry(NewspaperPage p) : base(p)
        {
            frameHeight = PAPER_HEIGHT - top - bottom;
            clasColumnWidth = (PAPER_WIDTH - left - right - gap * (p.Grid.ColumnsCount - 1)) / p.Grid.ColumnsCount;
        }
    }
}
