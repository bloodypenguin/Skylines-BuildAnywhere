using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using ColossalFramework;
using ColossalFramework.Math;
using ICities;

namespace BuildAnywhere
{
    public class Mod : LoadingExtensionBase, IUserMod
    {

        private static RedirectCallsState _state;
        private static IntPtr _originalPtr;
        private static IntPtr _detourPtr;
        private static object _lock = new object();
        private static Type _fineNetToolType;
        private static FieldInfo _fineNetToolPrefabField;
        private static LoadMode loadMode;


        public string Name
        {
            get
            {
                _originalPtr = typeof(GameAreaManager).GetMethod("QuadOutOfArea",
                            BindingFlags.Instance | BindingFlags.Public).MethodHandle.GetFunctionPointer();
                _detourPtr = typeof(Mod).GetMethod("QuadOutOfArea",
                            BindingFlags.Instance | BindingFlags.Public).MethodHandle.GetFunctionPointer();

                return "CrossTheLine";
            }

        }

        public string Description
        {
            get { return "Build your infrastructure outside of city limits"; }
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            loadMode = mode;
            if (loadMode == LoadMode.LoadGame || loadMode == LoadMode.NewGame)
            {
                _state = RedirectionHelper.PatchJumpTo(_originalPtr, _detourPtr);
                _fineNetToolType = FindType("NetToolFine");
                if (_fineNetToolType != null)
                {
                    _fineNetToolPrefabField = _fineNetToolType.GetField("m_prefab", BindingFlags.Instance | BindingFlags.Public);
                }
            }
        }

        public override void OnLevelUnloading()
        {
            if (loadMode == LoadMode.LoadGame || loadMode == LoadMode.NewGame)
            {
                RedirectionHelper.RevertJumpTo(_originalPtr, _state);
                _fineNetToolType = null;
                _fineNetToolPrefabField = null;
            }
        }

        private static Type FindType(string className)
        {
            return (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                    from type in assembly.GetTypes()
                    where type.Name == className
                    select type).FirstOrDefault();
        }

        public bool QuadOutOfArea(Quad2 quad)
        {
            do
            {
            }
            while (!Monitor.TryEnter(_lock, SimulationManager.SYNCHRONIZE_TIMEOUT));

            var result = true;

            var currentTool = ToolsModifierControl.GetCurrentTool<ToolBase>();
            if (currentTool != null)
            {

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
                            currentTool is PropTool || currentTool is WaterTool)
                        {
                            result = false;
                        }
                        else
                        {
                            if (ToolsModifierControl.toolController != null && _fineNetToolType != null && _fineNetToolPrefabField != null &&
                                ToolsModifierControl.toolController.CurrentTool.GetType() == _fineNetToolType)
                            {
                                var prefab = (NetInfo)_fineNetToolPrefabField.GetValue(ToolsModifierControl.toolController.CurrentTool);
                                if (!CheckNetInfo(ref prefab))
                                {
                                    result = false;
                                }
                            }
                        }
                    }
                }
            }
            try
            {
                if (result)
                {
                    RedirectionHelper.RevertJumpTo(_originalPtr, _state);
                    result = Singleton<GameAreaManager>.instance.QuadOutOfArea(quad);
                    RedirectionHelper.PatchJumpTo(_originalPtr, _detourPtr);
                }

            }
            finally
            {
                Monitor.Exit(_lock);
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
            if (!((RoadAI)prefab.m_netAI).m_enableZoning)
            {
                return false;
            }
            return true;
        }

    }
}
