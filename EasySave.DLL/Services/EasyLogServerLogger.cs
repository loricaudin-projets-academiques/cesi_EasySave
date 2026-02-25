using EasyLog.Configuration;
using EasyLog.Models;

namespace EasyLog.Services {
    public class EasyLogServerLogger : IEasyLogger {
        private readonly EasyLogServer _server;
        public EasyLogServerLogger(LogConfiguration logConfig) {
            _server = new EasyLogServer();
            _server.ConnectToLogServer(logConfig.LogServerUrl, logConfig.LogServerPort);
        } public void UpdateLog(LogEntry entry) {
            _server.SendJson(entry);
        } public void UpdateState(StateEntry state)
        { _server.SendJson(state);
        } } }