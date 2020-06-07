﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace DubsAnalyzer
{
    [ProfileMode("HarmonyTranspilers", UpdateMode.Update, "TransPatchTipKey")]
    internal class H_HarmonyTranspilers
    {
        public static bool Active = false;

        public static string TipCache = string.Empty;
        public static string LogCache;

        public static void MouseOver(Rect r, Profiler prof, ProfileLog log)
        {
            if (LogCache != log.Label)
            {
                LogCache = log.Label;
                TipCache = string.Empty;
                //   var patches = Harmony.GetAllPatchedMethods().ToList();

                //    foreach (var methodBase in patches)
                //    {
                var infos = Harmony.GetPatchInfo(log.Meth);
                //foreach (var infosPrefix in infos.Prefixes)
                //{
                //    if (infosPrefix.PatchMethod == log.Meth)
                //    {
                //        TipCache += $"{infosPrefix.owner} {infosPrefix.PatchMethod}\n";
                //    }
                //}
                //foreach (var infosPostfixesx in infos.Postfixes)
                //{
                //    if (infosPostfixesx.PatchMethod == log.Meth)
                //    {
                //        TipCache += $"{infosPostfixesx.owner} {infosPostfixesx.PatchMethod}\n";
                //    }
                //}
                foreach (var infosPostfixesx in infos.Transpilers)
                {
                    //  if (infosPostfixesx.PatchMethod == log.Meth)
                    //  {
                    TipCache += $"{infosPostfixesx.owner} - {infosPostfixesx.PatchMethod.Name}\n\n";
                    // }
                }
                // }
            }
            TooltipHandler.TipRegion(r, TipCache);
        }

        public static void Clicked(Profiler prof, ProfileLog log)
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {
                if (log.Meth != null)
                {
                    Analyzer.harmony.Unpatch(log.Meth, HarmonyPatchType.Transpiler, "*");
                    Messages.Message("Unpatched", MessageTypeDefOf.TaskCompletion, false);
                }
                else
                {
                    Messages.Message("Null method", MessageTypeDefOf.NegativeEvent, false);
                }

            }
        }

        public static List<MethodBase> PatchedMeths = new List<MethodBase>();
        public static void ProfilePatch()
        {
            var go = new HarmonyMethod(typeof(H_HarmonyTranspilers), nameof(Prefix));
            var biff = new HarmonyMethod(typeof(H_HarmonyTranspilers), nameof(Postfix));
            int c = 0;
            int p = 0;
            var patches = Harmony.GetAllPatchedMethods().ToList();
            foreach (var mode in patches)
            {
                try
                {
                    c++;

                    Patches patchInfo = Harmony.GetPatchInfo(mode);
                    var pilers = patchInfo.Transpilers;
                    if (!pilers.NullOrEmpty())
                    {
                        p++;

                        if (pilers.Any(x => x.owner != Analyzer.harmony.Id && x.owner != Analyzer.perfharmony.Id) && !PatchedMeths.Contains(mode))
                        {
                            PatchedMeths.Add(mode);
                            Analyzer.harmony.Patch(mode, go, biff);
                            // Log.Warning($"Patched transpiler {mode}");
                        }
                    }
                }
                catch (Exception)
                {

                }

            }

            //  Log.Warning($"{c} patched methods");
            //  Log.Warning($"{p} with transpilers");
        }

        [HarmonyPriority(Priority.Last)]
        public static void Prefix(MethodBase __originalMethod, ref string __state)
        {
            if (!Active)
            {
                return;
            }

            __state = __originalMethod.Name;

            Analyzer.Start(__state, () =>
            {
                if (__originalMethod.ReflectedType != null)
                {
                    return $"{__originalMethod.Name} - {__originalMethod.ReflectedType.FullName}";
                }
                return $"{__originalMethod.Name} - {__originalMethod.GetType().FullName}";
            }, __originalMethod.GetType(), null, null, __originalMethod as MethodInfo);
        }

        [HarmonyPriority(Priority.First)]
        public static void Postfix(string __state)
        {
            if (Active)
            {
                Analyzer.Stop(__state);
            }
        }
    }

    [ProfileMode("HarmonyPatches", UpdateMode.Update, "HarmPatchesTipKey")]
    internal class H_HarmonyPatches
    {
        public static bool Active = false;
        public static void Clicked(Profiler prof, ProfileLog log)
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {
                if (log.Meth == null)
                {
                    Messages.Message("Null method", MessageTypeDefOf.NegativeEvent, false);
                    return;
                }

                var patches = Harmony.GetAllPatchedMethods().ToList();
                // Patches patchInfo = Harmony.GetPatchInfo(log.Meth);

                foreach (var methodBase in patches)
                {
                    var infos = Harmony.GetPatchInfo(methodBase);
                    foreach (var infosPrefix in infos.Prefixes)
                    {
                        if (infosPrefix.PatchMethod == log.Meth)
                        {
                            Analyzer.harmony.Unpatch(methodBase, HarmonyPatchType.All, "*");
                            Messages.Message("Unpatched prefixes", MessageTypeDefOf.TaskCompletion, false);
                        }
                    }
                    foreach (var infosPostfixesx in infos.Postfixes)
                    {
                        if (infosPostfixesx.PatchMethod == log.Meth)
                        {
                            Analyzer.harmony.Unpatch(methodBase, HarmonyPatchType.All, "*");
                            Messages.Message("Unpatched postfixes", MessageTypeDefOf.TaskCompletion, false);
                        }
                    }
                }
            }
        }

        public static List<Patch> PatchedPres = new List<Patch>();
        public static List<Patch> PatchedPosts = new List<Patch>();
        public static void ProfilePatch()
        {            
            var go = new HarmonyMethod(typeof(H_HarmonyPatches), nameof(Prefix));
            var biff = new HarmonyMethod(typeof(H_HarmonyPatches), nameof(Postfix));
            var patches = Harmony.GetAllPatchedMethods().ToList();

            foreach (var mode in patches)
            {
                Patches patchInfo = Harmony.GetPatchInfo(mode);
                foreach (var fix in patchInfo.Prefixes)
                {
                    try
                    {
                        if (Analyzer.harmony.Id != fix.owner && Analyzer.perfharmony.Id != fix.owner && !PatchedPres.Contains(fix))
                        {
                            PatchedPres.Add(fix);
                            Analyzer.harmony.Patch(fix.PatchMethod, go, biff);
                        }
                    }
                    catch (Exception )
                    {

                    }
                }

                foreach (var fix in patchInfo.Postfixes)
                {
                    try
                    {
                        if (Analyzer.harmony.Id != fix.owner && Analyzer.perfharmony.Id != fix.owner && !PatchedPosts.Contains(fix))
                        {
                            PatchedPosts.Add(fix);
                            Analyzer.harmony.Patch(fix.PatchMethod, go, biff);
                        }
                    }
                    catch (Exception)
                    {

                    }
                }
            }
        }

        [HarmonyPriority(Priority.Last)]
        public static void Prefix(MethodBase __originalMethod, ref string __state)
        {
            if (!Active) return;

            __state = __originalMethod.GetHashCode().ToString();

            Analyzer.Start(__state, () =>
            {
                if (__originalMethod.ReflectedType != null)
                {
                    return $"{__originalMethod.ToString()} - {__originalMethod.ReflectedType.FullName}";
                }
                return $"{__originalMethod.ToString()} - {__originalMethod.GetType().FullName}";
            }, __originalMethod.GetType(), null, null, __originalMethod as MethodInfo);
        }

        [HarmonyPriority(Priority.First)]
        public static void Postfix(string __state)
        {
            if (Active)
            {
                Analyzer.Stop(__state);
            }
        }
    }
}