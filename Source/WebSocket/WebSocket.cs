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
    public class Behaviour : WebSocketBehavior
    {
        protected override void OnOpen()
        {
            base.OnOpen();
            Verse.Log.Message("websocket Connection established");
            WebSocket.dataCollected += this.OnDataCollected;
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            base.OnMessage(e);
        }

        protected override void OnClose(CloseEventArgs e)
        {
            base.OnClose(e);
            WebSocket.dataCollected -= this.OnDataCollected;
        }

        protected override void OnError(WebSocketSharp.ErrorEventArgs e)
        {
            base.OnError(e);
        }

        private void OnDataCollected(object data)
        {
            var serialized = JsonConvert.SerializeObject(data);
            SendAsync(serialized, (_) => {});
        }
    }

    [StaticConstructorOnStartup]
    public static class WebSocket
    {
        static readonly WebSocketServer server;
        static readonly int port = 4000;
        public static event Action<object> dataCollected;
        static int lastUpdatedTick = 0;
        static WebSocket()
        {
            // open socket server
            server = new WebSocketServer(port);
            server.AddWebSocketService<Behaviour>("/rw_analyzer");
            server.Start();
            Verse.Log.Message($"WebSocket server listening on port {port}");
        }

        public static void SendData()
        {
            var currentTick = Find.TickManager.TicksGame;
            if (lastUpdatedTick == currentTick) return;

            var list = new List<TickLog>();

            foreach(var profile in ProfileController.Profiles.Values)
            {
                var log = new TickLog
                {
                    label = profile.label,
                    key = profile.key,
                    hit = profile.hits[profile.currentIndex],
                    time = profile.times[profile.currentIndex]
                };

                list.Add(log);
            }
            

            var jsonData = new JsonData();
            jsonData.type = "data";
            jsonData.data = list;
            jsonData.globalTick = currentTick;

            var serialized = JsonConvert.SerializeObject(jsonData);

            dataCollected?.Invoke(serialized);

            lastUpdatedTick = currentTick;
            
        }

        class JsonData
        {
            public string type;
            public object data;
            public int globalTick;
        }
    }
}