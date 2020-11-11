using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzer.WebSocket.Protocol
{
    public class JsonData
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public virtual JsonDataType type { get; set; } // default: none
    }

    public class ToggleGameState : JsonData
    {
        public override JsonDataType type => JsonDataType.ToggleGameState;
    }

    public static class JsonDataFactory
    {
        public static JsonData Parse(string rawJsonData)
        {
            var obj = JsonConvert.DeserializeObject<JsonData>(rawJsonData);
            JsonData returnValue = new JsonData();
            switch (obj.type)
            {
                case JsonDataType.ToggleGameState:
                    returnValue = new ToggleGameState();
                    break;
            }

            return returnValue;
        }
    }
}
