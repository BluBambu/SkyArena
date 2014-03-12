using UnityEngine;
using System.Collections;

public class World : MonoBehaviour
{
    [HideInInspector]
    public Transform player;

    /// <summary>
    /// The maximum distance away from the player for a chunk to be loaded
    /// </summary>
    public const int DistToLoadChunk = ChunkSize * 3;

    /// <summary>
    /// The minimum distance away from the player for a chunk to be destroyed
    /// </summary>
    public const int DistToDestroyChunk = ChunkSize * 4;

    /// <summary>
    /// The width of a chunk
    /// </summary>
    public const int ChunkSize = 16;

    // Sizes of the world
    public const int worldX = 512;
    public const int worldY = 64;
    public const int worldZ = 512;

    public GameObject chunkPrefab;
    public Chunk[, ,] chunks;

    private byte[, ,] data;

    public void InitWorld()
    {
        data = new byte[worldX, worldY, worldZ];
        chunks = new Chunk[worldX / ChunkSize, worldY / ChunkSize, worldZ / ChunkSize];

        for (int x = 0; x < worldX; x++)
        {
            for (int z = 0; z < worldZ; z++)
            {
                int topY = worldY / 2 - 15 + (int)(15 * Mathf.PerlinNoise((float)x * .05f, (float)z * 0.05f));

                for (int y = 0; y < topY; y++)
                {
                    data[x, y, z] = Stone.ID;
                }

                for (int y = topY; y < worldY; y++)
                {
                    data[x, y, z] = Air.ID;
                }

                data[x, topY - 1, z] = Grass.ID;

                data[x, 0, z] = BedRock.ID;
            }
        }
    }

    private void Update()
    {
        if (NetworkManager.hasGameStarted)
        {
            UpdateChunks();

            if (Input.GetKeyUp(KeyCode.Escape))
            {
                Application.LoadLevel(0);
            }
        }
    }

    private void UpdateChunks()
    {
        for (int x = 0; x < chunks.GetLength(0); x++)
        {
            for (int z = 0; z < chunks.GetLength(2); z++)
            {
                float dist = Vector2.Distance(new Vector2(x * World.ChunkSize, z * World.ChunkSize), new Vector2(player.position.x, player.position.z));

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
            chunks[x, y, z] = (Instantiate(chunkPrefab, new Vector3(x * ChunkSize, y * ChunkSize, z * ChunkSize),
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

    public byte BlockAt(int x, int y, int z)
    {
        if (x >= worldX || x < 0 || y >= worldY || y < 0 || z >= worldZ || z < 0)
        {
            return Stone.ID;
        }
        return data[x, y, z];
    }

    public void ChangeBlock(int x, int y, int z, byte block)
    {
        networkView.RPC("ChangeBlockRPC", RPCMode.AllBuffered, x, y, z, (int)block);
    }

    [RPC]
    private void ChangeBlockRPC(int x, int y, int z, int block)
    {
        data[x, y, z] = (byte)block;
    }

    public void UpdateChunk(int x, int y, int z)
    {
        networkView.RPC("UpdateChunkRPC", RPCMode.All, x, y, z);
    }

    /// <summary>
    /// Updates the chunk at the specific location, also updates chunks around it if needed
    /// </summary>
    [RPC]
    public void UpdateChunkRPC(int x, int y, int z)
    {
        int updateX = Mathf.FloorToInt(x / (float)World.ChunkSize);
        int updateY = Mathf.FloorToInt(y / (float)World.ChunkSize);
        int updateZ = Mathf.FloorToInt(z / (float)World.ChunkSize);

        if (chunks[updateX, updateY, updateZ] != null)
            chunks[updateX, updateY, updateZ].shouldUpdate = true;

        if (x - (World.ChunkSize * updateX) == 0 && updateX != 0)
        {
            if (chunks[updateX - 1, updateY, updateZ] != null)
                chunks[updateX - 1, updateY, updateZ].shouldUpdate = true;
        }
        if (x - (World.ChunkSize * updateX) == 15 && updateX != chunks.GetLength(0) - 1)
        {
            if (chunks[updateX + 1, updateY, updateZ] != null)
                chunks[updateX + 1, updateY, updateZ].shouldUpdate = true;
        }
        if (y - (World.ChunkSize * updateY) == 0 && updateY != 0)
        {
            if (chunks[updateX, updateY - 1, updateZ] != null)
                chunks[updateX, updateY - 1, updateZ].shouldUpdate = true;
        }
        if (y - (World.ChunkSize * updateY) == 15 && updateY != chunks.GetLength(1) - 1)
        {
            if (chunks[updateX, updateY + 1, updateZ] != null)
                chunks[updateX, updateY + 1, updateZ].shouldUpdate = true;
        }
        if (z - (World.ChunkSize * updateZ) == 0 && updateZ != 0)
        {
            if (chunks[updateX, updateY, updateZ - 1] != null)
                chunks[updateX, updateY, updateZ - 1].shouldUpdate = true;
        }
        if (z - (World.ChunkSize * updateZ) == 15 && updateZ != chunks.GetLength(2) - 1)
        {
            if (chunks[updateX, updateY, updateZ + 1] != null)
                chunks[updateX, updateY, updateZ + 1].shouldUpdate = true;
        }
    }
}
