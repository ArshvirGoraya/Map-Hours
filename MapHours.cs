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
            Debug.Log($"MST: building list cleared");
        }
        void Start(){
            DaggerfallUI.UIManager.OnWindowChange += UIManager_OnWindowChangeHandler;
            PlayerGPS.OnMapPixelChanged += OnMapPixelChanged;
        }

        private void UIManager_OnWindowChangeHandler(object sender, EventArgs e){
            // if (!(DaggerfallUI.UIManager.TopWindow is DaggerfallExteriorAutomapWindow)){ justOpenedMap = true; } // ! check not needed
            justOpenedMap = true;
        }

        private void OnMapPixelChanged(DFPosition mapPixel){
            buildingsList.Clear();
            Debug.Log($"MST: building list cleared");
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
                    Debug.Log($"MST: Update previous time");
                }
                else{
                    Debug.Log($"MST: NOT updating previous time");
                }
            }
            else{
                // * Building is NOT stored. 
                // * Create time and open/close text.
                Debug.Log($"MST: creating tooltip");
                GetBuildingOpenClosedText(buildingSummary);
                buildingsList.Add(
                    buildingSummary, 
                    new string[2]{
                        GetBuildingOpenCloseTime(buildingSummary), // * Create Open/Close time for building.
                        GetBuildingOpenClosedText(buildingSummary) // * Get if building is open/closed:
                    }
                );
            }
            buildingNameplate.textLabel.ToolTipText += GetStoredToolTipText(buildingSummary);
        }

        string GetStoredToolTipText(BuildingSummary buildingSummary){
            return Environment.NewLine + buildingsList[buildingSummary][0] + Environment.NewLine + buildingsList[buildingSummary][1];
        }
        
        string GetBuildingOpenClosedText(BuildingSummary buildingSummary){
            if (IsBuildingLocked(buildingSummary)){ return CLOSED_TEXT; }
            return OPEN_TEXT;
        }

        string GetBuildingOpenCloseTime(BuildingSummary buildingSummary){
            return $"({ConvertTime(PlayerActivate.openHours[(int)buildingSummary.BuildingType])} - {ConvertTime(PlayerActivate.closeHours[(int)buildingSummary.BuildingType])})";
        }

        bool IsBuildingLocked(BuildingSummary buildingSummary){
            // * See if open right now: (includes holidays + guild membership + quest)]
            return (!GameManager.Instance.PlayerActivate.BuildingIsUnlocked(buildingSummary) && 
                buildingSummary.BuildingType < DFLocation.BuildingTypes.Temple
                && buildingSummary.BuildingType != DFLocation.BuildingTypes.HouseForSale);
        }

        string ConvertTime(int hour){
            if (hour >= 24){
                return new DateTime(1, 1, 1, 0, 0, 0).ToString("hh:mm tt");
            }
            return new DateTime(1, 1, 1, hour, 0, 0).ToString("hh:mm tt");
        }

        bool IsBuildingSupported(BuildingTypes buildingType){
            return buildingType == BuildingTypes.Alchemist ||
            buildingType == BuildingTypes.Armorer ||
            buildingType == BuildingTypes.Bank ||
            buildingType == BuildingTypes.Bookseller ||
            buildingType == BuildingTypes.ClothingStore ||
            buildingType == BuildingTypes.FurnitureStore ||
            buildingType == BuildingTypes.GemStore ||
            buildingType == BuildingTypes.GeneralStore ||
            buildingType == BuildingTypes.Library ||
            buildingType == BuildingTypes.GuildHall ||
            buildingType == BuildingTypes.PawnShop ||
            buildingType == BuildingTypes.WeaponSmith ||
            buildingType == BuildingTypes.Temple ||
            buildingType == BuildingTypes.Palace ||
            //! These ones don't really need it.
            // buildingType == BuildingTypes.HouseForSale ||
            // buildingType == BuildingTypes.Town4 ||
            // buildingType == BuildingTypes.House1 ||
            // buildingType == BuildingTypes.House2 ||
            // buildingType == BuildingTypes.House3 ||
            // buildingType == BuildingTypes.House4 ||
            // buildingType == BuildingTypes.House5 ||
            // buildingType == BuildingTypes.House6 ||
            // buildingType == BuildingTypes.Town23 ||
            buildingType == BuildingTypes.Tavern;
        }
    }
}
