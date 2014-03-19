using UnityEngine;
using System.Collections;

public class Missile : MonoBehaviour
{
    /// <summary>
    /// The amount of time that the missile will fly before being destroyed
    /// </summary>
    public static readonly float flyTime = 5f;

    /// <summary>
    /// The amount of damage that the missile does when hitting a player
    /// </summary>
    public int damage;

    /// <summary>
    /// How fast, in meters per second, the missile is traveling
    /// </summary>
    public float speed;

    /// <summary>
    /// Which team shot the missile
    /// </summary>
    public bool isRedTeamOwner;

    /// <summary>
    /// The amount of time that the missile has flown for
    /// </summary>
    private float flyTimer;

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

    /// <summary>
    /// Called when the missile collides with another object
    /// </summary>
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
