// Project:         MapHours for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2024 Arshvir Goraya
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          Arshvir Goraya
// Origin Date:     September 6 2024
// Source Code:     https://github.com/ArshvirGoraya/Map-Hours

using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallConnect;
using DaggerfallWorkshop;
using static DaggerfallConnect.DFLocation;
using System;
using static DaggerfallWorkshop.Game.ExteriorAutomap;
using System.Collections.Generic;
using DaggerfallConnect.Utility;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using static DaggerfallWorkshop.DaggerfallLocation;
using System.Linq;

namespace MapHoursMod
{
    public class MapHours : MonoBehaviour
    {
        public BuildingNameplate[] buildingNameplatesRef;
        static readonly Dictionary<BuildingSummary, string[]> buildingsList = new Dictionary<BuildingSummary, string[]>();
        const string OPEN_TEXT = "(OPEN)";
        const string CLOSED_TEXT = "(CLOSED)";
        static bool justOpenedMap = false;
        static string locationDungeonName = null;
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
            ResetStorage();
        }
        void Start(){
            DaggerfallUI.UIManager.OnWindowChange += UIManager_OnWindowChangeHandler;
            PlayerGPS.OnMapPixelChanged += OnMapPixelChanged;
        }

        private void UIManager_OnWindowChangeHandler(object sender, EventArgs e){
            justOpenedMap = true;
        }

        private void OnMapPixelChanged(DFPosition mapPixel){
            ResetStorage();
        }

        static private void ResetStorage(){
            justOpenedMap = true;
            locationDungeonName = null;
            if (GameManager.Instance.PlayerGPS.CurrentLocation.HasDungeon){
                locationDungeonName = GetSpecialDungeonName(GameManager.Instance.StreamingWorld.CurrentPlayerLocationObject.Summary);
            }
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

        
        public StaticDoor[] FindAllDungeonDoors(){ // not optimal but more accurate
            DaggerfallStaticDoors[] doorCollections = GameManager.Instance.StreamingWorld.CurrentPlayerLocationObject.StaticDoorCollections;
            return DaggerfallStaticDoors.FindDoorsInCollections(doorCollections, DoorTypes.DungeonEntrance);
        }

        void SetToolTip(BuildingNameplate buildingNameplate, BuildingSummary buildingSummary){
            if (buildingsList.TryGetValue(buildingSummary, out _)){
                // * Building is stored.
                // * Check if need to update the open/close text:
                if (justOpenedMap){
                    buildingsList[buildingSummary][1] = GetBuildingOpenClosedText(buildingNameplate, buildingSummary);
                }
            }else{
                // * Building is NOT stored. 
                // * Create time and open/close text.
                Debug.Log($"MH: creating tooltip");
                if (GameManager.Instance.PlayerGPS.CurrentLocation.HasDungeon){

                    // string dungeonName= GetSpecialDungeonName(GameManager.Instance.StreamingWorld.CurrentPlayerLocationObject.Summary);
                    // Debug.Log($"MH: location dungeon name: {dungeonName}");
                    // Debug.Log($"MH: buildingNameplate.textLabel.ToolTipText: {buildingNameplate.textLabel.ToolTipText}");

                    // Debug.Log($"MH: location has dungeon Name: {GameManager.Instance.PlayerGPS.CurrentLocation.Dungeon.RecordElement.Header.LocationName}");


                    if (buildingSummary.BuildingType == BuildingTypes.Palace){
                        // Debug.Log($"MH: palace building key: {buildingSummary.buildingKey}");
                        // DaggerfallLocation location = GameManager.Instance.StreamingWorld.CurrentPlayerLocationObject;
                        // Debug.Log($"MH: daggerfall Location: {location}");

                        // DaggerfallStaticDoors[] doorCollections = location.StaticDoorCollections;
                        // foreach (DaggerfallStaticDoors doorCollection in doorCollections){
                        //     foreach (StaticDoor door in doorCollection.Doors){
                        //         Debug.Log($"MH: checking static door: {door}");
                        //         if (door.doorType == DoorTypes.DungeonEntrance){
                        //             Debug.Log($"MH: dungeon enterance found: {door} for buildingkey : {door.buildingKey}");
                        //         }
                        //     }
                        // }
                        

                        // GameManager.Instance.PlayerGPS.CurrentLocation.Exterior.Buildings;
                        // foreach (LocationDoorElement door in GameManager.Instance.PlayerGPS.CurrentLocation.Dungeon.RecordElement.Doors){
                        //     Debug.Log($"door {door} of {GameManager.Instance.PlayerGPS.CurrentLocation.Exterior.Buildings[door.BuildingDataIndex]}");
                        // }

                        // DaggerfallWorkshop.DaggerfallDungeon.GetSpecialDungeonName();
                        // Debug.Log($"MH: location has dungeon Name: {GameManager.Instance.PlayerGPS.CurrentLocation.Dungeon.RecordElement.Header.LocationName}");
                        // Debug.Log($"MH: location has dungeon ID: {GameManager.Instance.PlayerGPS.CurrentLocation.Dungeon.RecordElement.Header.LocationId}");
                        // Debug.Log($"MH: location has dungeon ExteriorID: {GameManager.Instance.PlayerGPS.CurrentLocation.Dungeon.RecordElement.Header.ExteriorLocationId}");
                        // Debug.Log($"MH: location has dungeon X: {GameManager.Instance.PlayerGPS.CurrentLocation.Dungeon.RecordElement.Header.X}");
                        // Debug.Log($"MH: location has dungeon Y: {GameManager.Instance.PlayerGPS.CurrentLocation.Dungeon.RecordElement.Header.Y}");
                        // Debug.Log($"MH: ===================================");
                        // Debug.Log($"MH: palace model: {buildingSummary.ModelID}");
                        // Debug.Log($"MH: palace key: {buildingSummary.buildingKey}");
                        // Debug.Log($"MH: palace name: {buildingNameplate.name}");
                        // Debug.Log($"MH: palace uniqueIndex: {buildingNameplate.uniqueIndex}");
                        // Debug.Log($"MH: building Object: {buildingNameplate.gameObject}");
                        // Debug.Log($"MH: building Object hash: {buildingNameplate.gameObject.GetHashCode()}");
                        // Debug.Log($"MH: building Object ID: {buildingNameplate.gameObject.GetInstanceID()}");
                        // Debug.Log($"MH: building Object type: {buildingNameplate.gameObject.GetType()}");
                        // Debug.Log($"MH: building Object x: {buildingNameplate.gameObject.transform.position.x}");
                        // Debug.Log($"MH: building Object y: {buildingNameplate.gameObject.transform.position.y}");
                        // Debug.Log($"MH: building Object local x: {buildingNameplate.gameObject.transform.localPosition.x}");
                        // Debug.Log($"MH: building Object local y: {buildingNameplate.gameObject.transform.localPosition.y}");
                        // foreach (var component in buildingNameplate.gameObject.GetComponents(typeof(Component))){
                        //     Debug.Log($"MH: palace components: {component}");
                        // }
                    }                    
                }

                buildingsList.Add(
                    buildingSummary, 
                    new string[2]{
                        GetBuildingHours(buildingNameplate, buildingSummary), // * Create Open/Close time for building.
                        GetBuildingOpenClosedText(buildingNameplate, buildingSummary) // * Get if building is open/closed:
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
        
        bool IsBuildingAlwaysAccessible(BuildingNameplate buildingNameplate, BuildingSummary buildingSummary){
            // * If opening and closing hours are the same (e.g., Taverns, Temples)
            if (PlayerActivate.openHours[(int)buildingSummary.BuildingType] == PlayerActivate.closeHours[(int)buildingSummary.BuildingType] % 25){ // If 25 reset to 0.
                return true;
            }
            // * Depends on Guild rank:
            if (GameManager.Instance.GuildManager.GetGuild(buildingSummary.FactionId).HallAccessAnytime()){
                return true;
            }

            if (buildingNameplate.textLabel.ToolTipText.Equals(locationDungeonName)){
                return true;
            }

            return false;
        }


        static public string GetSpecialDungeonName(LocationSummary summary)
        {
            string dungeonName = string.Empty;
            if (summary.RegionName == "Daggerfall" && summary.LocationName == "Daggerfall")
                dungeonName = DaggerfallUnity.Instance.TextProvider.GetText(475);
            else if (summary.RegionName == "Wayrest" && summary.LocationName == "Wayrest")
                dungeonName = DaggerfallUnity.Instance.TextProvider.GetText(476);
            else if (summary.RegionName == "Sentinel" && summary.LocationName == "Sentinel")
                dungeonName = DaggerfallUnity.Instance.TextProvider.GetText(477);
            else
                dungeonName = summary.LocationName;

            return dungeonName.TrimEnd('.');
        }

        string GetBuildingOpenClosedText(BuildingNameplate buildingNameplate, BuildingSummary buildingSummary){
            if (!MapHoursSettings.GetBool(buildingSummary.BuildingType.ToString(), "ShowOpenClosed")){ return ""; }

            if (MapHoursSettings.GetBool("ToolTipsExperimental", "DontShowOpenClosedForAlwaysAccessible") &&
                IsBuildingAlwaysAccessible(buildingNameplate, buildingSummary)
                ){
                    return "";
            }

            if (MapHoursSettings.GetBool("ToolTips", "OpenIfUnlocked")){ 
                if (IsBuildingLocked(buildingNameplate, buildingSummary)){ return Environment.NewLine + CLOSED_TEXT; }
                return Environment.NewLine + OPEN_TEXT;
             }else{
                if (!PlayerActivate.IsBuildingOpen(buildingSummary.BuildingType)){ return Environment.NewLine + CLOSED_TEXT; }
                return Environment.NewLine + OPEN_TEXT;
             }
        }

        string GetBuildingHours(BuildingNameplate buildingNameplate, BuildingSummary buildingSummary){
            if (!MapHoursSettings.GetBool(buildingSummary.BuildingType.ToString(), "ShowHours")){ return ""; }

            if (MapHoursSettings.GetBool("ToolTipsExperimental", "DontShowHoursForAlwaysAccessible") &&
                IsBuildingAlwaysAccessible(buildingNameplate, buildingSummary)
                ){
                    return "";
            }

            return Environment.NewLine + $"({ConvertTime(PlayerActivate.openHours[(int)buildingSummary.BuildingType])} - {ConvertTime(PlayerActivate.closeHours[(int)buildingSummary.BuildingType])})";
        }

        // * Logic taken from PlayerActivate ActivateBuilding().
        // bool IsBuildingLocked(BuildingSummary buildingSummary){
        //     // * See if open right now: (includes holidays + guild membership + quest)]
        //     return (!GameManager.Instance.PlayerActivate.BuildingIsUnlocked(buildingSummary) && 
        //         buildingSummary.BuildingType < DFLocation.BuildingTypes.Temple
        //         && buildingSummary.BuildingType != DFLocation.BuildingTypes.HouseForSale);
        // }

        bool IsBuildingLocked(BuildingNameplate buildingNameplate, BuildingSummary buildingSummary){
            if (buildingNameplate.textLabel.ToolTipText.Equals(locationDungeonName)){
                return false;
            }

            // * See if open right now: (includes holidays + guild membership + quest)]
            return !GameManager.Instance.PlayerActivate.BuildingIsUnlocked(buildingSummary);
        }

        bool BuildingIsDungeon(BuildingSummary buildingSummary){
            // if (GameManager.Instance.PlayerGPS.CurrentLocation.HasDungeon){
            //     string dungeonName= GetSpecialDungeonName(GameManager.Instance.StreamingWorld.CurrentPlayerLocationObject.Summary);
            //     Debug.Log($"MH: location dungeon name: {dungeonName}");
            //     Debug.Log($"MH: buildingNameplate.textLabel.ToolTipText: {buildingNameplate.textLabel.ToolTipText}");
            //     return true;
            // }
            // ModelID
            // buildingKey
            return false;
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
