using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Analyzer.WebSocket.Protocol
{
    public enum JsonDataType
    {
        None,
        LogData,
        InitEntries,
        EntryAdded,
        EntrySwapped,
        EntryRemoved
    }
}
