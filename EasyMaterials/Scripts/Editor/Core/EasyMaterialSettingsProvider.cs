using UnityEditor;
using UnityEngine.UIElements;

namespace Nexcide.EasyMaterials {

    class EasyMaterialSettingsProvider : SettingsProvider {

        private static EasyMaterialSettingsProvider _instance;

        [SettingsProvider]
        public static SettingsProvider Create() {
            return new EasyMaterialSettingsProvider();
        }

        public static void RepaintIfActive() {
            if (_instance != null) {
                _instance.Repaint();
            }
        }

        public EasyMaterialSettingsProvider() : base("Preferences/Nexcide/Easy Materials", SettingsScope.User) {
            keywords = GetSearchKeywordsFromGUIContentProperties<SettingsContent>();
        }

        public override void OnActivate(string searchContext, VisualElement rootElement) {
            _instance = this;
            Undo.undoRedoPerformed += EasyMaterialSettings.UndoRedoPerformed;
        }

        public override void OnDeactivate() {
            _instance = null;
            Undo.undoRedoPerformed -= EasyMaterialSettings.UndoRedoPerformed;
        }

        public override void OnGUI(string searchContext) {
            EasyMaterialSettings.OnGUI();
        }
    }
}
