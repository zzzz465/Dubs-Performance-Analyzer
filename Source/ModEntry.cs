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

namespace Analyzer
{
    [StaticConstructorOnStartup]
    public static class ModEntry
    {
        static List<FieldInfo> profileLogInfos = new List<FieldInfo>();
        static ModEntry()
        {
            OpenSocketServer();
            var serializer = new JsonSerializer();
            var writer = new StringWriter();
            serializer.Serialize(writer, new { name = "asdf" });

            var result = writer.ToString();

            Log.Message(result);
        }

        class ExampleBehaviour : WebSocketBehavior
        {
            protected override void OnOpen()
            {
                base.OnOpen();
                Verse.Log.Message("Connected!");
            }

            protected override void OnMessage(MessageEventArgs e)
            {
                base.OnMessage(e);
                if(e.Data == "request")
                {
                    var logs = Analyzer.Profiling.Analyzer.Logs;
                    Verse.Log.Message($"Log count: ${logs.Count}");
                    try
                    {
                        var serializedObjects = JsonConvert.SerializeObject(logs);
                        Verse.Log.Message(serializedObjects);
                        var UTF8EncodedBytes = Encoding.UTF8.GetBytes(serializedObjects);
                        this.Send(serializedObjects);
                        // this.Send("Hello respond");
                    }
                    catch(Exception ex)
                    {
                        RecursivePrint(ex);
                    }
                }
            }
        }

        static void RecursivePrint(Exception ex)
        {
            Verse.Log.Message(ex.Message);
            if (ex.InnerException != null)
                RecursivePrint(ex.InnerException);
        }

        static void OpenSocketServer()
        {
            var server = new WebSocketServer(4000);
            server.AddWebSocketService<ExampleBehaviour>("/Example");
            server.Start();
            Verse.Log.Message("Listening server on localhost 4000 port");
        }

        static string SerializeToJsonString(this IEnumerable<ProfileLog> logs)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append('[');
            foreach(var log in logs)
            {
                List<string> fields = new List<string>();
                foreach(var fi in profileLogInfos)
                {
                    var name = fi.Name;
                    var value = fi.GetValue(log);
                }
            }

            builder.Append(']');
            return builder.ToString();
        }
    }
}
