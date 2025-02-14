using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.Finance.Enums
{
    public class IGTradable : Attribute
    {
        public Region[] Region { get; }

        public string Epic { get; }

        public IGTradable(string Epic, params Region[] region)
        {
            Region = region;
            this.Epic = Epic;
        }
    }
}
