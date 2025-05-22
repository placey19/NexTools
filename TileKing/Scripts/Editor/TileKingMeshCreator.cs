using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

using static Nexcide.TileKing.MathsUtil;

namespace Nexcide.TileKing {

    public class TileKingMeshCreator {

        private int _materialsCount;
        private readonly List<Vector3> _vertices = new();
        private readonly List<List<int>> _subMeshTriangles = new();
        private readonly List<Color> _colors = new();
        private readonly List<Vector2> _uv0 = new();
        private readonly List<Vector3> _normals = new();
        private readonly List<Vector4> _tangents = new();

        public void PopulateMesh(TileKingMesh tileKingMesh) {
            TileCanvas canvas = tileKingMesh.Canvas;
            _materialsCount = Mathf.Max(tileKingMesh.Materials.Count, 1);
            Mesh mesh = tileKingMesh.MeshAsset;
            mesh.Clear(keepVertexLayout: false);
            Clear();

            int canvasWidth = canvas.Width();
            int canvasHeight = canvas.Height();
            Vector2 materialScale = canvas.MaterialScale();
            Vector2 tileSize = canvas.TileSize();
            Vector2 tileScale = new(1.0f / materialScale.x, 1.0f / materialScale.y);
            Tile[,] tiles = canvas.GetTiles();

            for (int y = 0; y < canvasHeight; ++y) {
                for (int x = 0; x < canvasWidth; ++x) {
                    float pX = (x * tileSize.x);
                    float pY = (y * tileSize.y);

                    Vector3 l1 = new(pX,              pY,              0.0f);
                    Vector3 l2 = new(pX,              pY + tileSize.y, 0.0f);
                    Vector3 r2 = new(pX + tileSize.x, pY + tileSize.y, 0.0f);
                    Vector3 r1 = new(pX + tileSize.x, pY,              0.0f);

                    Tile tile = tiles[x, y];
                    AddSquareFace(l1, l2, r2, r1, tile, tileScale);
                }
            }

            mesh.indexFormat = (_vertices.Count >= 65536 ? IndexFormat.UInt32 : IndexFormat.UInt16);
            mesh.SetVertices(_vertices);
            mesh.SetColors(_colors);
            mesh.SetUVs(0, _uv0);
            mesh.SetNormals(_normals);
            mesh.SetTangents(_tangents);

            mesh.subMeshCount = _subMeshTriangles.Count;
            for (int i = 0; i < _subMeshTriangles.Count; ++i) {
                mesh.SetTriangles(_subMeshTriangles[i], i);
            }
        }

        private void Clear() {
            _vertices.Clear();

            _subMeshTriangles.Clear();
            for (int i = 0; i < _materialsCount; ++i) {
                List<int> triangles = new List<int>();
                _subMeshTriangles.Add(triangles);
            }

            _colors.Clear();
            _uv0.Clear();
            _normals.Clear();
            _tangents.Clear();
        }

        private void AddSquareFace(Vector3 l1, Vector3 l2, Vector3 r2, Vector3 r1, Tile tile, Vector2 scale) {
            Vector3 normal = CalculateNormal(l1, l2, r1);
            Vector4 tangent = (r1 - l1).normalized;
            tangent.w = -1.0f;

            Vector2 uvL1 = new(0.0f,           0.0f);
            Vector2 uvL2 = new(0.0f,           1.0f * scale.y);
            Vector2 uvR2 = new(1.0f * scale.x, 1.0f * scale.y);
            Vector2 uvR1 = new(1.0f * scale.x, 0.0f);

            if (tile.FlipU) {
                (uvL1, uvR1) = (uvR1, uvL1);
                (uvL2, uvR2) = (uvR2, uvL2);
            }

            if (tile.FlipV) {
                (uvL1, uvL2) = (uvL2, uvL1);
                (uvR1, uvR2) = (uvR2, uvR1);
            }

            if (tile.Rotation != TileRotation.None) {
                List<Vector2> uvs = new () {
                    uvL1, uvL2, uvR2, uvR1
                };
                uvs.Shift((int)tile.Rotation);

                uvL1 = uvs[0];
                uvL2 = uvs[1];
                uvR2 = uvs[2];
                uvR1 = uvs[3];
            }

            int iL1 = AddVertex(l1, tile.Color, uvL1, normal, tangent);
            int iL2 = AddVertex(l2, tile.Color, uvL2, normal, tangent);
            int iR2 = AddVertex(r2, tile.Color, uvR2, normal, tangent);
            int iR1 = AddVertex(r1, tile.Color, uvR1, normal, tangent);

            AddTriangle(tile.MaterialId, iL1, iL2, iR1);
            AddTriangle(tile.MaterialId, iR1, iL2, iR2);
        }

        private int AddVertex(Vector3 vertex, Color color, Vector2 uv0, Vector3 normal, Vector4 tangent) {
            _vertices.Add(vertex);
            _colors.Add(color);
            _uv0.Add(uv0);
            _normals.Add(normal);
            _tangents.Add(tangent);

            return (_vertices.Count - 1);
        }

        private void AddTriangle(int subMesh, int i0, int i1, int i2) {
            if (subMesh >= _materialsCount) {
                subMesh = 0;
            }

            List<int> triangles = _subMeshTriangles[subMesh];
            triangles.Add(i0);
            triangles.Add(i1);
            triangles.Add(i2);
        }
    }
}
