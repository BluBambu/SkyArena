using UnityEngine;
using System.Collections;

public class PlayerInfo : MonoBehaviour
{
    [HideInInspector]
    public Vector3 spawnPoint;

    private void Awake()
    {
        spawnPoint = new Vector3(World.worldX / 2, World.worldY, World.worldZ / 2);
    }

    // Use this for initialization
    void Start()
    {
        transform.position = spawnPoint;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Die()
    {
        transform.position = spawnPoint;
    }
}
