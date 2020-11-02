using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzer.WebSocket.Protocol
{
    public class LogData : JsonData
    {
        public override JsonDataType type => JsonDataType.LogData;
        public IEnumerable<TickLog> tickLogs { get; set; }
        public int globalTick { get; set; }

        public LogData(IEnumerable<TickLog> logs, int globalTick)
        {
            this.tickLogs = logs;
            this.globalTick = globalTick;
        }
    }
}
