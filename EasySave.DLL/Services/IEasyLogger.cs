using EasyLog.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLog.Services
{
    public interface IEasyLogger
    {
        public void UpdateLog(LogEntry entry);

        public void UpdateState(StateEntry state);
    }
}
