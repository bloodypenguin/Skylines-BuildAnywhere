using BuildAnywhere.Detours;
using ICities;

namespace BuildAnywhere
{
    public class LoadingExtension : LoadingExtensionBase
    {

        public override void OnLevelLoaded(LoadMode mode)
        {
            switch (mode)
            {
                case LoadMode.LoadMap:
                case LoadMode.NewMap:
                    return;
                case LoadMode.LoadGame:
                case LoadMode.NewGame:
                    if (Util.IsModActive("81 Tiles(Fixed for C:S 1.2 +)") || Util.IsModActive("81 Tile Unlock"))
                    {
                        UnityEngine.Debug.Log("81 Tiles is active.");
                        return;
                    }
                    break;
            }
            GameAreaManagerDetour.Deploy();
        }

        public override void OnLevelUnloading()
        {
            GameAreaManagerDetour.Revert();
        }
    }
}