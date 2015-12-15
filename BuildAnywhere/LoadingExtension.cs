using BuildAnywhere.Detours;
using ICities;

namespace BuildAnywhere
{
    public class LoadingExtension : LoadingExtensionBase
    {

        public override void OnLevelLoaded(LoadMode mode)
        {
            if (mode != LoadMode.LoadGame || mode != LoadMode.NewGame)
            {
                return;
            }
            if (Util.IsModActive("81 Tiles(Fixed for C:S 1.2 +)") || Util.IsModActive("81 Tile Unlock"))
            {
                UnityEngine.Debug.Log("81 Tiles is active.");
                return;
            }
            GameAreaManagerDetour.Deploy();
        }

        public override void OnLevelUnloading()
        {
            GameAreaManagerDetour.Revert();
        }
    }
}