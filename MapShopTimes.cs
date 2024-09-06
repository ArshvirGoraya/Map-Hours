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

namespace MapShopTimesMod
{
    public class MapShopTimes : MonoBehaviour
    {
        // public string[] buildingNameplatesRef;
        public BuildingNameplate[] buildingNameplatesRef;
        Dictionary<BuildingSummary, string> buildingsList = new Dictionary<BuildingSummary, string>();

        string storeTime;
        private static Mod mod;

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            mod = initParams.Mod;

            var go = new GameObject(mod.Title);
            go.AddComponent<MapShopTimes>();

            mod.IsReady = true;
        }

        private void Start(){
            // DaggerfallUI.Instance.UserInterfaceManager.OnWindowChange += UIWindowChange;
        }

        private void LateUpdate(){
            if (DaggerfallUI.UIManager.TopWindow is DaggerfallExteriorAutomapWindow){
            //     if (!ArrayUtility.ArrayEquals(buildingNameplatesRef, ExteriorAutomap.instance.buildingNameplates)){
            //         buildingNameplatesRef = (BuildingNameplate[])ExteriorAutomap.instance.buildingNameplates.Clone();
            //         Debug.Log($"MST - nameplates changed!");
            //     }

                foreach (var buildingNameplate in ExteriorAutomap.instance.buildingNameplates){
                    BuildingSummary buildingSummary = (BuildingSummary)buildingNameplate.textLabel.Tag;
                    if (BuildingIsStore(buildingSummary.BuildingType)){

                        if (buildingsList.TryGetValue(buildingSummary, out string storedToolTip)){
                            buildingNameplate.textLabel.ToolTipText = storedToolTip;
                        }else{
                            storeTime = $"{ConvertTime(PlayerActivate.openHours[(int)buildingSummary.BuildingType])} - {ConvertTime(PlayerActivate.closeHours[(int)buildingSummary.BuildingType])}";
                            buildingNameplate.textLabel.ToolTipText += Environment.NewLine + $"\n{storeTime}";
                            // buildingNameplate.textLabel.ToolTipText += $"\n{storeTime}";
                            buildingsList.Add(buildingSummary, buildingNameplate.textLabel.ToolTipText);
                        }

                        // if (buildingNameplate.textLabel.ToolTipText != storeTime){
                            // buildingNameplate.textLabel.ToolTipText += Environment.NewLine + $"\n{storeTime}";
                        // }
                        // Debug.Log($"MST - {buildingNameplate.textLabel.ToolTipText}");
                        // Debug.Log($"MST - ToolTipText: {buildingNameplate.textLabel.ToolTipText}");
                        // if (PlayerActivate.IsBuildingOpen(buildingSummary.BuildingType)){
                        // }
                    }
                }
            }
        }

        // void CreateBuildingNamePlates(){
        //     bool isEqual = true;
        //     for (int i = 0; i < ExteriorAutomap.instance.buildingNameplates.Length; i++){
        //         if (isEqual){
        //             if (buildingNameplatesRef.Length < i || ExteriorAutomap.instance.buildingNameplates[i].name != buildingNameplatesRef[i]){
        //                 isEqual = false;
        //             }
        //         }
        //         if (!isEqual){
        //             if (buildingNameplatesRef.Length < i){
        //                 buildingNameplatesRef.Append(ExteriorAutomap.instance.buildingNameplates[i].name);    
        //             }else{
        //                 buildingNameplatesRef[i] = ExteriorAutomap.instance.buildingNameplates[i].name;
        //             }
        //         }
        //     }
        //     Array.Resize(ref buildingNameplatesRef, ExteriorAutomap.instance.buildingNameplates.Length);
        // }

        // void UIWindowChange(object sender, System.EventArgs e){
        //     Debug.Log($"MST - top window: {DaggerfallUI.UIManager.TopWindow}");
        //     if (DaggerfallUI.UIManager.TopWindow is DaggerfallExteriorAutomapWindow){
        //         ExteriorAutomap map = ExteriorAutomap.instance;
        //         foreach (var buildingNameplate in map.buildingNameplates){
        //             BuildingSummary buildingSummary = (BuildingSummary)buildingNameplate.textLabel.Tag;
        //             if (BuildingIsStore(buildingSummary.BuildingType)){
        //                 var openHour = PlayerActivate.openHours[(int)buildingSummary.BuildingType];
        //                 var closeHour = PlayerActivate.closeHours[(int)buildingSummary.BuildingType];
        //                 string storeTime = $"{ConvertTime(openHour)} - {ConvertTime(closeHour)}";
        //                 buildingNameplate.textLabel.ToolTipText += $": {storeTime}";
        //                 Debug.Log($"MST - {buildingNameplate.textLabel.ToolTipText}");
        //                 // Debug.Log($"MST - ToolTipText: {buildingNameplate.textLabel.ToolTipText}");
        //                 // if (PlayerActivate.IsBuildingOpen(buildingSummary.BuildingType)){
        //                 // }
        //             }
        //         }
        //     }
        // }

        string ConvertTime(int hour){
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
