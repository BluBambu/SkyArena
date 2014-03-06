using System.Collections.Generic;
using Microsoft.Win32;
using UnityEngine;
using System.Collections;

public class Chunk : MonoBehaviour
{
    // locations of the block texture relative to the tile sheet
    private readonly Vector2 _grassSide = new Vector2(0, 0);
    private readonly Vector2 _grassTop = new Vector2(1, 0);
    private readonly Vector2 _stone = new Vector2(2, 0);
    private readonly Vector2 _dirt = new Vector2(3, 0);
    private readonly Vector3 _bedrock = new Vector3(0, 1);

    /// <summary>
    /// The percent width of a single texture in the texture atlas
    /// </summary>
    private const float _tileUnitPerc = 0.25f;

    /// <summary>
    /// A reference to the world script
    /// </summary>
    public World world;

    /// <summary>
    /// The position of the chunk in world coordinates
    /// </summary>
    public Vector3 position;

    public bool shouldUpdate;

    // Mesh data of the chunk
    private List<Vector3> _vertices;
    private List<int> _triangles;
    private List<Vector2> _uv;
    private Mesh _mesh;
    private MeshCollider _meshCol;

    // util field used in mesh generation
    private int _faceCount;

    private void Start()
    {
        _vertices = new List<Vector3>();
        _triangles = new List<int>();
        _uv = new List<Vector2>();

        _mesh = GetComponent<MeshFilter>().mesh;
        _meshCol = GetComponent<MeshCollider>();

        GenerateMesh();
    }

    private void Update()
    {
        if (shouldUpdate)
        {
            GenerateMesh();
            shouldUpdate = false;
        }
    }

    /// <summary>
    /// Generates a new mesh for the chunk, only faces that face an air block will be created
    /// </summary>
    private void GenerateMesh()
    {
        for (int x = 0; x < World.ChunkSize; x++)
        {
            for (int y = 0; y < World.ChunkSize; y++)
            {
                for (int z = 0; z < World.ChunkSize; z++)
                {
                    if (LocalBlockAt(x, y, z) != 0)
                    {
                        if (LocalBlockAt(x, y + 1, z) == Blocks.Air)
                        {
                            CubeTop(x, y, z, LocalBlockAt(x, y, z));
                        }
                        if (LocalBlockAt(x, y - 1, z) == Blocks.Air)
                        {
                            CubeBottom(x, y, z, LocalBlockAt(x, y, z));
                        }
                        if (LocalBlockAt(x + 1, y, z) == Blocks.Air)
                        {
                            CubeEast(x, y, z, LocalBlockAt(x, y, z));
                        }
                        if (LocalBlockAt(x - 1, y, z) == Blocks.Air)
                        {
                            CubeWest(x, y, z, LocalBlockAt(x, y, z));
                        }
                        if (LocalBlockAt(x, y, z + 1) == Blocks.Air)
                        {
                            CubeNorth(x, y, z, LocalBlockAt(x, y, z));
                        }
                        if (LocalBlockAt(x, y, z - 1) == Blocks.Air)
                        {
                            CubeSouth(x, y, z, LocalBlockAt(x, y, z));
                        }
                    }
                }
            }
        }

        UpdateMesh();
    }

    /// <summary>
    /// Commits the mesh changes to the mesh filter
    /// </summary>
    private void UpdateMesh()
    {
        _mesh.Clear();
        _mesh.vertices = _vertices.ToArray();
        _mesh.uv = _uv.ToArray();
        _mesh.triangles = _triangles.ToArray();
        _mesh.Optimize();
        _mesh.RecalculateNormals();

        _meshCol.sharedMesh = null;
        _meshCol.sharedMesh = _mesh;

        _vertices.Clear();
        _uv.Clear();
        _triangles.Clear();

        _faceCount = 0;
    }

    /// <summary>
    /// Gets the block given relative to the chunk's origin
    /// </summary>
    private byte LocalBlockAt(int x, int y, int z)
    {
        return world.Block(x + (int)position.x, y + (int)position.y, z + (int)position.z);
    }

    //----- Cube Face Generation -----
    private void CubeTop(int x, int y, int z, byte block)
    {
        _vertices.Add(new Vector3(x, y, z + 1));
        _vertices.Add(new Vector3(x + 1, y, z + 1));
        _vertices.Add(new Vector3(x + 1, y, z));
        _vertices.Add(new Vector3(x, y, z));

        switch (block)
        {
            case Blocks.Grass:
                Cube(_grassTop);
                break;
            case Blocks.Dirt:
                Cube(_dirt);
                break;
            case Blocks.Stone:
                Cube(_stone);
                break;
            case Blocks.Bedrock:
                Cube(_bedrock);
                break;
        }
    }

    private void CubeBottom(int x, int y, int z, byte block)
    {
        _vertices.Add(new Vector3(x, y - 1, z));
        _vertices.Add(new Vector3(x + 1, y - 1, z));
        _vertices.Add(new Vector3(x + 1, y - 1, z + 1));
        _vertices.Add(new Vector3(x, y - 1, z + 1));

        switch (block)
        {
            case Blocks.Grass:
            case Blocks.Dirt:
                Cube(_dirt);
                break;
            case Blocks.Stone:
                Cube(_stone);
                break;
            case Blocks.Bedrock:
                Cube(_bedrock);
                break;
        }
    }

    private void CubeNorth(int x, int y, int z, byte block)
    {
        _vertices.Add(new Vector3(x + 1, y - 1, z + 1));
        _vertices.Add(new Vector3(x + 1, y, z + 1));
        _vertices.Add(new Vector3(x, y, z + 1));
        _vertices.Add(new Vector3(x, y - 1, z + 1));

        switch (block)
        {
            case Blocks.Grass:
                Cube(_grassSide);
                break;
            case Blocks.Dirt:
                Cube(_dirt);
                break;
            case Blocks.Stone:
                Cube(_stone);
                break;
            case Blocks.Bedrock:
                Cube(_bedrock);
                break;
        }
    }

    private void CubeEast(int x, int y, int z, byte block)
    {
        _vertices.Add(new Vector3(x + 1, y - 1, z));
        _vertices.Add(new Vector3(x + 1, y, z));
        _vertices.Add(new Vector3(x + 1, y, z + 1));
        _vertices.Add(new Vector3(x + 1, y - 1, z + 1));

        switch (block)
        {
            case Blocks.Grass:
                Cube(_grassSide);
                break;
            case Blocks.Dirt:
                Cube(_dirt);
                break;
            case Blocks.Stone:
                Cube(_stone);
                break;
            case Blocks.Bedrock:
                Cube(_bedrock);
                break;
        }
    }

    private void CubeSouth(int x, int y, int z, byte block)
    {
        _vertices.Add(new Vector3(x, y - 1, z));
        _vertices.Add(new Vector3(x, y, z));
        _vertices.Add(new Vector3(x + 1, y, z));
        _vertices.Add(new Vector3(x + 1, y - 1, z));

        switch (block)
        {
            case Blocks.Grass:
                Cube(_grassSide);
                break;
            case Blocks.Dirt:
                Cube(_dirt);
                break;
            case Blocks.Stone:
                Cube(_stone);
                break;
            case Blocks.Bedrock:
                Cube(_bedrock);
                break;
        }
    }

    private void CubeWest(int x, int y, int z, byte block)
    {
        _vertices.Add(new Vector3(x, y - 1, z + 1));
        _vertices.Add(new Vector3(x, y, z + 1));
        _vertices.Add(new Vector3(x, y, z));
        _vertices.Add(new Vector3(x, y - 1, z));

        switch (block)
        {
            case Blocks.Grass:
                Cube(_grassSide);
                break;
            case Blocks.Dirt:
                Cube(_dirt);
                break;
            case Blocks.Stone:
                Cube(_stone);
                break;
            case Blocks.Bedrock:
                Cube(_bedrock);
                break;
        }
    }

    /// <summary>
    /// Should be called for every face creation, updates the triangles and the UV lines
    /// </summary>
    private void Cube(Vector2 texturePos)
    {
        _triangles.Add(_faceCount * 4);     //1
        _triangles.Add(_faceCount * 4 + 1); //2
        _triangles.Add(_faceCount * 4 + 2); //3
        _triangles.Add(_faceCount * 4);     //1
        _triangles.Add(_faceCount * 4 + 2); //3
        _triangles.Add(_faceCount * 4 + 3); //4

        _uv.Add(new Vector2(_tileUnitPerc * texturePos.x + _tileUnitPerc, _tileUnitPerc * texturePos.y));
        _uv.Add(new Vector2(_tileUnitPerc * texturePos.x + _tileUnitPerc, _tileUnitPerc * texturePos.y + _tileUnitPerc));
        _uv.Add(new Vector2(_tileUnitPerc * texturePos.x, _tileUnitPerc * texturePos.y + _tileUnitPerc));
        _uv.Add(new Vector2(_tileUnitPerc * texturePos.x, _tileUnitPerc * texturePos.y));

        _faceCount++;
    }
}
