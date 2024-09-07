using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using static DaggerfallWorkshop.Game.DaggerfallUI;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallWorkshop.Utility;
using DaggerfallConnect;
using DaggerfallWorkshop;
using static DaggerfallConnect.DFLocation;
using System;
using static DaggerfallWorkshop.Game.ExteriorAutomap;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using DaggerfallWorkshop.Game.UserInterface;
using UnityScript.Lang;
using System.Xml;
using DaggerfallConnect.Utility;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using DaggerfallWorkshop.Game.Guilds;

namespace MapHoursMod
{
    public class MapHours : MonoBehaviour
    {
        public BuildingNameplate[] buildingNameplatesRef;
        static readonly Dictionary<BuildingSummary, string[]> buildingsList = new Dictionary<BuildingSummary, string[]>();
        const string OPEN_TEXT = "(OPEN)";
        const string CLOSED_TEXT = "(CLOSED)";
        static bool justOpenedMap = false;
        static ModSettings MapHoursSettings;

        private static Mod mod;
        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams){
            mod = initParams.Mod;
            var go = new GameObject(mod.Title);
            go.AddComponent<MapHours>();
            mod.LoadSettingsCallback = LoadSettings;
            mod.LoadSettings();
            mod.IsReady = true;
        }

        // * Raised when user changes mod settings.
        static void LoadSettings(ModSettings modSettings, ModSettingsChange change){
            MapHoursSettings = modSettings;
            justOpenedMap = true;
            buildingsList.Clear();
        }
        void Start(){
            DaggerfallUI.UIManager.OnWindowChange += UIManager_OnWindowChangeHandler;
            PlayerGPS.OnMapPixelChanged += OnMapPixelChanged;
        }

        private void UIManager_OnWindowChangeHandler(object sender, EventArgs e){
            justOpenedMap = true;
        }

        private void OnMapPixelChanged(DFPosition mapPixel){
            buildingsList.Clear();
        }

        private void LateUpdate(){
            if (!(DaggerfallUI.UIManager.TopWindow is DaggerfallExteriorAutomapWindow)){
                return;
            }
            foreach (var buildingNameplate in ExteriorAutomap.instance.buildingNameplates){
                if (IsBuildingSupported(((BuildingSummary)buildingNameplate.textLabel.Tag).BuildingType)){
                    // * If first building has the same Label as the stored, no need to update any. Unless is a forceupdate.
                    if (buildingNameplate.textLabel.ToolTipText.EndsWith(")")){ // * If there is an event that triggers when automap is re-rendered, use that instead.
                        return;
                    }
                    SetToolTip(buildingNameplate, (BuildingSummary)buildingNameplate.textLabel.Tag);
                }
            }
            justOpenedMap = false;
        }

        void SetToolTip(BuildingNameplate buildingNameplate, BuildingSummary buildingSummary){
            if (buildingsList.TryGetValue(buildingSummary, out _)){
                // * Building is stored.
                // * Check if need to update the open/close text:
                if (justOpenedMap){
                    buildingsList[buildingSummary][1] = GetBuildingOpenClosedText(buildingSummary);
                }
            }else{
                // * Building is NOT stored. 
                // * Create time and open/close text.
                Debug.Log($"MST: creating tooltip");
                GetBuildingOpenClosedText(buildingSummary);
                buildingsList.Add(
                    buildingSummary, 
                    new string[2]{
                        GetBuildingHours(buildingSummary), // * Create Open/Close time for building.
                        GetBuildingOpenClosedText(buildingSummary) // * Get if building is open/closed:
                    }
                );
            }
            buildingNameplate.textLabel.ToolTipText += GetStoredToolTipText(buildingSummary);
        }

        string GetStoredToolTipText(BuildingSummary buildingSummary){
            if (MapHoursSettings.GetBool("ToolTips", "HoursAboveOpenClosed")){ 
                return buildingsList[buildingSummary][0] + buildingsList[buildingSummary][1];
            }
            return buildingsList[buildingSummary][1] + buildingsList[buildingSummary][0];
        }
        
        bool IsBuildingAlwaysAccessible(BuildingSummary buildingSummary){
            // * If opening and closing hours are the same (e.g., Taverns, Temples)
            if (PlayerActivate.openHours[(int)buildingSummary.BuildingType] == PlayerActivate.closeHours[(int)buildingSummary.BuildingType] % 25){ // If 25 reset to 0.
                return true;
            }
            // * Depends on Guild rank:
            if (GameManager.Instance.GuildManager.GetGuild(buildingSummary.FactionId).HallAccessAnytime()){
                return true;
            }

            return false;
        }

        string GetBuildingOpenClosedText(BuildingSummary buildingSummary){
            if (!MapHoursSettings.GetBool(buildingSummary.BuildingType.ToString(), "ShowOpenClosed")){ return ""; }

            if (MapHoursSettings.GetBool("ToolTips", "DontShowOpenClosedForAlwaysAccessible") &&
                IsBuildingAlwaysAccessible(buildingSummary)
                ){
                    return "";
            }

            if (MapHoursSettings.GetBool("ToolTips", "OpenIfUnlocked")){ 
                if (IsBuildingLocked(buildingSummary)){ return Environment.NewLine + CLOSED_TEXT; }
                return Environment.NewLine + OPEN_TEXT;
             }else{
                if (!PlayerActivate.IsBuildingOpen(buildingSummary.BuildingType)){ return Environment.NewLine + CLOSED_TEXT; }
                return Environment.NewLine + OPEN_TEXT;
             }
        }

        string GetBuildingHours(BuildingSummary buildingSummary){
            if (!MapHoursSettings.GetBool(buildingSummary.BuildingType.ToString(), "ShowHours")){ return ""; }

            if (MapHoursSettings.GetBool("ToolTips", "DontShowHoursForAlwaysAccessible") &&
                IsBuildingAlwaysAccessible(buildingSummary)
                ){
                    return "";
            }

            return Environment.NewLine + $"({ConvertTime(PlayerActivate.openHours[(int)buildingSummary.BuildingType])} - {ConvertTime(PlayerActivate.closeHours[(int)buildingSummary.BuildingType])})";
        }

        // * Logic taken from PlayerActivate ActivateBuilding().
        bool IsBuildingLocked(BuildingSummary buildingSummary){
            // * See if open right now: (includes holidays + guild membership + quest)]
            return (!GameManager.Instance.PlayerActivate.BuildingIsUnlocked(buildingSummary) && 
                buildingSummary.BuildingType < DFLocation.BuildingTypes.Temple
                && buildingSummary.BuildingType != DFLocation.BuildingTypes.HouseForSale);
        }

        string ConvertTime(int hour){
            if (hour >= 24) {hour = 0;}
            if (MapHoursSettings.GetBool("ToolTips", "Use12HourTimeFormatting")){
                return new DateTime(1, 1, 1, hour, 0, 0).ToString("hh:mm tt"); 
            }else{
                return new DateTime(1, 1, 1, hour, 0, 0).ToString("HH:mm");
            }
        }

        bool IsBuildingSupported(BuildingTypes buildingType){
            if (MapHoursSettings.GetBool(buildingType.ToString(), "ShowHours") || MapHoursSettings.GetBool(buildingType.ToString(), "ShowOpenClosed")){ return true; }
            return false;
        }
    }
}
