using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Nexcide.EasyMaterials {

    /// <summary>
    /// Collection of MaterialData objects that's stored in a List but also maintains a Dictionary to get MaterialData from the
    /// associated material asset's path. This also registers for a asset modification callback so when a Material asset is modified
    /// in the inspector, we can tell the MaterialData to update its asset preview.
    /// </summary>
    [Serializable]
    public class MaterialDataCollection {

        [SerializeField]
        private List<MaterialData> _materialDataList = new List<MaterialData>();

        [NonSerialized]
        private Dictionary<string, MaterialData> _materialPathDictionary = new Dictionary<string, MaterialData>();

        // count property that simply calls the list
        public int Count {
            get { return _materialDataList.Count; }
        }

        public void OnEnable() {
            AssetModifications.OpenForEdit = IsOpenForEdit;

            // if the list already has data (could do due to serialization), refresh the list and dictionary
            Refresh();
            RefreshDictionary();
        }
        
        public void OnDisable() {
            AssetModifications.OpenForEdit = null;

            _materialPathDictionary.Clear();
        }

        public List<MaterialData> List() {
            return _materialDataList;
        }

        public IEnumerable<MaterialData> Reverse() {
            for (int i = (_materialDataList.Count - 1); i >= 0; --i) {
                yield return _materialDataList[i];
            }
        }

        public void Refresh(bool force = false) {
            foreach (MaterialData materialData in _materialDataList) {
                materialData.Refresh(force);
            }
        }

        public void RefreshDictionary() {
            _materialPathDictionary.Clear();

            if (_materialDataList.Count > 0) {
                foreach (MaterialData materialData in _materialDataList) {
                    _materialPathDictionary.Add(materialData.GetPath(), materialData);
                }
            }
        }

        public bool Contains(MaterialData materialData) {
            return _materialDataList.Contains(materialData);
        }

        public MaterialData GetLast() {
            int count = _materialDataList.Count;
            return (count > 0 ? _materialDataList[count - 1] : null);
        }

        public void MoveToLast(MaterialData materialData) {
            _materialDataList.Remove(materialData);
            _materialDataList.Add(materialData);
        }

        public void InsertAt(int index, Material material, Color color) {
            MaterialData materialData = null;

            string path = AssetDatabase.GetAssetPath(material);
            if (path != null && path.Length > 0) {
                materialData = GetMaterialData(path);
            }

            if (materialData == null) {
                materialData = new MaterialData(material, color);
                materialData.Refresh();
            }

            if (_materialDataList.Contains(materialData)) {
                _materialDataList.Remove(materialData);
            }

            Insert(index, materialData);
        }

        public void Insert(int index, MaterialData materialData) {
            _materialDataList.Insert(index, materialData);

            try {
                string path = materialData.GetPath();
                _materialPathDictionary.Add(path, materialData);
            } catch (ArgumentException) {
                // ignore if already added
            }
        }

        public void Add(MaterialData materialData) {
            _materialDataList.Add(materialData);

            try {
                string path = materialData.GetPath();
                _materialPathDictionary.Add(path, materialData);
            } catch (ArgumentException) {
                // ignore if already added
            }
        }

        public bool Remove(MaterialData materialData) {
            string path = materialData.GetPath();
            _materialPathDictionary.Remove(path);

            return _materialDataList.Remove(materialData);
        }

        public void RemoveFirst() {
            _materialDataList.RemoveAt(0);
        }

        public void Clear() {
            _materialDataList.Clear();
            _materialPathDictionary.Clear();
        }

        private void IsOpenForEdit(string[] paths) {
            foreach (string path in paths) {
                MaterialData materialData = GetMaterialData(path);

                if (materialData != null) {
                    materialData.RefreshAssetPreviewIfChanged();
                }
            }
        }

        private MaterialData GetMaterialData(string path) {
            try {
                return _materialPathDictionary[path];
            } catch (KeyNotFoundException) {
                return null;
            }
        }
    }
}
