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
using Newtonsoft.Json.Converters;
using HarmonyLib;

namespace Analyzer.WebSocket
{
    public enum JsonDataType
    {
        LogData,
        InitEntries,
        EntryAdded,
        EntrySwapped,
        EntryRemoved
    }
    public class JsonData
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public JsonDataType type;
    }

    public class Behaviour : WebSocketBehavior
    {
        protected override void OnOpen()
        {
            base.OnOpen();
            Verse.Log.Message("websocket Connection established");

            SendAvailableEntries();

            WebSocket.dataCollected += this.OnDataCollected;
            GUIController.entryAdded += this.OnEntryAdded;
            GUIController.entrySwapped += this.OnEntrySwapped;
            GUIController.entryRemoved += this.OnEntryRemoved;
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            base.OnMessage(e);
        }

        protected override void OnClose(CloseEventArgs e)
        {
            base.OnClose(e);
            WebSocket.dataCollected -= this.OnDataCollected;
            GUIController.entryAdded -= this.OnEntryAdded;
            GUIController.entrySwapped -= this.OnEntrySwapped;
            GUIController.entryRemoved -= this.OnEntryRemoved;
        }

        protected override void OnError(WebSocketSharp.ErrorEventArgs e)
        {
            base.OnError(e);
        }

        private void SendAvailableEntries()
        {
            var entries = GenTypes.AllTypes.Where(m => m.TryGetAttribute<Entry>(out _)).OrderBy(m => m.TryGetAttribute<Entry>().name)
                .Select(type => type.TryGetAttribute<Entry>())
                .Select(entry => new { name = entry.name, category = entry.category.ToString() });

            var jsonData = new JsonData() {
                type = JsonDataType.InitEntries,
                data = entries
            };

            SendObj(jsonData);
        }

        private void OnDataCollected(JsonData data)
        {
            SendObj(data, true);
        }

        private void SendObj(JsonData data, bool async = false)
        {
            var serializedText = JsonConvert.SerializeObject(data);
            if (async)
                SendAsync(serializedText, (_) => { });
            else
                Send(serializedText);
        }

        private void OnEntryAdded(string name, Category entryCategory)
        {
            var data = new {
                name,
                category = entryCategory
            };

            var obj = new JsonData();
            obj.type = JsonDataType.EntryAdded;
            obj.data = data;

            SendObj(obj);
        }

        private void OnEntrySwapped(string name)
        {
            var obj = new JsonData()
            {
                type = JsonDataType.EntrySwapped,
                data = new { name }
            };

            SendObj(obj);
        }
        private void OnEntryRemoved(string name)
        {
            var obj = new JsonData()
            {
                type = JsonDataType.EntryRemoved,
                data = new { name }
            };

            SendObj(obj);
        }
    }

    [StaticConstructorOnStartup]
    public static class WebSocket
    {
        static readonly WebSocketServer server;
        static readonly int port = 4000;
        public static event Action<JsonData> dataCollected;
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
            jsonData.type = JsonDataType.LogData;
            jsonData.data = new {
                tickLogs = list,
                globalTick = currentTick
            };

            dataCollected?.Invoke(jsonData);

            lastUpdatedTick = currentTick;
            
        }
    }
    static class DevStringPatch
    {
        public static void Patch()
        {
            // why it can't find pesudo method?
            /*
            var pesudoTranslatedOriginal = Verse.Translator.PseudoTranslated;
            var prefix = AccessTools.Method(typeof(DevStringPatch), "prefix");
            var harmony = new Harmony("Madeline.harmonyPatcher");
            if (pesudoTranslatedOriginal == null)
                throw new Exception("pesudo null");
            if (pesudoTranslatedOriginal == null)
                throw new Exception("prefix null");
            harmony.Patch(pesudoTranslatedOriginal, new HarmonyMethod(prefix));
            Log.Message("Patched!");
            */
        }

        static bool prefix(string original, ref string __result)
        {
            Log.Message("Prefix");
            __result = original;
            return false;
        }
    }
}