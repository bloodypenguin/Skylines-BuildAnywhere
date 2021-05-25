using System;
using System.Reflection;
using ColossalFramework;

namespace BuildAnywhere
{
    //don't rename anything here! 81 Tiles searches for this code!
    public static class CrossTheLine
    {

        public static bool IsCrossingLineProhibited()
        {
            var currentTool = ToolsModifierControl.GetCurrentTool<ToolBase>();
            if (currentTool == null)
            {
                return true;
            }
            var result = true;
            var netTool = currentTool as NetTool;
            if (netTool != null)
            {
                if (!CheckNetInfo(ref netTool.m_prefab))
                {
                    result = false;
                }
            }
            else
            {
                var buildingTool = currentTool as BuildingTool;
                if (buildingTool != null)
                {
                    BuildingInfo building;
                    if (buildingTool.m_relocate == 0)
                    {
                        building = buildingTool.m_prefab;
                    }
                    else
                    {
                        building = Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingTool.m_relocate].Info;
                    }
                    if (building != null && building.m_placementMode != BuildingInfo.PlacementMode.Roadside)
                    {
                        result = CheckBuildingForZoneableRoads(building);
                    }
                }
                else
                {
                    if (currentTool is TreeTool || currentTool is DefaultTool || currentTool is DisasterTool ||
                        currentTool is PropTool || currentTool is WaterTool || currentTool is DistrictTool || currentTool is TerrainTool)
                    {
                        result = false;
                    }
                }
            }
            return result;
        }

        private static bool CheckBuildingForZoneableRoads(BuildingInfo building)
        {
            var result = true;
            var paths = building.m_paths;
            if (paths == null || paths.Length <= 0)
            {
                result = false;
            }
            else
            {
                var allClear = true;
                foreach (var path in paths)
                {
                    if (CheckNetInfo(ref path.m_netInfo))
                    {
                        allClear = false;
                        break;
                    }
                }
                if (allClear)
                {
                    result = false;
                }
            }
            return result;
        }

        private static bool CheckNetInfo(ref NetInfo prefab)
        {
            if (prefab == null)
            {
                return true;
            }
            if (prefab.m_netAI is PedestrianPathAI)
            {
                return true;
            }
            if (!(prefab.m_netAI is RoadAI))
            {
                return false;
            }
            return ((RoadAI)prefab.m_netAI).m_enableZoning;
        }
    }
}