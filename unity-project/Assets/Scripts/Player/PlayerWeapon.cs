using UnityEngine;
using System.Collections;

public class PlayerWeapon : MonoBehaviour
{
    /// <summary>
    /// Where missile should be fired from
    /// </summary>
    public GameObject fireLoc;

    /// <summary>
    /// Reference to the missile prefab;
    /// </summary>
    public GameObject missilePrefab;

    public static readonly int damage = 10;

    public static readonly float missileSpeed = .25f;

    // Use this for initialization
    void Start()
    {
        if (!networkView.isMine)
        {
            enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonUp(1))
        {
            Quaternion angle = new Quaternion();
            angle.eulerAngles = new Vector3(Camera.main.transform.eulerAngles.x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
            networkView.RPC("Fire", RPCMode.All, fireLoc.transform.position, angle, damage, missileSpeed);
        }
    }

    [RPC]
    private void Fire(Vector3 firingLoc, Quaternion angle, int damage, float speed)
    {
        GameObject missileClone = (Instantiate(missilePrefab, firingLoc, angle)) as GameObject;
        Missile missile = missileClone.GetComponent<Missile>();
        missile.damage = damage;
        missile.speed = speed;
        missile.isRedTeamOwner = gameObject.layer == Layers.RedTeam;
    }
}
