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
using Analyzer.WebSocket.Protocol;

using LogData = Analyzer.WebSocket.Protocol.LogData;

namespace Analyzer.WebSocket
{

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
            if (e.IsText)
            {
                var data = e.Data;
                try
                {
                    var obj = JsonDataFactory.Parse(data);
                    Verse.Log.Message(obj.ToString());
                    switch(obj)
                    {
                        case ToggleGameState _:
                            Find.TickManager.TogglePaused();
                            break;

                        case EntrySwapped entry:
                            GUIController.SwapToEntry(entry.entryName);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Verse.Log.Message(ex.Message);
                }
            }
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
                .Select(entry => new InitEntries.Entry() { name = entry.name, category = entry.category.ToString() });

            var data = new InitEntries(entries);

            SendData(data);
        }

        private void OnDataCollected(JsonData data)
        {
            SendData(data, true);
        }

        private void SendData(JsonData data, bool async = false)
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

            var obj = new EntryAdded(name, entryCategory.ToString());

            SendData(obj);
        }

        private void OnEntrySwapped(string name)
        {
            var data = new EntrySwapped(name);

            SendData(data);
        }
        private void OnEntryRemoved(string name)
        {
            var data = new EntryRemoved(name);

            SendData(data);
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

        public static void SendData(TimeSpan elapsedTime)
        {
            var currentTick = Find.TickManager.TicksGame;
            if (lastUpdatedTick == currentTick) return;

            var list = new List<TickLog>();

            var builder = new StringBuilder();

            foreach(var profile in ProfileController.Profiles.Values)
            {
                var index = (profile.currentIndex - 1) % Profiler.RECORDS_HELD;
                var log = new TickLog
                {
                    label = profile.label,
                    key = profile.key,
                    hit = profile.hits[index],
                    time = profile.times[index],
                    tick = currentTick
                };
                
                list.Add(log);
            }

            var data = new LogData(list, currentTick, elapsedTime.TotalMilliseconds);

            dataCollected?.Invoke(data);

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