using UnityEditor;
using UnityEngine.UIElements;

namespace Nexcide.TileKing {

    class SceneListSettingsProvider : SettingsProvider {

        private static SceneListSettingsProvider _instance;

        [SettingsProvider]
        public static SettingsProvider Create() {
            return new SceneListSettingsProvider();
        }

        public static void RepaintIfActive() {
            _instance?.Repaint();
        }

        public SceneListSettingsProvider() : base("Preferences/Nexcide/Tile King", SettingsScope.User) {
            keywords = GetSearchKeywordsFromGUIContentProperties<SettingsContent>();
        }

        public override void OnActivate(string searchContext, VisualElement rootElement) {
            _instance = this;
            Undo.undoRedoPerformed += TileKingSettings.UndoRedoPerformed;
        }

        public override void OnDeactivate() {
            _instance = null;
            Undo.undoRedoPerformed -= TileKingSettings.UndoRedoPerformed;
        }

        public override void OnGUI(string searchContext) {
            TileKingSettings.OnGUI();
        }
    }
}
