using Custom_Search;
using HarmonyLib;
using MelonLoader;
using System.Collections.Generic;
using UnityEngine;
using Il2Cpp;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;



[assembly: MelonInfo(typeof(CustomSearch), "CustomSearch", "0.1.1", "PaulM37", "")]
[assembly: MelonGame("Turbolento Games", "Placid Plastic Duck Simulator")]


namespace Custom_Search
{
    public class CustomSearch : MelonMod
    {
        internal static CustomSearch _instance { get; set; }
        private static GeneralManager _generalManager;
        private static DuckUIManager _DuckUIManager;
        private static MenuManager _menuManager;
        private static MenuButton _ResumeButton;
        private static string _sceneName;
        public static Dictionary<string, string> _duckNames { get; set; } = new Dictionary<string, string>();
        public static Dictionary<string, string> _duckRevertNames { get; set; } = new Dictionary<string, string>();
        public static Dictionary<string, int> _ducks { get; set; } = new Dictionary<string, int>();
        public static Dictionary<string, bool> _excludedScene { get; set; }
        public static DuckManager currentduck { get; set; }

        public static Dictionary<String, GameObject> _waitingDucks { get; set; }
        public static bool _DuckSearch { get; set; }
        public static string SearchDuckName { get; set; }
        public override void OnEarlyInitializeMelon()
        {
            _instance = this;
            
        }

        public override void OnInitializeMelon()
        {
            CustomSearchSettings.RegisterSettings();
            _DuckSearch = CustomSearchSettings.DuckSearch.Value;
            // set scene to exclude to process
            _excludedScene = new Dictionary<string, bool>()
             {

                {"Intro",false},
                {"Loading",false},
                {"Bootstrap",false}
            };

        }
        public static bool isActive()
        {
            return _DuckSearch;
        }
        public static void Msg(string Message, bool iserror = false)
        {
            if(CustomSearchSettings.NoMsgFlag.Value) {return;}
            if(iserror)
            {
                 _instance.LoggerInstance.Error(Message);
            }
            else
            {
               _instance.LoggerInstance.Msg(Message);
            }
            
            
        }
        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            // save scene name et check if process needed
            CustomSearch.Msg($"Init Scene {sceneName}");
            _sceneName = sceneName;
            if (_excludedScene.ContainsKey(sceneName)) { return; }
            if (!CustomSearch.isActive()) { return; }

            //Hook Menu Manager
            if (_menuManager == null)
            {
                _menuManager = Singleton<MenuManager>.I;
                if (_menuManager == null)
                {
                    CustomSearch.Msg("Menu Manager Didn't Hook!!",true);
                }
            }
            //Hook General Manager
            _generalManager = Singleton<GeneralManager>.I;
            if (_generalManager == null)
            {
                CustomSearch.Msg("General Manager Didn't Hook!!",true);
            }

            // Get Duck Names List
            if (_DuckUIManager == null)
            {
                _DuckUIManager = Singleton<DuckUIManager>.I;
                if (_DuckUIManager == null)
                {
                    CustomSearch.Msg("Duck UI Manager Didn't Hook!!",true);
                }
                else
                {

                    CustomSearch.Msg("Duck names from Duck UI Manager");
                    CustomSearch.Msg($"Duck names count = {_DuckUIManager.duckNames.Count}");
                    foreach (var duckname in _DuckUIManager.duckNames)
                    {
                        CustomSearch.Msg($"Duck {duckname.key} , {duckname.value} ");

                        // update lists of Duckname
                        CustomSearch.UpdateDuckNameList(duckname.key, duckname.value);
                        // add DUck in Duck List 
                        CustomSearch.updateDuckList(duckname.key, null);


                    }



                }
            }

        }
        public override void OnLateUpdate()
        {
            if (!CustomSearch.isActive()) { return; }

            // manage ducks loaded during Loadin screen (no spawn)
            if (_waitingDucks != null && _generalManager != null && _ducks != null)
            {
                foreach (var duck in _waitingDucks)
                {
                    var duckIndex = CustomSearch.updateDuckList(duck.Key, duck.Value);
                    if (duckIndex >= 0) { _waitingDucks.Remove(duck.Key); }
                }
            }

        }
        public static void DrawMenu()
        {
            if (!CustomSearch.isActive()) { return; }

            // Mod Screen
            GUILayout.BeginArea(new Rect(Screen.width / 24, Screen.height / 24, Screen.width / 24 + Screen.width / 6, Screen.height / 24 + Screen.height / 6));
            // Title
            GUILayout.Label(CustomSearchSettings.ModTitle.Value);
            // input box wih label
            GUILayout.BeginHorizontal();
            GUILayout.Label(CustomSearchSettings.NameLabel.Value);
            CustomSearch.SearchDuckName = GUILayout.TextField(CustomSearch.SearchDuckName, 50);
            GUILayout.EndHorizontal();
            // Submit Button
            if (GUILayout.Button(CustomSearchSettings.ButtonText.Value))
            {
                if (CustomSearch.SearchDuckName != null && SearchDuckName != "")
                {
                    DuckSearch(CustomSearch.SearchDuckName.ToLower());
                    CustomSearch.ResumeGame();
                }
            }
            GUILayout.EndArea();
        }
        public static void ResumeGame()
        {
            if (_ResumeButton == null) { return; }
            MelonEvents.OnGUI.Unsubscribe(DrawMenu);

            // Create a fake pointer event and simulate click on RESUME Button to exit Pause Menu
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            Button targetButton = _ResumeButton.Button;
            if (targetButton != null) { ExecuteEvents.Execute(targetButton.gameObject, pointerData, ExecuteEvents.pointerClickHandler); }
        }
        public static string InputPrompt()
        {
            CustomSearch.SearchDuckName = "";
            MelonEvents.OnGUI.Subscribe(DrawMenu, 100); // The higher the value, the lower the priority.
            return CustomSearch.SearchDuckName;
        }


        public static void DuckSearch(string duckName = null) //Void can be used for other mods
        {
            DuckManager currentduck = null;
            String MyMessage = null;
            if (duckName == null || duckName == "")
            {

                duckName = InputPrompt();
                return;
            }

            if (_duckNames.ContainsKey(duckName))
            {
                if (_ducks.ContainsKey(_duckNames[duckName]))
                {
                    var duckIndex = _ducks[_duckNames[duckName]];
                    if (duckIndex < 0) { }
                    else
                    {
                        currentduck = _generalManager.GetDuck(duckIndex);
                    }
                }
                if (currentduck == null)
                {
                    MyMessage = String.Format(CustomSearchSettings.MsgDuckNotHere.Value, duckName);
                }
                else
                {
                    _generalManager.ChangeCurrentDuck(currentduck);

                    MyMessage = String.Format(CustomSearchSettings.MsgDuckFound.Value, duckName);
                }
            }
            else
            {
                MyMessage = String.Format(CustomSearchSettings.MsgDuckNotFound.Value, duckName);
            }

            if (MyMessage != null)
            {
                CustomSearch.Msg(MyMessage);
                CustomSearch._generalManager.MsgManager.PushMsg(MyMessage);
            }

        }
        public static void AddWaitingDuck(string duckId, GameObject newDuck)
        {
            // add duck loaded during Loading screen to waiting list
            if (_waitingDucks == null)
            {

                _waitingDucks = new Dictionary<string, GameObject>() { { duckId, newDuck } };
            }
            else
            {
                _waitingDucks[duckId] = newDuck;
            }
        }
        public static int updateDuckList(string duckId, GameObject newDuck)
        {
            // update duck list with new spawned duck
            int duckIndex = -1;
            if (newDuck != null)
            {
                duckIndex = CustomSearch._generalManager.GetDuckIndex(newDuck);
            }
            if (_ducks.ContainsKey(duckId))
            {

                _ducks[duckId] = duckIndex;
            }
            else
            {
                _ducks.Add(duckId, duckIndex);
            }
            return duckIndex;
        }
        public static bool UpdateDuckNameList(string duckID, string duckName)
        {
            bool hasduplicate = false;
            String LowerDuckName = "";
            if(duckName != null) { LowerDuckName = duckName.ToLower(); }

            if (_duckRevertNames != null && _duckRevertNames.ContainsKey(duckID))
            {
                var oldname = _duckRevertNames[duckID];
                if(oldname != "" && oldname != null && _duckNames != null && _duckNames.ContainsKey(oldname)) { _duckNames.Remove(oldname); }
                _duckRevertNames[duckID] = LowerDuckName;
            }
            else
            {
                if (_duckRevertNames == null)
                {
                    _duckRevertNames = new Dictionary<string, string>() { { duckID, LowerDuckName } };
                }
                else
                {
                    _duckRevertNames.Add(duckID, LowerDuckName);
                }
            }

            if (LowerDuckName != "")
            {
                if (_duckNames != null && _duckNames.ContainsKey(LowerDuckName))
                {
                    hasduplicate = true;
                    _duckNames[LowerDuckName] = duckID;

                }
                else
                {
                    if (_duckNames == null)
                    {
                        _duckNames = new Dictionary<string, string>() { { LowerDuckName, duckID } };
                    }
                    else
                    {
                        _duckNames.Add(LowerDuckName, duckID);
                    }
                }
            }

            return hasduplicate;
        }
        [HarmonyPatch(typeof(GeneralManager), "OnDuckSpawned")]
        public class GeneralManager_DuckSpawned
        {
            static void Postfix(ref GeneralManager __instance, GameObject newDuck, string duckId, bool saveSpawn, string fixedName, int playerID)
            {
                if (!CustomSearch.isActive()) { return; }
                if (CustomSearch._excludedScene.ContainsKey(_sceneName)) { CustomSearch.AddWaitingDuck(duckId, newDuck); return; }

                CustomSearch.updateDuckList(duckId, newDuck);
            }

        }
        [HarmonyPatch(typeof(DuckManager), "LateUpdate")]
        public class DuckManager_LateUpdate
        {
            static void Postfix(ref DuckManager __instance)
            {
                if (!CustomSearch.isActive()) { return; }
                DuckManager Duck = __instance;

                // update duck name list
                if (CustomSearch.UpdateDuckNameList(Duck.duckID, Duck.DisplayName))
                {
                    var MyMessage = String.Format(CustomSearchSettings.MsgDuckDuplicate.Value, Duck.DisplayName);
                    CustomSearch._instance.LoggerInstance.Msg(MyMessage);
                    if (CustomSearch._generalManager != null && CustomSearch._generalManager.MsgManager != null)
                    {
                        CustomSearch._generalManager.MsgManager.PushMsg(MyMessage);
                    }
                }
            }

        }
        [HarmonyPatch(typeof(MenuManager), "ShowMenuScreen")]
        public class MenuManager_ShowMenuScreen
        {
            static void Postfix(ref MenuManager __instance, MainMenuType type)
            {
                CustomSearch.Msg($"Show Menu {type}");
                CustomSearch._menuManager = __instance;
                if (!CustomSearch.isActive()) { return; }
                // display Mod View on Pause Menu
                if (type == MainMenuType.Pause) { CustomSearch.InputPrompt(); }
                else { MelonEvents.OnGUI.Unsubscribe(DrawMenu); }

            }
        }
        [HarmonyPatch(typeof(MenuButton), "OnEnable")]
        public class MenuButton_Enable
        {
            static void Postfix(ref MenuButton __instance)
            {

                if (__instance.Button.name == "RESUME_Button")
                {
                    if (__instance._baseScreen != null && __instance._baseScreen.name == "PauseMenu")
                    {
                        CustomSearch.Msg($"Menu Button {__instance.Button.name} / {__instance._baseScreen.name}");
                        CustomSearch._ResumeButton = __instance;

                    }
                }
            }
        }
    }
}
