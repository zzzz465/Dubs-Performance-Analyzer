using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Verse;
using System.Net;
using System.Net.WebSockets;
using System.Net.Sockets;
using WebSocketSharp;
using WebSocketSharp.Server;
using Analyzer.Profiling;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.IO;
using Newtonsoft.Json;

namespace Analyzer.WebSocket
{
    public struct TickLog
    {
        public string label, key;
        public int hit;
        public double time;
    }
}