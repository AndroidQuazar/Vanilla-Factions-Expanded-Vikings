using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VFEV.Facepaint
{
    using Verse;

    public class FacepaintExtension : DefModExtension
    {
        public List<string> tags = new List<string>();
        public ColorGenerator color;
        public List<string>   tagsTwo = new List<string>();
        public ColorGenerator colorTwo;
    }
}
