using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;

public class PlayerInfo : MonoBehaviour
{
    public enum ActiveHandItem
    {
        Slot1, Slot2, Slot3, Weapon, Pickaxe
    }

    public ActiveHandItem activeHandItem;

    /// <summary>
    /// The maximum amount of health
    /// </summary>
    public static readonly int maxHealth = 100;

    /// <summary>
    /// The current amount of health 
    /// </summary>
    public int Health
    {
        get { return _health; }
        set
        {
            if (value <= 0)
            {
                Die();
                _health = maxHealth;
            }
            else
            {
                _health = value;
            }
        }
    }

    public SortedDictionary<byte, int> blockInv;

    /// <summary>
    /// Backing field for the health property
    /// </summary>
    private int _health;

    /// <summary>
    /// A reference to the mesh of the player
    /// </summary>
    private GameObject _mesh;

    /// <summary>
    /// The reference to the 3D text mesh that displays the player's name
    /// </summary>
    private TextMesh _nameTag;

    /// <summary>
    /// The name of the player
    /// </summary>
    private string _playerName;
    private NetworkManager _networkManager;
    private PlayerTerrainModify _playerTerrainModify;
    private PlayerWeapon _playerWeapon;
    public bool isBluTeam;
    private bool _isBreakingBlock;
    public bool IsBreakingBlock
    {
        get { return _isBreakingBlock; }
        set
        {
            _isBreakingBlock = value;
            networkView.RPC("SetIsBreakingBlock", RPCMode.AllBuffered, value);
        }
    }

    private void OnGUI()
    {
        GUI.Button(new Rect(50, 125, 50, 50), "1");
        GUI.Label(new Rect(125, 140, 50, 50), "Weapon");
        GUI.Button(new Rect(50, 200, 50, 50), "2");
        GUI.Label(new Rect(125, 215, 50, 50), "Pickaxe");

        if (activeHandItem == ActiveHandItem.Weapon)
        {
            GUI.Label(new Rect(105, 140, 50, 50), "<");
        }
        else if (activeHandItem == ActiveHandItem.Pickaxe)
        {
            GUI.Label(new Rect(105, 215, 50, 50), "<");
        }

        var keys = blockInv.Keys.ToArray();

        for (int i = 0; i < keys.Length; i++)
        {
            GUI.Button(new Rect(50, 200 + 75 * (i + 1), 50, 50), i + 3 + "");
            GUI.Label(new Rect(125, 215 + 75 * (i + 1), 50, 50), Blocks.GetName(keys[i]) + ": " + blockInv[keys[i]]);
            if ((int)activeHandItem == i)
            {
                GUI.Label(new Rect(105, 215 + 75 * (i + 1), 50, 50), "<");
            }
        }
    }

    void Start()
    {
        blockInv = new SortedDictionary<byte, int>();
        _mesh = transform.FindChild("Mesh").gameObject;
        _nameTag = transform.FindChild("NameTag").gameObject.GetComponent<TextMesh>();
        _playerTerrainModify = GetComponent<PlayerTerrainModify>();
        _playerWeapon = GetComponent<PlayerWeapon>();
        activeHandItem = ActiveHandItem.Pickaxe;
        _networkManager = GameObject.Find("_Scripts").GetComponent<NetworkManager>();

        if (isBluTeam)
        {
            transform.position = World.BluTeamRespawnPoint;
        }
        else
        {
            transform.position = World.RedTeamRespawnPoint;
        }
    }

    void Update()
    {
        if (!networkView.isMine)
        {
            if (gameObject.layer == Layers.BluTeam)
            {
                _mesh.renderer.material.color = Color.blue;
            }
            else if (gameObject.layer == Layers.RedTeam)
            {
                _mesh.renderer.material.color = Color.red;
            }

            UpdateNameTagRotation();
        }
        else
        {
            CheckHandItemSwitch();
            CheckMouseInput();
            CheckInv();
        }
    }

    private void CheckInv()
    {
        var toRemove = blockInv.Where(pair => pair.Value <= 0)
             .Select(pair => pair.Key)
             .ToList();

        foreach (var key in toRemove)
        {
            blockInv.Remove(key);
        }
    }

    private void CheckHandItemSwitch()
    {
        if (Input.GetKeyUp(KeyCode.Alpha1))
        {
            activeHandItem = ActiveHandItem.Weapon;
        }
        else if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            activeHandItem = ActiveHandItem.Pickaxe;
        }
        else if (Input.GetKeyUp(KeyCode.Alpha3))
        {
            activeHandItem = ActiveHandItem.Slot1;
        }
        else if (Input.GetKeyUp(KeyCode.Alpha4))
        {
            activeHandItem = ActiveHandItem.Slot2;
        }
        else if (Input.GetKeyUp(KeyCode.Alpha5))
        {
            activeHandItem = ActiveHandItem.Slot3;
        }
    }

    private void CheckMouseInput()
    {
        bool isBreakingBlock = false;

        if (Input.GetMouseButtonDown(0))
        {
            var keys = blockInv.Keys.ToArray();
            switch (activeHandItem)
            {
                case ActiveHandItem.Weapon:
                    _playerWeapon.Fire();
                    break;
                case ActiveHandItem.Slot1:
                    if (keys.Length > 0)
                    {
                        if (_playerTerrainModify.PlaceBlockInFront((byte)keys[0]))
                        {
                            blockInv[keys[0]]--;
                        }
                    }
                    break;
                case ActiveHandItem.Slot2:
                    if (keys.Length > 1)
                    {
                        if (_playerTerrainModify.PlaceBlockInFront((byte)keys[1]))
                        {
                            blockInv[keys[1]]--;
                        }
                    }
                    break;
                case ActiveHandItem.Slot3:
                    if (keys.Length > 2)
                    {
                        if (_playerTerrainModify.PlaceBlockInFront((byte)keys[2]))
                        {
                            blockInv[keys[2]]--;
                        }
                    }
                    break;
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            var keys = blockInv.Keys.ToArray();
            switch (activeHandItem)
            {
                case ActiveHandItem.Weapon:
                    _playerWeapon.Fire();
                    break;
                case ActiveHandItem.Slot1:
                    if (keys.Length > 0)
                    {
                        if (_playerTerrainModify.PlaceBlockInFront((byte)keys[0]))
                        {
                            blockInv[keys[0]]--;
                        }
                    }
                    break;
                case ActiveHandItem.Slot2:
                    if (keys.Length > 1)
                    {
                        if (_playerTerrainModify.PlaceBlockInFront((byte)keys[1]))
                        {

                            blockInv[keys[1]]--;
                        }
                    }
                    break;
                case ActiveHandItem.Slot3:
                    if (keys.Length > 2)
                    {
                        if (_playerTerrainModify.PlaceBlockInFront((byte)keys[2]))
                        {
                            blockInv[keys[2]]--;
                        }
                    }
                    break;
            }
        }
        else if (Input.GetMouseButton(0))
        {
            switch (activeHandItem)
            {
                case ActiveHandItem.Pickaxe:
                    _playerTerrainModify.DamageBlockInFront();
                    isBreakingBlock = true;
                    break;
            }
        }
        else if (Input.GetMouseButton(1))
        {
            switch (activeHandItem)
            {
                case ActiveHandItem.Pickaxe:
                    _playerTerrainModify.DamageBlockInFront();
                    isBreakingBlock = true;
                    break;
            }
        }

        if (isBreakingBlock != IsBreakingBlock)
        {
            IsBreakingBlock = isBreakingBlock;
        }
    }

    /// <summary>
    /// When the player should die, this method should be called
    /// </summary>
    public void Die()
    {
        if (isBluTeam)
        {
            transform.position = World.BluTeamRespawnPoint;
            if (networkView.isMine)
            {
                _networkManager.networkView.RPC("IncreaseRedTeamKills", RPCMode.AllBuffered);
            }
        }
        else
        {
            transform.position = World.RedTeamRespawnPoint;
            if (networkView.isMine)
            {
                _networkManager.networkView.RPC("IncreaseBluTeamKills", RPCMode.AllBuffered);
            }
        }
    }

    /// <summary>
    /// Rotates the name text mesh so that it faces the camera
    /// </summary>
    private void UpdateNameTagRotation()
    {
        _nameTag.text = _playerName;
        var rotate = Quaternion.LookRotation(Camera.main.transform.position - _nameTag.gameObject.transform.position);
        _nameTag.gameObject.transform.rotation = Quaternion.Slerp(_nameTag.gameObject.transform.rotation, rotate, 1f);
    }

    public void AddBlockToInv(byte block)
    {
        if (block == BedRock.ID || block == Air.ID)
        {
            return;
        }

        if (block == Grass.ID)
        {
            block = Dirt.ID;
        }

        if (!blockInv.ContainsKey(block))
        {
            blockInv.Add(block, 0);
        }
        blockInv[block]++;
    }

    /// <summary>
    /// Sets what team the player is on
    /// </summary>
    [RPC]
    public void SetTeam(bool isRedTeam)
    {
        gameObject.layer = isRedTeam ? Layers.RedTeam : Layers.BluTeam;
        isBluTeam = !isRedTeam;
    }

    /// <summary>
    /// Sets what the name of the player is
    /// </summary>
    [RPC]
    public void SetName(string playerName)
    {
        _playerName = playerName;
    }
}

