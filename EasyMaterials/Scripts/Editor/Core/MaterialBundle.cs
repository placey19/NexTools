using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nexcide.EasyMaterials {

    /// <summary>
    /// Data that's stored to asset file for a collection of materials.
    /// </summary>
    public class MaterialBundle : ScriptableObject, IEnumerable<Material> {

        [SerializeField]
        private List<Material> _bundle = new List<Material>();

        public void Add(Material material) {
            _bundle.Add(material);
        }

        public int Count() {
            return _bundle.Count;
        }

        public IEnumerator<Material> GetEnumerator() {
            return _bundle.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return _bundle.GetEnumerator();
        }
    }
}
