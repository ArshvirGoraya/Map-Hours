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

namespace MapShopTimesMod
{
    public class MapShopTimes : MonoBehaviour
    {
        public BuildingNameplate[] buildingNameplatesRef;
        Dictionary<BuildingSummary, string> buildingsList = new Dictionary<BuildingSummary, string>();
        BuildingSummary buildingSummary;

        string shopTime;
        string openTime;
        string closeTime;

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
                buildingSummary = (BuildingSummary)buildingNameplate.textLabel.Tag;
                // if (BuildingIsStore(buildingSummary.BuildingType)){
                //     SetToolTip(buildingNameplate, buildingSummary);
                // }
                SetToolTip(buildingNameplate, buildingSummary);
            }
        }

        void SetToolTip(BuildingNameplate buildingNameplate, BuildingSummary buildingSummary){
            Debug.Log($"MST - Guild: {GameManager.Instance.GuildManager.GetGuild(buildingSummary.FactionId)} Access: {GameManager.Instance.GuildManager.GetGuild(buildingSummary.FactionId).HallAccessAnytime()}");
            
            if (buildingsList.TryGetValue(buildingSummary, out string storedToolTip)){
                buildingNameplate.textLabel.ToolTipText = storedToolTip;
            }
            else{
                shopTime = "";
                if (!BuildingAlwaysOpen(buildingSummary)){
                    openTime = ConvertTime(PlayerActivate.openHours[(int)buildingSummary.BuildingType], buildingSummary.BuildingType);
                    closeTime = ConvertTime(PlayerActivate.closeHours[(int)buildingSummary.BuildingType], buildingSummary.BuildingType);
                    shopTime = $"{openTime} - {closeTime} - ";
                }
                buildingNameplate.textLabel.ToolTipText += Environment.NewLine + shopTime.ToString();
                buildingsList.Add(buildingSummary, buildingNameplate.textLabel.ToolTipText);
            }

            // * See if open right now: (includes holidays + guild membership)]
            if (!GameManager.Instance.PlayerActivate.BuildingIsUnlocked(buildingSummary) && 
                buildingSummary.BuildingType < DFLocation.BuildingTypes.Temple
                && buildingSummary.BuildingType != DFLocation.BuildingTypes.HouseForSale)
            {
                buildingNameplate.textLabel.ToolTipText += "CLOSED";
                Debug.Log($"MST - {buildingSummary.BuildingType} is CLOSED");
            }else{
                Debug.Log($"MST - {buildingSummary.BuildingType} is OPEN");
                buildingNameplate.textLabel.ToolTipText += "OPEN";
            }

            // if (buildingSummary.BuildingType == DFLocation.BuildingTypes.Palace){
            //     Debug.Log($"MST - palace open?: {GameManager.Instance.GuildManager.GetGuild(buildingSummary.FactionId).HallAccessAnytime()}");
            // }

            // if (GameManager.Instance.PlayerActivate.BuildingIsUnlocked(buildingSummary)){
            //     buildingNameplate.textLabel.ToolTipText += "OPEN";
            // }else{
            //     buildingNameplate.textLabel.ToolTipText += "CLOSED";
            // }
        }

        bool BuildingAlwaysOpen(BuildingSummary buildingSummary){
            return GameManager.Instance.GuildManager.GetGuild(buildingSummary.FactionId).HallAccessAnytime() || 
                PlayerActivate.openHours[(int)buildingSummary.BuildingType] == 0 && 
                (PlayerActivate.closeHours[(int)buildingSummary.BuildingType] == 25 || PlayerActivate.closeHours[(int)buildingSummary.BuildingType] == 0);
        }

        string ConvertTime(int hour, BuildingTypes buildingType){
            if (hour >= 24){
                Debug.Log($"MST - over building type: {buildingType}");
                return new DateTime(1, 1, 1, 0, 0, 0).ToString("hh:mm tt");
            }
            return new DateTime(1, 1, 1, hour, 0, 0).ToString("hh:mm tt");
        }

        bool BuildingIsStore(BuildingTypes buildingType){
            if (
                buildingType == BuildingTypes.GeneralStore ||
                buildingType == BuildingTypes.PawnShop ||
                buildingType == BuildingTypes.WeaponSmith ||
                buildingType == BuildingTypes.Alchemist ||
                buildingType == BuildingTypes.GemStore ||
                buildingType == BuildingTypes.ClothingStore ||
                buildingType == BuildingTypes.Bank ||
                buildingType == BuildingTypes.Bookseller ||
                buildingType == BuildingTypes.Library
                ){
                return true;
            }
            return false;
        }
    }
}
