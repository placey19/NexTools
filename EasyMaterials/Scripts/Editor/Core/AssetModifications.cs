using System;
using System.Collections.Generic;
using UnityEditor;

namespace Nexcide.EasyMaterials {

    /// <summary>
    /// To allow registering and unregistering for callbacks to the IsOpenForEdit() method.
    /// This weird AssetModificationProcessor API has methods that get called magically by Unity for some bizarre reason.
    /// The IsOpenForEdit() method is called to allow control over whether or not an asset editor should be disabled, but
    /// we're using it to know which assets are in the process of being edited. There might be a better API for this functionality
    /// but I found this and it does the job!
    /// </summary>
    public class AssetModifications : AssetModificationProcessor {

        public static Action<string[]> OpenForEdit;

        private static bool IsOpenForEdit(string[] paths, List<string> outNotEditablePaths, StatusQueryOptions statusQueryOptions) {
            OpenForEdit?.Invoke(paths);
            return true;
        }
    }
}
