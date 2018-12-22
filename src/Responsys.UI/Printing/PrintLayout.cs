using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Enivate.ResponseHub.Responsys.UI.Printing
{
    public class PrintLayout
    {

        public static readonly PrintLayout A4 = new PrintLayout("21cm", "29.7cm", "3.18cm", "2.54cm");
        public static readonly PrintLayout A4Narrow = new PrintLayout("21cm", "29.7cm", "1.27cm", "1.27cm");
        public static readonly PrintLayout A4Moderate = new PrintLayout("21cm", "29.7cm", "1.91cm", "2.54cm");

        public PrintLayout(string w, string h, string leftright, string topbottom)
            : this(w, h, leftright, topbottom, leftright, topbottom)
        {
        }

        public PrintLayout(string w, string h, string left, string top, string right, string bottom)
        {
            var converter = new LengthConverter();
            var width = (double)converter.ConvertFromInvariantString(w);
            var height = (double)converter.ConvertFromInvariantString(h);
            var marginLeft = (double)converter.ConvertFromInvariantString(left);
            var marginTop = (double)converter.ConvertFromInvariantString(top);
            var marginRight = (double)converter.ConvertFromInvariantString(right);
            var marginBottom = (double)converter.ConvertFromInvariantString(bottom);
            Size = new Size(width, height);
            Margin = new Thickness(marginLeft, marginTop, marginRight, marginBottom);

        }


        public Thickness Margin { get; set; }

        public Size Size { get; }

        public double ColumnWidth
        {
            get
            {
                var column = 0.0;
                column = Size.Width - Margin.Left - Margin.Right;
                return column;
            }
        }

    }
}
