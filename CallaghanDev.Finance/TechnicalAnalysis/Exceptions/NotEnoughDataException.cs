using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.Finance.TechnicalAnalysis.Exceptions
{
    public class NotEnoughDataException : Exception
    {
        public NotEnoughDataException(string message) : base(message) { }

        public NotEnoughDataException(string message, Exception innerException) : base(message, innerException) { }
    }

}
