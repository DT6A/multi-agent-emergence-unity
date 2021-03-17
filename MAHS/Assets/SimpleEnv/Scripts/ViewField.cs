using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewField : MonoBehaviour
{
    public float viewAngle = 135.0f;
    public float viewRadius = 10.0f;

    public List<GameObject> collectVisibleObjects()
    {
        List<GameObject> objects = new List<GameObject>();

        Collider[] colliders = Physics.OverlapSphere(transform.position, viewRadius);
        for (int i = 0; i < colliders.Length; i++)
        {
            Transform trans = colliders[i].transform;
            Vector3 dirToTarget = (trans.position - transform.position).normalized;

            if (!colliders[i].CompareTag("Wall")  && Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2.0f)
            {
                float dist = Vector3.Distance(trans.position, transform.position);
                RaycastHit hit;
                
                if (Physics.Raycast(transform.position, dirToTarget, out hit, dist))
                {
                    if (colliders[i].gameObject.GetInstanceID() == hit.transform.gameObject.GetInstanceID())
                        objects.Add(colliders[i].gameObject);
                }
            }
        }
        
        return objects;
    }

    public Vector3 DirFromAngle(float angle)
    {
        angle += transform.eulerAngles.y;
        return new Vector3((float) Math.Sin(angle * Mathf.Deg2Rad), 0, (float) Math.Cos(angle * Mathf.Deg2Rad));
    }
}
