// Project:         MapHours for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2024 Arshvir Goraya
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          Arshvir Goraya
// Origin Date:     September 6 2024
// Source Code:     https://github.com/ArshvirGoraya/Map-Hours

using System;
using System.Collections.Generic;
using UnityEngine;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using static DaggerfallWorkshop.Game.ExteriorAutomap;
using static DaggerfallWorkshop.DaggerfallLocation;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using static DaggerfallConnect.DFLocation;
using DaggerfallConnect.Utility;
using Mono.Cecil;
using DaggerfallWorkshop.Game.UserInterface;
using UnityEditor;
using TMPro;
using System.Reflection;

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
        // const float NamePlateTextScaleDefault = 3.159375f;
        // readonly Vector2 ToolTipScaleDefault = new Vector2(1.0f, 1.0f);
        // readonly static float NamePlateTextScale = 3f;
        // readonly static Vector2 ToolTipScale = new Vector2(1.5f, 1.5f);
        static float toolTipSize = 1;
        // static DaggerfallFont toolTipFont = DaggerfallUI.DefaultFont;
        // static DaggerfallFont toolTipFont = DaggerfallUI.DefaultFont;
        // static DaggerfallFont newFont = DaggerfallUI.DefaultFont;
        // static TMP_FontAsset customTmpFont = Resources.Load<TMP_FontAsset>("FONT0003-SDF-copy");
        PropertyInfo textScaleInfo = typeof(ToolTip).GetProperty("TextScale", BindingFlags.Public | BindingFlags.Instance);
        bool textScaleExists;
        ////////////////////////////////////
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
        // static DaggerfallFont GetToolTipFont(int font_num){
        //     switch (font_num)
        //     {
        //         case 0: return DaggerfallUI.LargeFont;
        //         case 1: return DaggerfallUI.TitleFont;
        //         case 2: return DaggerfallUI.SmallFont;
        //         case 3: return DaggerfallUI.DefaultFont;
        //         case 4: return DaggerfallUI.Instance.GetFont(DaggerfallFont.FontName.FONT0004);
        //         default: return DaggerfallUI.DefaultFont;
        //     }
        // }
        static void LoadSettings(ModSettings modSettings, ModSettingsChange change){
            MapHoursSettings = modSettings;
            // toolTipFont = GetToolTipFont(MapHoursSettings.GetInt("ToolTips", "ToolTipFont"));
            toolTipSize = MapHoursSettings.GetFloat("ToolTips", "ToolTipSize");
            ExteriorAutomap.instance.RevealUndiscoveredBuildings = MapHoursSettings.GetBool("ToolTips", "RevealUndiscoveredBuildings");
            ResetStorage();
        }
        void Start(){
            DaggerfallUI.UIManager.OnWindowChange += UIManager_OnWindowChangeHandler;
            DaggerfallWorkshop.PlayerGPS.OnMapPixelChanged += OnMapPixelChanged;
            textScaleExists = textScaleInfo != null;
            // newFont = new DaggerfallFont(DaggerfallFont.FontName.FONT0003);
            // newFont.SDFFontInfo;

            // Font customFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/Game/Mods/MapHours/Fonts/KingthingsPetrock.tff");
            // Font customFont = Resources.Load<Font>("Fonts/Kingthings Petrock");
            // customTmpFont = Resources.Load<TMP_FontAsset>("FONT0003-SDF-copy");
            
            // if (customTmpFont == null){
            //     Debug.Log($"map hours - custom font not loaded");
            // }
            // Debug.Log($"map hours - customFont: {customTmpFont}");
        }
        private void UIManager_OnWindowChangeHandler(object sender, EventArgs e){
            justOpenedMap = true;
        }
        static private void OnMapPixelChanged(DFPosition mapPixel){
            if (!GameManager.Instance.StateManager.GameInProgress) { return; } // * Required or will generate errors in Player.log
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
        ////////////////////////////////////
        private void LateUpdate(){
            if (!(DaggerfallUI.UIManager.TopWindow is DaggerfallExteriorAutomapWindow)){
                return;
            }
            foreach (var buildingNameplate in ExteriorAutomap.instance.buildingNameplates){
                if (textScaleExists){
                    buildingNameplate.textLabel.ToolTip.TextScale = toolTipSize;

                    // buildingNameplate.textLabel.TextScale = NamePlateTextScale;
                    // Debug.Log($"autosize mode: {buildingNameplate.textLabel.ToolTip.AutoSize}");

                    // buildingNameplate.textLabel.ToolTip.AutoSize = AutoSizeModes.ScaleFreely;
                    // buildingNameplate.textLabel.ToolTip.AutoSize = AutoSizeModes.ScaleToFit;
                    // buildingNameplate.textLabel.ToolTip.Scale = ToolTipScale;

                    // buildingNameplate.textLabel.ToolTip.Font = toolTipFont;
                    // buildingNameplate.textLabel.ToolTip.Font = newFont;

                    // MapHoursToolTip toolTipOverwrite = new MapHoursToolTip();
                    // toolTipOverwrite.Draw(buildingNameplate.textLabel.ToolTipText);

                    // buildingNameplate.textLabel.ToolTip.Font.LoadSDFFontAsset("FONT0003-SDF-copy");
                    // buildingNameplate.textLabel.ToolTip.Font.LoadSDFFontAsset("Kingthings Petrock-copy");
                }
                
                if (IsBuildingSupported(((BuildingSummary)buildingNameplate.textLabel.Tag).BuildingType)){
                    // * If first building has the same Label as the stored.
                    if (buildingNameplate.textLabel.ToolTipText.EndsWith(")")){ 
                        // * If there is an event that triggers when automap is re-rendered, use that instead instead of checking ")".
                        return;
                    }
                    SetToolTip(buildingNameplate, (BuildingSummary)buildingNameplate.textLabel.Tag);
                }
            }
            justOpenedMap = false;
        }
        // public StaticDoor[] FindAllDungeonDoors(){ // not optimal but more accurate
        //     DaggerfallStaticDoors[] doorCollections = GameManager.Instance.StreamingWorld.CurrentPlayerLocationObject.StaticDoorCollections;
        //     return DaggerfallStaticDoors.FindDoorsInCollections(doorCollections, DoorTypes.DungeonEntrance);
        // }
        void SetToolTip(BuildingNameplate buildingNameplate, BuildingSummary buildingSummary){
            if (buildingsList.TryGetValue(buildingSummary, out _)){
                // * Building is stored: Check if need to update the open/close text:
                if (justOpenedMap){
                    buildingsList[buildingSummary][1] = GetBuildingOpenClosedText(buildingNameplate, buildingSummary);
                }
            }else{
                string[] buildingQuality = GetBuildingQualityText(buildingSummary);

                // * Building is NOT stored: Create time and open/close text.
                buildingsList.Add(
                    buildingSummary, 
                    new string[4]{
                        GetBuildingHours(buildingNameplate, buildingSummary), // * Create Open/Close time for building.
                        GetBuildingOpenClosedText(buildingNameplate, buildingSummary), // * Get if building is open/closed.
                        buildingQuality[0],
                        buildingQuality[1],
                    }
                );
            }
            // * Set tooltips text.
            buildingNameplate.textLabel.ToolTipText += GetStoredToolTipText(buildingSummary);
        }
        string GetStoredToolTipText(BuildingSummary buildingSummary){
            if (
                buildingsList[buildingSummary][0].Length == 0 && 
                buildingsList[buildingSummary][1].Length == 0 && 
                buildingsList[buildingSummary][2].Length == 0 && 
                buildingsList[buildingSummary][3].Length == 0
                ){
                return "";
            }
            string returnText = "";
            string hours = buildingsList[buildingSummary][0];
            string openClosed = buildingsList[buildingSummary][1];
            string qualityText = GetQualityText(buildingSummary);

            if (qualityText.Length > 0 && MapHoursSettings.GetBool("ToolTips", "ShowQualityNextToBuildingName")){
                returnText += " " + qualityText;
            }
            if (MapHoursSettings.GetBool("ToolTips", "HoursBeforeOpenClosed")){
                if (hours.Length != 0){
                    returnText += Environment.NewLine + hours; 
                    if (MapHoursSettings.GetBool("ToolTips", "ShowHoursAndOpenClosedInSameLine")){
                        if (openClosed.Length != 0){ returnText += " " + openClosed; }
                    }else{
                        if (openClosed.Length != 0){ returnText += Environment.NewLine + openClosed; }
                    }
                }else{
                    if (openClosed.Length != 0){ returnText += Environment.NewLine + openClosed; }
                }
            }else{
                if (openClosed.Length != 0){
                    returnText += Environment.NewLine + openClosed; 
                    if (MapHoursSettings.GetBool("ToolTips", "ShowHoursAndOpenClosedInSameLine")){
                        if (hours.Length != 0){ returnText += " " + hours; }
                    }else{
                        if (hours.Length != 0){ returnText += Environment.NewLine + hours; }
                    }
                }else{
                    if (hours.Length != 0){ returnText += Environment.NewLine + hours; }
                }
            }

            if (qualityText.Length > 0 && !MapHoursSettings.GetBool("ToolTips", "ShowQualityNextToBuildingName")){
                returnText += Environment.NewLine + qualityText;
            }
            return returnText;
        }

        string GetQualityText(BuildingSummary buildingSummary){
            if (buildingsList[buildingSummary][2].Length == 0 && buildingsList[buildingSummary][3].Length == 0){
                return "";
            }
            string returnText = "";
            string qualityNumber = buildingsList[buildingSummary][2];
            string qualityText = buildingsList[buildingSummary][3];
            if (MapHoursSettings.GetBool("ToolTips", "QualityNumberBeforeQualityText")){
                if (qualityNumber.Length != 0){
                    returnText += qualityNumber;
                }
                if (qualityText.Length != 0){
                    if (qualityNumber.Length != 0){
                        returnText += " ";    
                    }
                    returnText += qualityText;
                }
            }else{
                if (qualityText.Length != 0){
                    returnText += qualityText;
                }
                if (qualityNumber.Length != 0){
                    if (qualityText.Length != 0){
                        returnText += " ";    
                    }
                    returnText += qualityNumber;
                }
            }

            return returnText;
        }

        ////////////////////////////////////
        string[] GetBuildingQualityText(BuildingSummary buildingSummary){
            string[] qualityText = {"", ""};
            // if (!IsBuildingStore(buildingSummary)){
            //     return qualityText;
            // }
            if (!IsQualityTextSupported(buildingSummary.BuildingType)){
                return qualityText;
            }
            //
            int shopQuality = GetShopQualityNumber(buildingSummary);
            if (MapHoursSettings.GetBool(buildingSummary.BuildingType.ToString(), "ShowQualityNumber")){
                qualityText[0] = $"({shopQuality})";
            }
            if (MapHoursSettings.GetBool(buildingSummary.BuildingType.ToString(), "ShowQualityText")){
                qualityText[1] = GetShopQualityText(shopQuality);
            }
            return qualityText;
        }
        int GetShopQualityNumber(BuildingSummary buildingSummary){
            if (buildingSummary.Quality <= 3) return 1;
            else if (buildingSummary.Quality <= 7) return 2;
            else if (buildingSummary.Quality <= 13) return 3;
            else if (buildingSummary.Quality <= 17) return 4;
            else return 5;
        }
        string GetShopQualityText(int quality){
            switch (quality){
                case 1: return "(Rusty)";
                case 2: return "(Sturdy)";
                case 3: return "(Practical)";
                case 4: return "(Appointed)";
                default: return "(Soothing)";
            }
        }
        bool IsQualityTextSupported(BuildingTypes buildingType){
            return  (
                MapHoursSettings.GetBool(buildingType.ToString(), "ShowQualityNumber") || 
                MapHoursSettings.GetBool(buildingType.ToString(), "ShowQualityText")
            );
        }
        // bool IsBuildingStore(BuildingSummary buildingSummary){
        //     return (
        //         buildingSummary.BuildingType == BuildingTypes.Alchemist ||
        //         buildingSummary.BuildingType == BuildingTypes.Armorer ||
        //         buildingSummary.BuildingType == BuildingTypes.Bookseller ||
        //         buildingSummary.BuildingType == BuildingTypes.GeneralStore ||
        //         buildingSummary.BuildingType == BuildingTypes.ClothingStore ||
        //         buildingSummary.BuildingType == BuildingTypes.GemStore ||
        //         buildingSummary.BuildingType == BuildingTypes.PawnShop ||
        //         buildingSummary.BuildingType == BuildingTypes.WeaponSmith ||
        //         buildingSummary.BuildingType == BuildingTypes.FurnitureStore
        //     );
        // }
        bool IsBuildingAlwaysAccessible(BuildingNameplate buildingNameplate, BuildingSummary buildingSummary){
            // * If opening and closing hours are the same (e.g., Taverns, Temples)
            if (PlayerActivate.openHours[(int)buildingSummary.BuildingType] == PlayerActivate.closeHours[(int)buildingSummary.BuildingType] % 25){ // If 25 reset to 0.
                return true;
            }
            // * Depends on Guild rank:
            if (GameManager.Instance.GuildManager.GetGuild(buildingSummary.FactionId).HallAccessAnytime()){
                return true;
            }
            // * Is a dungeon (e.g., Castle Daggerfall, Palace Sentinel, etc.)
            if (buildingNameplate.textLabel.ToolTipText.Equals(locationDungeonName)){
                return true;
            }
            return false;
        }
        // * Taken from: DaggerfallDungeon.cs
        static public string GetSpecialDungeonName(LocationSummary summary){
            string dungeonName;
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

            if (MapHoursSettings.GetBool("ToolTips", "DontShowOpenClosedForAlwaysAccessible") &&
                IsBuildingAlwaysAccessible(buildingNameplate, buildingSummary)
            ){ return ""; }

            if (MapHoursSettings.GetBool("ToolTips", "OpenIfUnlocked")){
                if (IsBuildingLocked(buildingNameplate, buildingSummary)){ return CLOSED_TEXT; }
                else { return OPEN_TEXT; } 
             }else{
                if (!PlayerActivate.IsBuildingOpen(buildingSummary.BuildingType)){ return CLOSED_TEXT; }
                else { return OPEN_TEXT; } 
             }
        }
        string GetBuildingHours(BuildingNameplate buildingNameplate, BuildingSummary buildingSummary){
            if (!MapHoursSettings.GetBool(buildingSummary.BuildingType.ToString(), "ShowHours")){ return ""; }

            if (MapHoursSettings.GetBool("ToolTips", "DontShowHoursForAlwaysAccessible") &&
                IsBuildingAlwaysAccessible(buildingNameplate, buildingSummary)
            ){ return "";}

            return $"({ConvertTime(PlayerActivate.openHours[(int)buildingSummary.BuildingType])} - {ConvertTime(PlayerActivate.closeHours[(int)buildingSummary.BuildingType])})";
        }
        bool IsBuildingLocked(BuildingNameplate buildingNameplate, BuildingSummary buildingSummary){
            if (buildingNameplate.textLabel.ToolTipText.Equals(locationDungeonName)){ return false; }
            // * See if open right now: (includes holidays + guild membership + quest)]
            return !GameManager.Instance.PlayerActivate.BuildingIsUnlocked(buildingSummary);
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
            if (IsQualityTextSupported(buildingType)){ return true; }
            return false;
        }
    }
}
