﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using HarmonyLib;
using Verse;

namespace DubsAnalyzer
{
    [AttributeUsage(AttributeTargets.Field)]
    public class Setting : Attribute
    {
        public string name;
        public string tip;
        public Setting(string name, string tip = null)
        {
            this.name = name;
            this.tip = tip.Translate();
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class PerformancePatch : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ProfileMode : Attribute
    {
        public bool IsPatched = false;
        public Dictionary<FieldInfo, Setting> Settings;

        public string name;
        public string tip;

        public UpdateMode mode;
        public bool Active = false;
        public Type typeRef;
        public MethodInfo MouseOver;
        public MethodInfo Selected;
        public MethodInfo Clicked;
        public MethodInfo Checkbox;
        public MethodInfo DoRow;
        public bool Basics = false;
        public Thread Patchinator = null;

        public ProfileMode(string name, UpdateMode mode, string tip = null, bool Basics = false)
        {
            this.name = name;
            this.mode = mode;
            this.tip = tip.Translate();
            this.Basics = Basics;
        }

        public void ProfilePatch()
        {
            if (Patchinator == null)
            {
                if (Dialog_Analyzer.State == CurrentState.Unpatching) // We are currently unpatching methods, we should not be currently patching more methods
                    return;

                Patchinator = new Thread(() =>
                {                     
                    try
                    {
                        AccessTools.Method(typeRef, "ProfilePatch")?.Invoke(null, null);
                        IsPatched = true;
                    }
                    catch (Exception e)
                    {
                        Log.Error(e.ToString());
                    }
                });
                Patchinator.Start();
            }
        }
    }
}