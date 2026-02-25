using EasyLog.Configuration;
using EasyLog.Models;
namespace EasyLog.Services
{
    public class EasyLogLocalLogger : IEasyLogger
    {
        private readonly DailyLogService _dailyLogService;
        private readonly StateLogService _stateService;
        public EasyLogLocalLogger(LogConfiguration logConfig, DailyLogService dailyLogService, StateLogService stateLogService)
        {
            _dailyLogService = dailyLogService;
            _stateService = stateLogService;
        }
        public void UpdateLog(LogEntry entry)
        {
            _dailyLogService.AddLogEntry(entry);
        }
        public void UpdateState(StateEntry state)
        {
            _stateService.UpdateState(state);
        }
    }
}