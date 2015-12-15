using System;
using System.Reflection;
using System.Threading;
using BuildAnywhere.Redirection;
using ColossalFramework;
using ColossalFramework.Math;

namespace BuildAnywhere.Detours
{
    [TargetType(typeof(GameAreaManager))]
    public class GameAreaManagerDetour
    {
        private static object _lock = new object();
        private static RedirectCallsState _state;
        private static IntPtr _originalPtr = IntPtr.Zero;
        private static IntPtr _detourPtr = IntPtr.Zero;

        private static MethodInfo _detourInfo = typeof(GameAreaManagerDetour).GetMethod("QuadOutOfArea");
        private static bool _deployed;

        public static void Deploy()
        {
            if (_deployed)
            {
                return;
            }
            var tuple = RedirectionUtil.RedirectMethod(typeof(GameAreaManager), _detourInfo);
            _originalPtr = tuple.First.MethodHandle.GetFunctionPointer();
            _detourPtr = _detourInfo.MethodHandle.GetFunctionPointer();
            _state = tuple.Second;
            _deployed = true;
        }

        public static void Revert()
        {
            if (!_deployed) return;
            if (_originalPtr != IntPtr.Zero && _detourPtr != IntPtr.Zero)
            {
                RedirectionHelper.RevertJumpTo(_originalPtr, _state);
            }
            _deployed = false;
        }

        [RedirectMethod]
        public bool QuadOutOfArea(Quad2 quad)
        {
            var result = CrossTheLine.IsCrossingLineProhibited();
            if (result)
            {
                do
                {
                }
                while (!Monitor.TryEnter(_lock, SimulationManager.SYNCHRONIZE_TIMEOUT));
                try
                {
                    RedirectionHelper.RevertJumpTo(_originalPtr, _state);
                    result = Singleton<GameAreaManager>.instance.QuadOutOfArea(quad);
                    RedirectionHelper.PatchJumpTo(_originalPtr, _detourPtr);
                }
                finally
                {
                    Monitor.Exit(_lock);
                }
            }
            return result;
        }
    }
}