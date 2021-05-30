using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ViewField))]
public class ViewFieldEditor : Editor
{
    private void OnSceneGUI()
    {
        ViewField vw = (ViewField) target;

        Handles.color = new Color(0, 1, 0, (float) 0.1);
        
        Handles.DrawSolidArc(vw.transform.position, Vector3.up, vw.DirFromAngle(-vw.viewAngle / 2),
            vw.viewAngle, vw.viewRadius);

        Vector3 viewAngleA = vw.DirFromAngle(-vw.viewAngle / 2);
        Vector3 viewAngleB = vw.DirFromAngle(vw.viewAngle / 2);

        Handles.DrawLine(vw.transform.position, vw.transform.position + viewAngleA * vw.viewRadius);
        Handles.DrawLine(vw.transform.position, vw.transform.position + viewAngleB * vw.viewRadius);

        Handles.color = Color.magenta;
        foreach (GameObject obj in vw.collectVisibleObjects())
        {
            Handles.DrawLine(vw.transform.position, obj.transform.position);
        }
    }
}