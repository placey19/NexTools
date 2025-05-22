using System;
using UnityEngine;

namespace Nexcide.TileKing {

    public enum TileRotation {

        None,
        Degrees_90,
        Degrees_180,
        Degrees_270,
    }

    [Serializable]
    public class Tile {

        public Color Color;
        public bool FlipU;
        public bool FlipV;
        public TileRotation Rotation;
        public int MaterialId;
    }

    [CreateAssetMenu(menuName = "Nexcide/Tile Canvas")]
    public class TileCanvas : ScriptableObject {

        public Color FromColor = Color.black;
        public Color ToColor = Color.white;
        public float SaturationMin;
        public float SaturationMax;
        public float ValueMin = 0.25f;
        public float ValueMax = 0.75f;

        [SerializeField]
        private int _randMaterialsSeed;

        [SerializeField]
        private int _randColorsSeed;

        [SerializeField]
        private int _randRotationSeed;

        [SerializeField]
        private int _randUVsSeed;

        [SerializeField]
        private int _width = 8;

        [SerializeField]
        private int _height = 8;

        [SerializeField]
        private Vector2 _tileSize = new(0.75f, 0.75f);

        [SerializeField]
        private Vector2 _materialScale = new(1.0f, 1.0f);

        [SerializeField]
        private bool _hasRandomizedColors;

        [SerializeField]
        private bool _hasRandomizedMaterials;

        [SerializeField]
        private int _materialsCount;

        [NonSerialized]
        private Tile[,] _tiles;

        public int Width() => _width;

        public int Height() => _height;

        public Vector2 TileSize() => _tileSize;

        public Vector2 MaterialScale() => _materialScale;

        public Tile[,] GetTiles() {
            if (_tiles == null) {
                CreateTiles();
            }

            return _tiles;
        }

        public void Resize(int width, int height) {
            _width = width;
            _height = height;
            CreateTiles();
        }

        public void SetSizeAndScale(Vector2 size, Vector2 scale) {
            _tileSize = size;
            _materialScale = scale;
        }

        public void RandomizeMaterials(int seed, int count) {
            _randMaterialsSeed = seed;
            _materialsCount = count;
            RandomizeMaterials();
        }

        public void RandomizeMaterials() {
            UnityEngine.Random.InitState(_randMaterialsSeed);

            Tile[,] tiles = GetTiles();
            foreach (Tile tile in tiles) {
                tile.MaterialId = UnityEngine.Random.Range(0, _materialsCount);
            }

            _hasRandomizedMaterials = true;
        }

        public void ResetMaterials() {
            Tile[,] tiles = GetTiles();
            foreach (Tile tile in tiles) {
                tile.MaterialId = 0;
            }

            _hasRandomizedMaterials = false;
        }

        public void RandomizeColors(int seed) {
            _randColorsSeed = seed;
            RandomizeColors();
        }

        public void RefreshColors() {
            if (_hasRandomizedColors) {
                RandomizeColors();
            }
        }

        private void RandomizeColors() {
            UnityEngine.Random.InitState(_randColorsSeed);

            Tile[,] tiles = GetTiles();
            foreach (Tile tile in tiles) {
                Color.RGBToHSV(FromColor, out float hueMin, out float saturation, out float value);
                Color.RGBToHSV(ToColor, out float hueMax, out saturation, out value);
                tile.Color = UnityEngine.Random.ColorHSV(hueMin, hueMax, SaturationMin, SaturationMax, ValueMin, ValueMax);
            }

            _hasRandomizedColors = true;
        }

        public void ResetColors(Color color) {
            Tile[,] tiles = GetTiles();
            foreach (Tile tile in tiles) {
                tile.Color = color;
            }

            _hasRandomizedColors = false;
        }

        public void RandomizeUVs(int seed) {
            _randUVsSeed = seed;
            UnityEngine.Random.InitState(_randUVsSeed);

            Tile[,] tiles = GetTiles();
            foreach (Tile tile in tiles) {
                tile.FlipU = (UnityEngine.Random.value >= 0.5f);
                tile.FlipV = (UnityEngine.Random.value >= 0.5f);
            }
        }

        public void ResetUVs() {
            Tile[,] tiles = GetTiles();
            foreach (Tile tile in tiles) {
                tile.FlipU = false;
                tile.FlipV = false;
            }
        }

        public void RandomizeRotations(int seed) {
            _randRotationSeed = seed;
            UnityEngine.Random.InitState(_randRotationSeed);

            int enumLength = Enum.GetValues(typeof(TileRotation)).Length;

            Tile[,] tiles = GetTiles();
            foreach (Tile tile in tiles) {
                tile.Rotation = (TileRotation)UnityEngine.Random.Range(0, enumLength);
            }
        }

        public void ResetRotations() {
            Tile[,] tiles = GetTiles();
            foreach (Tile tile in tiles) {
                tile.Rotation = TileRotation.None;
            }
        }

        private void CreateTiles() {
            _tiles = new Tile[_width, _height];

            for (int y = 0; y < _height; ++y) {
                for (int x = 0; x < _width; ++x) {
                    _tiles[x, y] = new Tile();
                    _tiles[x, y].Color = Color.white;
                }
            }

            if (_hasRandomizedColors) {
                RandomizeColors();
            }

            if (_hasRandomizedMaterials) {
                RandomizeMaterials();
            }
        }
    }
}
