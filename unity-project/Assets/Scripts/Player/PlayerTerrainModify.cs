using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;

public class PlayerTerrainModify : MonoBehaviour
{
    /// <summary>
    /// The amount of resistance that damage block gain back every second
    /// </summary>
    private const float BlockRecoverRate = 1f;

    /// <summary>
    /// The rate at which the pickaxe tool breaks down resistance
    /// </summary>
    private const float PickaxeDamageRate = 5f;

    /// <summary>
    /// The maximum distance the player can be from a block to modify it
    /// </summary>
    public const float MaxDistToModifyBlock = 5;

    /// <summary>
    /// The amount of delay there is in seconds between placing blocks
    /// </summary>
    public static float TimeToPlaceBlock = 0.25f;

    private World world;

    public Dictionary<Vector3, float> blockDamages;

    private bool _isBreakingBlock;
    private PlayerInfo _playerInfo;
    private ParticleSystem _breakParticleSystem;

    void OnGUI()
    {
        GUI.Box(new Rect(Screen.width / 2, Screen.height / 2, 10, 10), "");
    }

    private void Start()
    {
        GameObject worldGO = GameObject.Find("_Scripts");
        world = worldGO.GetComponent<World>();
        blockDamages = new Dictionary<Vector3, float>();
        _breakParticleSystem = transform.FindChild("BreakParticleSystem").GetComponent<ParticleSystem>();
        _playerInfo = GetComponent<PlayerInfo>();
    }

    private void Update()
    {
        foreach (var pos in blockDamages.Keys.ToList())
        {
            blockDamages[pos] -= BlockRecoverRate * Time.deltaTime;
        }

        var toRemove = blockDamages.Where(pair => pair.Value < 0)
                     .Select(pair => pair.Key)
                     .ToList();

        foreach (var key in toRemove)
        {
            blockDamages.Remove(key);
        }

        if (_isBreakingBlock)
        {
            if (!_breakParticleSystem.isPlaying)
            {
                _breakParticleSystem.Play();
            }
        }
        else
        {
            _breakParticleSystem.Stop();
        }
    }

    public void DamageBlockInFront()
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

                networkView.RPC("DamageBlock", RPCMode.AllBuffered, new Vector3(hitX, hitY, hitZ), PickaxeDamageRate * Time.deltaTime);
            }
        }
    }

    [RPC]
    public void SetIsBreakingBlock(bool isBreaking)
    {
        _isBreakingBlock = isBreaking;
    }

    /// <summary>
    /// Records damage on a certain block, if it is higher than its resistance then it will be destroyed
    /// </summary>
    [RPC]
    private void DamageBlock(Vector3 pos, float damage)
    {
        if (!blockDamages.ContainsKey(pos))
        {
            blockDamages.Add(pos, 0);
        }
        blockDamages[pos] += damage;

        float blockResistance = Blocks.GetResistance(world.BlockAt((int)pos.x, (int)pos.y, (int)pos.z));

        if (blockResistance != -1 && blockDamages[pos] >= blockResistance)
        {
            _playerInfo.AddBlockToInv(world.BlockAt((int)pos.x, (int)pos.y, (int)pos.z));
            ChangeBlockAt(pos, Air.ID);
        }

        _breakParticleSystem.transform.position = pos + new Vector3(.5f, 0, .5f);
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
                Vector3 hitPos = hit.point;

                hitPos += new Vector3(hit.normal.x, hit.normal.y, hit.normal.z) * -0.5f;

                int hitX = (int)hitPos.x;
                int hitY = (int)hitPos.y + 1;
                int hitZ = (int)hitPos.z;

                ChangeBlockAt(hitX, hitY, hitZ, 0);
            }
        }
    }

    /// <summary>
    /// Attempts place a block on top of the block that the crusor is currently pointed at
    /// </summary>
    public bool PlaceBlockInFront(byte block)
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.distance <= MaxDistToModifyBlock)
            {
                Vector3 hitPos = hit.point;

                hitPos += new Vector3(hit.normal.x, hit.normal.y, hit.normal.z) * 0.5f;

                int hitX = (int)hitPos.x;
                int hitY = (int)hitPos.y + 1;
                int hitZ = (int)hitPos.z;

                if (!Physics.CheckCapsule(new Vector3(hitX + .5f, hitY, hitZ + 0.5f), new Vector3(hitX + .5f, hitY + 1f, hitZ + 0.5f), .5f))
                {
                    ChangeBlockAt(hitX, hitY, hitZ, block);
                    return true;
                }
            }
        }
        return false;
    }

    public void ChangeBlockAt(Vector3 pos, byte to)
    {
        world.ChangeBlock((int)pos.x, (int)pos.y, (int)pos.z, to);
        world.UpdateChunk((int)pos.x, (int)pos.y, (int)pos.z);
    }
    public void ChangeBlockAt(int x, int y, int z, byte to)
    {
        world.ChangeBlock(x, y, z, to);
        world.UpdateChunk(x, y, z);
    }
}
