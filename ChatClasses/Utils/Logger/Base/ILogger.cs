using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Utils.Logger.Base
{
    public interface ILogger
    {
        void LogMessage(string message);
    }
}
