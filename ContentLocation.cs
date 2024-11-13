using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JavascriptEmulator
{
    public class ContentLocation
    {
        public string FileName { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }

        public ContentLocation Copy()
        {
            return new ContentLocation
            {
                FileName = FileName,
                Row = Row,
                Column = Column
            };
        }

        public override string ToString()
        {
            return $"{FileName}:{Row}:{Column}";
        }
    }
}
