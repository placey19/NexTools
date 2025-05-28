using UnityEditor;
using UnityEngine.UIElements;

namespace Nexcide.TileKing {

    class SceneListSettingsProvider : SettingsProvider {

        private const string SettingsPath = "Project/Nexcide/Tile King";

        [SettingsProvider]
        public static SettingsProvider Create() {
            return new SceneListSettingsProvider();
        }

        public SceneListSettingsProvider() : base(SettingsPath, SettingsScope.Project) {
            keywords = GetSearchKeywordsFromGUIContentProperties<SettingsContent>();
        }

        public override void OnActivate(string searchContext, VisualElement rootElement) {
            Undo.undoRedoPerformed += TileKingSettings.UndoRedoPerformed;
        }

        public override void OnDeactivate() {
            Undo.undoRedoPerformed -= TileKingSettings.UndoRedoPerformed;
        }

        public override void OnGUI(string searchContext) {
            TileKingSettings.OnGUI();
        }
    }
}
