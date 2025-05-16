using System;
using UnityEditor;
using UnityEngine;

using static EasyMaterials.EasyMaterialUtil;

namespace EasyMaterials {

    [Serializable]
    public class MaterialData {

        private const double AssetPreviewMaxRefreshDuration = 10.0;
        private const string MissingPrefix = "[Missing] ";

        [SerializeField] private Material _material;
        [SerializeField] private string _name;
        [SerializeField] private string _path;
        [SerializeField] private Color _color;
        [SerializeField] private bool _missing;

        private Texture _assetPreview;
        private Texture2D _assetPreviewRef;
        private bool _refreshingAssetPreview;
        private double _assetPreviewRefreshExpireTime;

        public MaterialData(Material material, Color color) {
            _material = material;
            _name = _material.name;
            _path = AssetDatabase.GetAssetPath(_material);
            _color = color;
        }

        public override bool Equals(object obj) {
            // compare the stored path instead of the material since the material can be null if deleted by the user
            MaterialData materialData = obj as MaterialData;
            return (materialData != null && materialData._path == _path);
        }

        public override int GetHashCode() {
            return _path.GetHashCode();
        }

        public string GetName() {
            // material will be null if the asset gets deleted by the user
            if (_material == null) {
                if (!_missing) {
                    _missing = true;
                } else {
                    // check if the missing material has come back
                    _material = AssetDatabase.LoadAssetAtPath<Material>(_path);
                    _missing = (_material == null);
                }
            }

            return (_missing ? (MissingPrefix + _path) : _name);
        }

        public bool IsMissing() {
            return _missing;
        }

        public string GetPath() {
            return _path;
        }

        public Texture GetAssetPreview() {
            return _assetPreview;
        }

        public Color GetColor() {
            return _color;
        }

        public void Select() {
            if (_material != null) {
                Selection.activeObject = _material;
            }
        }

        public void AddToBundle(MaterialBundle materialBundle) {
            if (_material != null) {
                materialBundle.Add(_material);
            } else {
                Log.w("Skipped saving missing material");
            }
        }

        public void Refresh(bool force = false) {
            if (_material != null) {
                _name = _material.name;
                _path = AssetDatabase.GetAssetPath(_material);

                RefreshAssetPreview(force);
            }
        }

        public void RefreshAssetPreviewIfChanged() {
            Texture2D assetPreview = AssetPreview.GetAssetPreview(_material);

            if (assetPreview != null && assetPreview != _assetPreviewRef) {
                // copy reference to the asset preview, only for detecting changes
                _assetPreviewRef = assetPreview;

                // deep copy the asset preview texture
                _assetPreview = new Texture2D(assetPreview.width, assetPreview.height, assetPreview.format, false);
                Graphics.CopyTexture(assetPreview, _assetPreview);

                EasyMaterialTool.RepaintIfActive();
            }
        }

        private void RefreshAssetPreview(bool force = false) {
            if (_material != null) {
                if (_assetPreview == null || _refreshingAssetPreview || force) {
                    Texture2D assetPreview = AssetPreview.GetAssetPreview(_material);

                    if (assetPreview != null) {
                        _assetPreview = new Texture2D(assetPreview.width, assetPreview.height, assetPreview.format, false);
                        Graphics.CopyTexture(assetPreview, _assetPreview);

                        _refreshingAssetPreview = false;
                        EasyMaterialTool.RepaintIfActive();
                    } else {
                        if (_assetPreviewRefreshExpireTime == 0.0) {
                            _assetPreviewRefreshExpireTime = (EditorApplication.timeSinceStartup + AssetPreviewMaxRefreshDuration);
                        }

                        if (EditorApplication.timeSinceStartup < _assetPreviewRefreshExpireTime) {
                            _refreshingAssetPreview = true;
                            EditorApplication.delayCall += () => RefreshAssetPreview();
                        } else {
                            _refreshingAssetPreview = false;
                            _assetPreviewRefreshExpireTime = 0.0;
                        }
                    }
                }

                // if asset preview still isn't available, use temporary icon
                if (_assetPreview == null) {
                    _assetPreview = AssetDatabase.GetCachedIcon(_path);
                }
            }
        }

        public void StartDrag() {
            if (_material != null) {
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.objectReferences = ToArray<UnityEngine.Object>(_material);
                DragAndDrop.StartDrag("Assign Material");
            }
        }
    }
}
