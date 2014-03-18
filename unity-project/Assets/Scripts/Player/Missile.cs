using UnityEngine;
using System.Collections;

public class Missile : MonoBehaviour
{
    /// <summary>
    /// The amount of time that the missile will fly for before being destroyed
    /// </summary>
    public static readonly float flyTime = 5f;

    /// <summary>
    /// The amount of damage that the missile does when hitting a player
    /// </summary>
    public int damage;
    public float speed;
    public bool isRedTeamOwner;

    private float flyTimer;

    // Use this for initialization
    private void Start()
    {

    }

    // Update is called once per frame
    private void Update()
    {
        if (flyTimer <= flyTime)
        {
            transform.position += transform.forward * speed;
            flyTimer += Time.deltaTime;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider otherCol)
    {
        GameObject other = otherCol.gameObject;

        switch (other.layer)
        {
            case Layers.Block:
                Destroy(gameObject);
                break;
            case Layers.RedTeam:
                if (!isRedTeamOwner)
                {
                    other.GetComponent<PlayerInfo>().Health -= damage;
                }
				Destroy(gameObject);
                break;
            case Layers.BluTeam:
                if (isRedTeamOwner)
                {
                    other.GetComponent<PlayerInfo>().Health -= damage;
                }
				Destroy(gameObject);
                break;
        }

        if (other.layer == Layers.Block)
        {
            Destroy(gameObject);
        }
    }
}
