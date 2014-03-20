using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class PlayerInfo : MonoBehaviour
{
    public enum ActiveHandItem
    {
        Weapon, Pickaxe, Block, None
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

    public Dictionary<byte, int> blockInv;

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

    void Start()
    {
        blockInv = new Dictionary<byte, int>();
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
    }

    private void CheckMouseInput()
    {
        bool isBreakingBlock = false;

        if (Input.GetMouseButtonDown(0))
        {
            switch (activeHandItem)
            {
                case ActiveHandItem.Weapon:
                    _playerWeapon.Fire();
                    break;
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            switch (activeHandItem)
            {
                case ActiveHandItem.Weapon:
                    _playerWeapon.Fire();
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

