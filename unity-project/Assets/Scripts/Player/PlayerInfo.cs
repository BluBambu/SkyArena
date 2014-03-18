using UnityEngine;
using System.Collections;

public class PlayerInfo : MonoBehaviour
{
    [HideInInspector]
    public Vector3 spawnPoint;

    /// <summary>
    /// The maximum amount of health
    /// </summary>
    public static readonly int maxHealth = 100;

    /// <summary>
    /// The current amount of health 
    /// </summary>
    public int Health
    {
        get { return health; }
        set
        {
            if (value <= 0)
            {
                Die();
                health = maxHealth;
            }
            else
            {
                health = value;
            }
        }
    }

    private int health;
    private GameObject _mesh;
	private TextMesh _nameTag;
	private string _playerName;

    private void Awake()
    {
        spawnPoint = new Vector3(World.worldX / 2, World.worldY, World.worldZ / 2);
    }

    // Use this for initialization
    void Start()
    {
        transform.position = spawnPoint;
        _mesh = transform.FindChild("Mesh").gameObject;
		_nameTag = transform.FindChild("NameTag").gameObject.GetComponent<TextMesh>();
    }

    // Update is called once per frame
    void Update()
    {
		if (!networkView.isMine){
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
    }

    public void Die()
    {
        transform.position = spawnPoint;
    }

	private void UpdateNameTagRotation()
	{
		_nameTag.text = _playerName;
		var rotate = Quaternion.LookRotation(Camera.main.transform.position - _nameTag.gameObject.transform.position);
		_nameTag.gameObject.transform.rotation = Quaternion.Slerp(_nameTag.gameObject.transform.rotation, rotate, 1f);
	}

    [RPC]
    public void SetTeam(bool isRedTeam)
    {
        if (isRedTeam)
        {
            gameObject.layer = Layers.RedTeam;
        }
        else
        {
            gameObject.layer = Layers.BluTeam;
        }
    }

	[RPC]
	public void SetName(string playerName){
		_playerName = playerName;
	}
}

