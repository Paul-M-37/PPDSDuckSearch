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
              public static Dictionary<string, bool> _excludedScene { get; set; } = new Dictionary<string, bool>()
             {

                {"Intro",false},
                {"Loading",false},
                {"Bootstrap",false}
            };
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
            // get Duck List
            GetDuckList();

            // add alien on hyppospace
            if ( _sceneName == "hyppospace" )
            {                
                if( _ducks.ContainsKey("Duck46Alien"))
                {
                    _ducks["Duck46Alien"] = 0;
                }
                else
                {
                    _ducks.Add("Duck46Alien", 0);    
                }
                
            }
         }
        public static void GetDuckList()
        // public static Dictionary<string, Dictionary<string, string>>? GetDuckList( )
        {
            if (!CustomSearch.isActive()) { return ; }

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

        public static int UpdateDuckList(string duckId, GameObject? newDuck)
        {
            if (!CustomSearch.isActive()) { return 0; }

        

            // update duck list with new spawned duck
            int duckIndex = -1;
            if (newDuck != null)
            {
                if (_generalManager != null) { duckIndex = _generalManager.GetDuckIndex(newDuck); }
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
        static public void AddWaitingDucks()
        {
            if (!CustomSearch.isActive()) { return; }
            if (_waitingDucks == null) { return; }

            // manage ducks loaded during Loadin screen (no spawn)
            if (_waitingDucks != null && _generalManager != null && _ducks != null)
            {
                foreach (var duck in _waitingDucks)
                {
                    var duckIndex = UpdateDuckList(duck.Key, duck.Value);
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
            if (!CustomSearch.isActive()) { return; }
            if (_ResumeButton == null) { return; }
            MelonEvents.OnGUI.Unsubscribe(DrawMenu);

            // Create a fake pointer event and simulate click on RESUME Button to exit Pause Menu
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            Button targetButton = _ResumeButton.Button;
            if (targetButton != null) { ExecuteEvents.Execute(targetButton.gameObject, pointerData, ExecuteEvents.pointerClickHandler); }
        }
        public static string InputPrompt()
        {
            if (!CustomSearch.isActive()) { return ""; }
            CustomSearch.SearchDuckName = "";
            MelonEvents.OnGUI.Subscribe(DrawMenu, 100); // The higher the value, the lower the priority.
            return CustomSearch.SearchDuckName;
        }


        public static string DuckSearch(string? duckName = null, string? duckId = null, bool AllowPrompt = true) //Void can be used for other mods
        {
            if (!CustomSearch.isActive()) { return ""; }
            
           DuckManager? currentduck = null;
            String? MyMessage = null;

            // id to search , default parameter id
            String? SearchedDuckId = duckId;
            String? SearchedName = duckName;

            // No general mager Exit
            if (_generalManager == null) { return ""; }

            // if no Duck Id nor Duck Name , prompt if prompt is active 
            if (SearchedDuckId == null && (SearchedName == null || SearchedName == ""))
            {
                if (AllowPrompt) { SearchedName = InputPrompt(); }
                return "";
            }
          
            // define Duck id to search if not provided.
            if (SearchedDuckId == null && SearchedName != null && _duckNames.ContainsKey(SearchedName))
            {
                SearchedDuckId = _duckNames[SearchedName];
            }
            else if (SearchedDuckId == null)
            {
                MyMessage = String.Format(CustomSearchSettings.MsgDuckNotFound.Value, SearchedName);
            }
            

            // get index of Duck in the scene
            if (SearchedDuckId != null && _ducks.ContainsKey(SearchedDuckId))
            {
                var duckIndex = _ducks[SearchedDuckId];
                if (duckIndex < 0) { }
                else
                {
                    currentduck = _generalManager.GetDuck(duckIndex);
                }
            }

            // if Duck Id and not found in scene
            if (SearchedDuckId != null && currentduck == null)
            {
                MyMessage = String.Format(CustomSearchSettings.MsgDuckNotHere.Value, SearchedName);
            }
            else
            {
                // show duck                
                _generalManager.SwitchToDucksView();
                _generalManager.SwitchOnDucks();
                _generalManager.ChangeCurrentDuck(currentduck);
                _generalManager.ResetSwitchCounter();
                MyMessage = String.Format(CustomSearchSettings.MsgDuckFound.Value, SearchedName);
            }


            // Send message to screen
            if (MyMessage != null)
            {
                _generalManager.MsgManager.PushMsg(MyMessage);
            }
            else
            {
                MyMessage = "";
            }
             return MyMessage; 

        }
        public static void AddWaitingDuck(string duckId, GameObject newDuck)
        {
            if (!CustomSearch.isActive()) { return ; }
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
        public static int updateDuckList(string duckId, GameObject? newDuck)
        {
            if (!CustomSearch.isActive()) { return 0; }

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
                if (oldname != "" && oldname != null && _duckNames != null && _duckNames.ContainsKey(oldname)
                && oldname != LowerDuckName)
                {
                    _duckNames.Remove(oldname);                 
                }
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
                    if( _duckNames[LowerDuckName] != duckID ) { hasduplicate = true; }
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
                if (_sceneName == null || CustomSearch._excludedScene.ContainsKey(_sceneName)) { CustomSearch.AddWaitingDuck(duckId, newDuck); return; }
                CustomSearch.AddWaitingDucks();
                CustomSearch.updateDuckList(duckId, newDuck);
            }

        }
        [HarmonyPatch(typeof(DuckManager), "NameChanged")]
        public class DuckManager_NameChanged
        {
            static void Postfix(ref DuckManager __instance, string duckID, string newName, bool sendChangeToPeers)
            {
                if (!CustomSearch.isActive()) { return; }
                
                // At name change
                DuckManager Duck = __instance;
    
                // update duck name list
                if (UpdateDuckNameList(duckID, newName))
                {
                    var MyMessage = String.Format(CustomSearchSettings.MsgDuckDuplicate.Value, newName);
                    CustomSearch.Msg( MyMessage );
                    if (_generalManager != null && _generalManager.MsgManager != null)
                    {
                        _generalManager.MsgManager.PushMsg(MyMessage);
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

                if (!CustomSearch.isActive()) { return; }
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
