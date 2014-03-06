using UnityEngine;
using System.Collections;

public class World : MonoBehaviour
{
    /// <summary>
    /// The dimensions of a chunk
    /// </summary>
    public const int ChunkSize = 16;

    /// <summary>
    /// The maximum distance away from the player for a chunk to be loaded
    /// </summary>
    public const int DistToLoadChunk = ChunkSize * 3;

    /// <summary>
    /// The minimum distance away from the player for a chunk to be destroyed
    /// </summary>
    public const int DistToDestroyChunk = ChunkSize * 4;

    // sizes of the world
    public static readonly int worldX = 512;
    public static readonly int worldY = 64;
    public static readonly int worldZ = 512;

    public GameObject chunkPrefab;
    public Chunk[, ,] chunks;

    private Transform player;

    /// <summary>
    /// The block data of the world
    /// </summary>
    public byte[, ,] data;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        chunks = new Chunk[Mathf.FloorToInt((float)worldX / ChunkSize), Mathf.FloorToInt((float)worldY / ChunkSize), Mathf.FloorToInt((float)worldZ / ChunkSize)];

        // locks the crusor
        Screen.lockCursor = true;

        InitData();
    }

    private void Update()
    {
        // unlocks the crusor when the user presses the escape key
        if (Input.GetKeyDown("escape"))
            Screen.lockCursor = false;

        UpdateChunks(player.position);
    }

    /// <summary>
    /// Initializes the block data of the world
    /// </summary>
    private void InitData()
    {
        data = new byte[worldX, worldY, worldZ];

        for (int x = 0; x < worldX; x++)
        {
            for (int z = 0; z < worldZ; z++)
            {
                int topoY = worldY / 2 - 15 + (int)(15 * Mathf.PerlinNoise((float)x * .05f, (float)z * .05f));

                for (int y = 0; y < topoY; y++)
                {
                    data[x, y, z] = Blocks.Stone;
                }

                data[x, topoY - 1, z] = Blocks.Grass;
                data[x, 0, z] = Blocks.Bedrock;
            }
        }
    }

    /// <summary>
    /// Loads and unloads chunks based on the given location, usually the player's location
    /// </summary>
    public void UpdateChunks(Vector3 pos)
    {
        for (int x = 0; x < chunks.GetLength(0); x++)
        {
            for (int z = 0; z < chunks.GetLength(2); z++)
            {
                float dist = Vector2.Distance(new Vector2(x * World.ChunkSize, z * World.ChunkSize), new Vector2(pos.x, pos.z));

                if (dist < DistToLoadChunk)
                {
                    if (chunks[x, 0, z] == null)
                    {
                        GenerateColumn(x, z);
                    }
                }
                else if (dist > DistToDestroyChunk)
                {
                    if (chunks[x, 0, z] != null)
                    {
                        DestroyColumn(x, z);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Generates all of the chunks in the specificed column
    /// </summary>
    private void GenerateColumn(int x, int z)
    {
        for (int y = 0; y < chunks.GetLength(1); y++)
        {
            chunks[x, y, z] = (Instantiate(chunkPrefab,
                new Vector3(x * ChunkSize, y * ChunkSize, z * ChunkSize),
                new Quaternion(0, 0, 0, 0)) as GameObject).GetComponent<Chunk>();

            Chunk chunkScript = chunks[x, y, z];

            chunkScript.world = this;
            chunkScript.position = new Vector3(x * ChunkSize, y * ChunkSize, z * ChunkSize);
        }
    }

    /// <summary>
    /// Destroys all of the chunks in the specificed column
    /// </summary>
    private void DestroyColumn(int x, int z)
    {
        for (int y = 0; y < chunks.GetLength(1); y++)
        {
            Destroy(chunks[x, y, z].gameObject);
        }
    }

    /// <summary>
    /// Gets the type of block in the world given its coordinates, if out of bounds then the byte equivalent of 0 will be returned
    /// </summary>
    public byte Block(int x, int y, int z)
    {
        if (x >= worldX || x < 0 || y >= worldY || y < 0 || z >= worldZ || z < 0)
        {
            return (byte)0;
        }
        return data[x, y, z];
    }
}
