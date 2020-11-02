using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzer.WebSocket.Protocol
{
    class EntrySwapped : JsonData
    {
        public override JsonDataType type => JsonDataType.EntrySwapped;
        public string entryName { get; set; }
        public EntrySwapped(string name)
        {
            entryName = name;
        }
    }
}
