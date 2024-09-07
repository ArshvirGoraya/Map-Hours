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

namespace MapShopTimesMod
{
    public class MapShopTimes : MonoBehaviour
    {
        public BuildingNameplate[] buildingNameplatesRef;
        // Dictionary<BuildingSummary, string> buildingsList = new Dictionary<BuildingSummary, string>();
        Dictionary<BuildingSummary, string[]> buildingsList = new Dictionary<BuildingSummary, string[]>();

        const string OPEN_TEXT = "OPEN";
        const string CLOSED_TEXT = "CLOSED";

        string buildingTime;
        string openTime;
        string closeTime;
        DaggerfallDateTime previousTime;

        private static Mod mod;

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            mod = initParams.Mod;

            var go = new GameObject(mod.Title);
            go.AddComponent<MapShopTimes>();

            mod.IsReady = true;
        }

        // private void Start(){}

        private void LateUpdate(){
            if (!(DaggerfallUI.UIManager.TopWindow is DaggerfallExteriorAutomapWindow)){
                return;
            }
            foreach (var buildingNameplate in ExteriorAutomap.instance.buildingNameplates){
                if (IsBuildingSupported(((BuildingSummary)buildingNameplate.textLabel.Tag).BuildingType)){
                    // * If first building has the same Label as the stored, no need to update any. 
                    if (buildingNameplate.textLabel.ToolTipText.EndsWith("AM") || buildingNameplate.textLabel.ToolTipText.EndsWith("PM")){
                        return;
                        // continue;
                    }
                    SetToolTip(buildingNameplate, (BuildingSummary)buildingNameplate.textLabel.Tag);
                }
            }
        }

        void SetToolTip(BuildingNameplate buildingNameplate, BuildingSummary buildingSummary){
            if (buildingsList.TryGetValue(buildingSummary, out _)){
                // * Building is stored.
                // * Check if need to update the open/close text:
                if (previousTime == DaggerfallUnity.Instance.WorldTime.Now){
                    buildingsList[buildingSummary][1] = GetBuildingOpenClosedText(buildingSummary);
                    Debug.Log($"MST: Update previous time");
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
            return Environment.NewLine + buildingsList[buildingSummary][1] + " - " + buildingsList[buildingSummary][0];
        }
        
        string GetBuildingOpenClosedText(BuildingSummary buildingSummary){
            if (IsBuildingLocked(buildingSummary)){ return CLOSED_TEXT; }
            return OPEN_TEXT;
        }

        string GetBuildingOpenCloseTime(BuildingSummary buildingSummary){
            return $"{ConvertTime(PlayerActivate.openHours[(int)buildingSummary.BuildingType])} - {ConvertTime(PlayerActivate.closeHours[(int)buildingSummary.BuildingType])}";
        }

        bool IsBuildingLocked(BuildingSummary buildingSummary){
            // * See if open right now: (includes holidays + guild membership + quest)]
            return (!GameManager.Instance.PlayerActivate.BuildingIsUnlocked(buildingSummary) && 
                buildingSummary.BuildingType < DFLocation.BuildingTypes.Temple
                && buildingSummary.BuildingType != DFLocation.BuildingTypes.HouseForSale);
        }

        bool BuildingAlwaysOpen(BuildingSummary buildingSummary){
            return GameManager.Instance.GuildManager.GetGuild(buildingSummary.FactionId).HallAccessAnytime() || 
                PlayerActivate.openHours[(int)buildingSummary.BuildingType] == 0 && 
                (PlayerActivate.closeHours[(int)buildingSummary.BuildingType] == 25 || PlayerActivate.closeHours[(int)buildingSummary.BuildingType] == 0);
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
