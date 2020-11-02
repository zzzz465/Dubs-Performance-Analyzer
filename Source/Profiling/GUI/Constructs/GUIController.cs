using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Analyzer.Profiling
{
    public enum Category
    {
        Settings = 0,
        Tick = 1,
        Update = 2,
        GUI = 3,
        Modder = 4
    }

    public static class GUIController
    {
        private static Tab currentTab;
        private static Entry currentEntry;
        private static Profiler currentProfiler;
        private static Category currentCategory = Category.Settings;

        private static Dictionary<Category, Tab> tabs;
        public static Dictionary<string, Type> types = new Dictionary<string, Type>();

        public static Profiler CurrentProfiler { get { return currentProfiler; } set { currentProfiler = value; } }
        public static Tab GetCurrentTab => currentTab;
        public static Category CurrentCategory => currentCategory;
        public static Entry CurrentEntry => currentEntry;

        public static IEnumerable<Tab> Tabs => tabs.Values;
        public static Tab Tab(Category cat) => tabs[cat];
        public static Entry EntryByName(string name) => Tabs.Where(t => t.entries.Keys.Any(e => e.name == name)).First().entries.First(e => e.Key.name == name).Key;
        public static event Action<string, Category> entryAdded; // entryName, Category
        public static event Action<string> entrySwapped; // entryName
        public static event Action<string> entryRemoved; // entryName

        public static void InitialiseTabs()
        {
            tabs = new Dictionary<Category, Tab>();

            addTab(() => ResourceCache.Strings.tab_setting, () => ResourceCache.Strings.tab_setting_desc, Category.Settings);
            addTab(() => ResourceCache.Strings.tab_tick, () => ResourceCache.Strings.tab_tick_desc, Category.Tick);
            addTab(() => ResourceCache.Strings.tab_update, () => ResourceCache.Strings.tab_update_desc, Category.Update);
            addTab(() => ResourceCache.Strings.tab_gui, () => ResourceCache.Strings.tab_gui_desc, Category.GUI);
            addTab(() => ResourceCache.Strings.tab_modder, () => ResourceCache.Strings.tab_modder_desc, Category.Modder);

            void addTab(Func<string> name, Func<string> desc, Category cat)
            {
                tabs.Add(cat, new Tab(name, () => currentCategory = cat, () => currentCategory == cat, cat, desc));
            }
        }

        public static void ClearEntries()
        {
            foreach (var tab in tabs.Values)
            {
                foreach (var entry in tab.entries.Keys)
                {
                    if (entry.isClosable)
                    {
                        RemoveEntry(entry.name);
                        continue; // already set to unpatched + inactive here
                    }
                    entry.isPatched = false;
                }
            }
        }

        public static void ResetToSettings()
        {
            if (currentEntry != null)
            {
                currentEntry.SetActive(false);
                ResetProfilers();
            }

            currentTab = Tab(Category.Settings);
            currentCategory = Category.Settings;
        }

        public static void ResetProfilers()
        {
            ProfileController.Profiles.Clear();
            Analyzer.RefreshLogCount();
            currentProfiler = null;
        }

        public static void SwapToEntry(string entryName)
        {
            Log.Message($"Swap entry {entryName}");
            if (currentEntry != null)
            {
                currentEntry.SetActive(false);
                ResetProfilers();
            }

            currentEntry = EntryByName(entryName);

            if (!currentEntry.isPatched)
            {
                currentEntry.PatchMethods();
            }

            currentEntry.SetActive(true);
            currentCategory = currentEntry.category;
            currentTab = Tab(currentCategory);

            Log.Message($"entrySwapped event null? {entrySwapped == null}");

            entrySwapped?.Invoke(entryName);
        }

        public static void AddEntry(string name, Category category)
        {
            Log.Message($"add entry {name}");
            Type myType = null;
            if (types.ContainsKey(name))
            {
                myType = types[name];
            }
            else
            {
                myType = DynamicTypeBuilder.CreateType(name, null);
                types.Add(name, myType);
            }


#if DEBUG
            ThreadSafeLogger.Message($"Adding entry {name} into the category {category}");
#endif
            var entry = Entry.Create(name, category, "Dynamically created entry for the type " + myType.Name, myType, true, true);

            if (Tab(category).entries.ContainsKey(entry))
            {
                ThreadSafeLogger.Error($"Attempting to re-add entry {name} into the category {category}");
            }
            else
            {
                Log.Message($"Adding entry {entry.name} in AddEntry");
                Tab(category).entries.Add(entry, myType);
                entryAdded?.Invoke(name, category);
            }
        }

        public static void RemoveEntry(string name)
        {
            Log.Message($"Swap entry {name}");

            var entry = EntryByName(name);
            entry.isPatched = false;
            entry.SetActive(false);

            Tab(entry.category).entries.Remove(entry);

#if DEBUG
            ThreadSafeLogger.Message($"Removing entry {name} from the category {entry.category.ToString()}");
#endif
            entryRemoved?.Invoke(name);
        }
    }
}
