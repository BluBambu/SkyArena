using System;
using System.Security.Policy;
using UnityEngine;
using System.Collections;

public class PlayerWeapon : MonoBehaviour
{

    /// <summary>
    /// The amount of time between missile shots
    /// </summary>
    public const float reloadTime = 0.25f;

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

    private float reloadTimer;

    public bool CanFire { get { return reloadTimer == 0; } }

    void Start()
    {
        if (!networkView.isMine)
        {
            enabled = false;
        }
    }

    private void Update()
    {
        if (reloadTimer != 0)
        {
            reloadTimer -= Time.deltaTime;

            reloadTimer = Math.Max(reloadTimer, 0);
        }
    }

    /// <summary>
    /// Fires a missile
    /// </summary>
    public void Fire()
    {
        if (CanFire)
        {
            Quaternion angle = new Quaternion();
            angle.eulerAngles = new Vector3(Camera.main.transform.eulerAngles.x, transform.rotation.eulerAngles.y,
                transform.rotation.eulerAngles.z);
            networkView.RPC("FireMissile", RPCMode.All, fireLoc.transform.position, angle, damage, missileSpeed);
            reloadTimer = reloadTime;
        }
    }

    /// <summary>
    /// Fires a missile once
    /// </summary>
    [RPC]
    private void FireMissile(Vector3 firingLoc, Quaternion angle, int damage, float speed)
    {
        GameObject missileClone = (Instantiate(missilePrefab, firingLoc, angle)) as GameObject;
        Missile missile = missileClone.GetComponent<Missile>();
        missile.damage = damage;
        missile.speed = speed;
        missile.isRedTeamOwner = gameObject.layer == Layers.RedTeam;
    }
}
