using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Basic_Tower : MonoBehaviour
{
    public Transform target;
    public float range = 15f;

    public string enemyTag = "Enemy";
    public float turnSpeed = 10f;

    public Transform part2Rotate;
    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("UpdateTarget", 0f, 0.5f);   
    }

    // Update is called once per frame
    void Update()
    {
        if (target == null)
        {
            return; 
        }

        Vector2 dir = target.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(dir);
        Vector3 rotation = Quaternion.Lerp(part2Rotate.rotation, lookRotation, Time.deltaTime * turnSpeed).eulerAngles;
        part2Rotate.rotation = Quaternion.Euler(0f, 0f, rotation.z);
    }

    void UpdateTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        float shortestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;

        foreach(GameObject enemy in enemies)
        {
            float distance2Enemy = Vector2.Distance(transform.position, enemy.transform.position);
            if(distance2Enemy< shortestDistance)
            {
                shortestDistance = distance2Enemy;
                nearestEnemy = enemy;
            }
        }

        if(nearestEnemy != null && shortestDistance <= range)
        {
            target = nearestEnemy.transform;
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
