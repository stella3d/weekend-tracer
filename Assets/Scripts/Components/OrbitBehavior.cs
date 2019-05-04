using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitBehavior : MonoBehaviour
{
    public Vector3 Point; 
    public float Radius;
    public float yAnchor;
    
    void Start()
    {
        yAnchor = Point.y;
    }

    void Update()
    {
        var pos = transform.position;
        //pos.y += Mathf.Sin(Time.time) * 0.05f - 0.005f;
        //pos.y = Mathf.Clamp(pos.y, Point.y, 2f);
        //transform.position = pos;
        
        //transform.RotateAround(Point, Vector3.up, 30 * Time.deltaTime);
        transform.LookAt(Point);
    }
}
