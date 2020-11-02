using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzer.WebSocket.Protocol
{
    public abstract class JsonData
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public abstract JsonDataType type { get; }
    }
}
