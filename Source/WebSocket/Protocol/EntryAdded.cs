using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzer.WebSocket.Protocol
{
    class EntryAdded : JsonData
    {
        public override JsonDataType type => JsonDataType.EntryAdded;
        public string name { get; set; }
        public string category { get; set; }

        public EntryAdded(string name, string category)
        {
            this.name = name;
            this.category = category;
        }
    }
}
