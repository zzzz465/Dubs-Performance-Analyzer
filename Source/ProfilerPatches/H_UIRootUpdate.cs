﻿using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.Sound;

namespace DubsAnalyzer
{
    [ProfileMode("UIRootUpdate", UpdateMode.Update)]
    internal static class H_UIRootUpdate
    {
        public static bool Active = false;

        public static void ProfilePatch()
        {

            var go = new HarmonyMethod(typeof(H_UIRootUpdate), nameof(Start));
            var biff = new HarmonyMethod(typeof(H_UIRootUpdate), nameof(Stop));

            void slop(Type e, string s)
            {
                Analyzer.harmony.Patch(AccessTools.Method(e, s), go, biff);
            }

            slop(typeof(ScreenshotTaker), nameof(ScreenshotTaker.Update));
            slop(typeof(DragSliderManager), nameof(DragSliderManager.DragSlidersUpdate));
            slop(typeof(WindowStack), nameof(WindowStack.WindowsUpdate));
            slop(typeof(MouseoverSounds), nameof(MouseoverSounds.ResolveFrame));
            slop(typeof(UIHighlighter), nameof(UIHighlighter.UIHighlighterUpdate));
            slop(typeof(Messages), nameof(Messages.Update));
            slop(typeof(WorldInterface), nameof(WorldInterface.WorldInterfaceUpdate));
            slop(typeof(MapInterface), nameof(MapInterface.MapInterfaceUpdate));
            slop(typeof(AlertsReadout), nameof(AlertsReadout.AlertsReadoutUpdate));
            slop(typeof(LessonAutoActivator), nameof(LessonAutoActivator.LessonAutoActivatorUpdate));
            slop(typeof(Tutor), nameof(Tutor.TutorUpdate));
        }

        public static void Start(MethodInfo __originalMethod, ref string __state)
        {
            if (Active)
            {
                __state = $"{__originalMethod.DeclaringType} - {__originalMethod.Name}";
                Analyzer.Start(__state);
            }
        }

        public static void Stop(string __state)
        {
            if (Active) Analyzer.Stop(__state);
        }
    }
}