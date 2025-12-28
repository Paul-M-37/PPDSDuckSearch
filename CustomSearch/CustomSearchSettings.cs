using System;
using MelonLoader;

#nullable disable

namespace Custom_Search
{
    public static class CustomSearchSettings //MelonSettings for the Mod
    {
        private const string SettingsCategory = "CustomSearch";
        internal static MelonPreferences_Entry<bool> DuckSearch;
        internal static MelonPreferences_Entry<bool> NoMsgFlag;
        internal static MelonPreferences_Entry<String> ModTitle;
        internal static MelonPreferences_Entry<String> NameLabel;
        internal static MelonPreferences_Entry<String> ButtonText;
        internal static MelonPreferences_Entry<String> MsgDuckNotFound;
        internal static MelonPreferences_Entry<String> MsgDuckNotHere;
        internal static MelonPreferences_Entry<String> MsgDuckFound;
        internal static MelonPreferences_Entry<string> MsgDuckDuplicate;
        internal static void RegisterSettings()
        {
            var category = MelonPreferences.CreateCategory(SettingsCategory, "CustomSearch");
            DuckSearch = category.CreateEntry("DuckSearch", true, "Duck Search", "Focus the searched Duck ");
            NoMsgFlag = category.CreateEntry("NoDebugMessage", true, "No Message", "No Debug Message");
            ModTitle = category.CreateEntry("ModTitle", "Il est où mon canard ?", "Mod Title", "Title of the Mod ");
            NameLabel = category.CreateEntry("NameLabel", "Nom du Canard :", "Name Label", "Label field duck name ");
            ButtonText = category.CreateEntry("ButtonText", "Trouve mon Canard !!", "Button Text", "Button Text ");
            MsgDuckNotFound = category.CreateEntry("MsgDuckNotFound", "Canard {0:DuckName} n'existe pas !", "Duck Not Found", "Message Duck Not found with parameter {0:DuckName} ");
            MsgDuckNotHere  = category.CreateEntry("MsgDuckNotHere", "Canard {0:DuckName} n'est pas là !", "Duck Not Here", "Message Duck Not here with parameter {0:DuckName} ");
            MsgDuckFound    = category.CreateEntry("MsgDuckFound", "Canard {0:DuckName} ♥ ♥ ♥", "Duck Found", "Message Duck found with parameter {0:DuckName} ");
            MsgDuckDuplicate  = category.CreateEntry("MsgDuckDuplicate", "Canard {0:DuckName} existe déja !", "Duck Duplicate", "Message Duck already exists with paraméeter {0:DuckName} ");

        }
    }
}