using System;
using UnityEngine;
using System.Collections;

public class PlayerTerrainModify : MonoBehaviour
{
    /// <summary>
    /// The maximum distance the player can be from a block to modify it
    /// </summary>
    public const float MaxDistToModifyBlock = 5;

    /// <summary>
    /// The amount of delay there is in seconds between placing blocks
    /// </summary>
    public static float TimeToPlaceBlock = 0.25f;

    private World world;
    private GameObject mainCamera;

    private float _blockPlaceTimer;

    private float _blockBreakTimer;

    void OnGUI()
    {
        GUI.Box(new Rect(Screen.width / 2, Screen.height / 2, 10, 10), "");
    }

    private void Awake()
    {
        GameObject worldGO = GameObject.Find("_Scripts");
        world = worldGO.GetComponent<World>();
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            DestroyBlockInFront();
        }

        if (_blockPlaceTimer == 0)
        {
            if (Input.GetMouseButton(1))
            {
                PlaceBlockInFront();
            }
        }
        else
        {
            _blockPlaceTimer -= Time.deltaTime;

            _blockPlaceTimer = Math.Max(_blockPlaceTimer, 0);
        }
    }

    /// <summary>
    /// Destroys the block in front of the player where the crusor is aimed at
    /// </summary>
    public void DestroyBlockInFront()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.distance <= MaxDistToModifyBlock)
            {
                Debug.DrawRay(ray.origin + new Vector3(1, 1, 1), ray.direction * 5, Color.green, 2);

                Vector3 hitPos = hit.point;

                hitPos += new Vector3(hit.normal.x, hit.normal.y, hit.normal.z) * -0.5f;

                int hitX = (int)hitPos.x;
                int hitY = (int)hitPos.y + 1;
                int hitZ = (int)hitPos.z;

                world.data[hitX, hitY, hitZ] = Blocks.Air;
                UpdateChunkAt(hitX, hitY, hitZ);
            }
        }
    }

    /// <summary>
    /// Attempts place a block on top of the block that the crusor is currently pointed at
    /// </summary>
    public void PlaceBlockInFront()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.distance <= MaxDistToModifyBlock)
            {
                Debug.DrawRay(ray.origin + new Vector3(1, 1, 1), ray.direction * 5, Color.green, 2);

                Vector3 hitPos = hit.point;

                hitPos += new Vector3(hit.normal.x, hit.normal.y, hit.normal.z) * 0.5f;

                int hitX = (int)hitPos.x;
                int hitY = (int)hitPos.y + 1;
                int hitZ = (int)hitPos.z;

                world.data[hitX, hitY, hitZ] = Blocks.Dirt;
                UpdateChunkAt(hitX, hitY, hitZ);
            }
        }
        ResetBlockPlaceTimer();
    }

    private void ResetBlockPlaceTimer()
    {
        _blockPlaceTimer = TimeToPlaceBlock;
    }

    /// <summary>
    /// Updates the chunk at the specific location, also updates chunks around it if needed
    /// </summary>
    public void UpdateChunkAt(int x, int y, int z)
    {
        int updateX = Mathf.FloorToInt(x / (float)World.ChunkSize);
        int updateY = Mathf.FloorToInt(y / (float)World.ChunkSize);
        int updateZ = Mathf.FloorToInt(z / (float)World.ChunkSize);

        world.chunks[updateX, updateY, updateZ].shouldUpdate = true;

        if (x - (World.ChunkSize * updateX) == 0 && updateX != 0)
        {
            world.chunks[updateX - 1, updateY, updateZ].shouldUpdate = true;
        }
        if (x - (World.ChunkSize * updateX) == 15 && updateX != world.chunks.GetLength(0) - 1)
        {
            world.chunks[updateX + 1, updateY, updateZ].shouldUpdate = true;
        }
        if (y - (World.ChunkSize * updateY) == 0 && updateY != 0)
        {
            world.chunks[updateX, updateY - 1, updateZ].shouldUpdate = true;
        }
        if (y - (World.ChunkSize * updateY) == 15 && updateY != world.chunks.GetLength(1) - 1)
        {
            world.chunks[updateX, updateY + 1, updateZ].shouldUpdate = true;
        }
        if (z - (World.ChunkSize * updateZ) == 0 && updateZ != 0)
        {
            world.chunks[updateX, updateY, updateZ - 1].shouldUpdate = true;
        }
        if (z - (World.ChunkSize * updateZ) == 15 && updateZ != world.chunks.GetLength(2) - 1)
        {
            world.chunks[updateX, updateY, updateZ + 1].shouldUpdate = true;
        }
    }
}
