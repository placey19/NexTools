using UnityEditor;
using UnityEngine.UIElements;

namespace Nexcide.SceneList {

    class SceneListSettingsProvider : SettingsProvider {

        private const string SettingsPath = "Project/Nexcide/Scene List";

        [SettingsProvider]
        public static SettingsProvider Create() {
            return new SceneListSettingsProvider();
        }

        public SceneListSettingsProvider() : base(SettingsPath, SettingsScope.Project) {
            keywords = GetSearchKeywordsFromGUIContentProperties<SettingsContent>();
        }

        public override void OnActivate(string searchContext, VisualElement rootElement) {
            Undo.undoRedoPerformed += SceneListSettings.UndoRedoPerformed;
        }

        public override void OnDeactivate() {
            Undo.undoRedoPerformed -= SceneListSettings.UndoRedoPerformed;
        }

        public override void OnGUI(string searchContext) {
            SceneListSettings.OnGUI();
        }
    }
}
