using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VitalConnection.AAL.Builder.Model;

namespace VitalConnection.AAL.Builder.IndesignExport
{
    public class PageGridGeometry: PageGeometry
    {

        public double GetX(int column)
        {
            return left + (column - 1) * cellWidth;
        }

        public double GetY(int row)
        {
            return top + (row - 1) * cellHeight;
        }

        public double GetWidth(int colSpan)
        {
            return colSpan * cellWidth - gap;
        }

        public double GetHeight(int rowSpan)
        {
            return rowSpan * cellHeight - gap;
        }

        private double cellWidth;
        private double cellHeight;


        public PageGridGeometry(NewspaperPage p) : base(p)
        {
            var fullWidth = (PAPER_WIDTH - left - right + gap);
            var fullHeight = (PAPER_HEIGHT - top - bottom + gap);
            cellWidth = fullWidth / p.Grid.ColumnsCount;
            cellHeight = fullHeight / p.Grid.RowCount;
        }
    }
}
