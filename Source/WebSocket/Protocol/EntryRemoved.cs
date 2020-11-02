using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzer.WebSocket.Protocol
{
    class EntryRemoved : JsonData
    {
        public override JsonDataType type => JsonDataType.EntryRemoved;
        public EntryRemoved(string name)
        {

        }
    }
}
