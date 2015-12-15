using System;
using System.Reflection;
using ColossalFramework;

namespace BuildAnywhere
{
    public static class CrossTheLine
    {
        private static bool _fieldInitialized;
        private static bool _typeInitialized;
        private static FieldInfo _fineNetToolPrefabField;
        private static Type _fineNetToolType;

        public static FieldInfo FineNetToolPrefabField
        {
            get
            {
                if (_fieldInitialized)
                {
                    return _fineNetToolPrefabField;
                }
                if (FineNetToolType != null)
                {
                    _fineNetToolPrefabField = _fineNetToolType.GetField("m_prefab", BindingFlags.Instance | BindingFlags.Public);
                }
                _fieldInitialized = true;
                return _fineNetToolPrefabField;
            }
        }

        public static Type FineNetToolType
        {
            get
            {
                if (_typeInitialized)
                {
                    return _fineNetToolType;
                }
                _fineNetToolType = Util.FindType("NetToolFine");
                _typeInitialized = true;
                return _fineNetToolType;
            }
        }

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
                    }
                }
                else
                {
                    if (currentTool is TreeTool || currentTool is BulldozeTool ||
                        currentTool is PropTool || currentTool is WaterTool || currentTool is DistrictTool || currentTool is TerrainTool)
                    {
                        result = false;
                    }
                    else
                    {
                        if (ToolsModifierControl.toolController != null && FineNetToolType != null &&
                            FineNetToolPrefabField != null &&
                            ToolsModifierControl.toolController.CurrentTool.GetType() == FineNetToolType)
                        {
                            var prefab =
                                (NetInfo)FineNetToolPrefabField.GetValue(ToolsModifierControl.toolController.CurrentTool);
                            if (!CheckNetInfo(ref prefab))
                            {
                                result = false;
                            }
                        }
                    }
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