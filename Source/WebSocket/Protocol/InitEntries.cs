using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzer.WebSocket.Protocol
{
    public class InitEntries : JsonData
    {
        public class Entry
        {
            public string name, category;
        }

        public override JsonDataType type => JsonDataType.InitEntries;
        public IEnumerable<InitEntries.Entry> entries { get; set; }

        public InitEntries(IEnumerable<Entry> entries)
        {
            this.entries = entries;
        }
    }
}
