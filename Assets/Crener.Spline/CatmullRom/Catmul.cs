using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Catmul : MonoBehaviour {

    // Use the transforms of GameObjects in 3d space as your points or define array with desired points
    public Transform[] points;
	
    // Store points on the Catmull curve so we can visualize them
    List<Vector2> newPoints = new List<Vector2>();
	
    // How many points you want on the curve
    uint numberOfPoints = 10;
	
    // Parametric constant: 0.0 for the uniform spline, 0.5 for the centripetal spline, 1.0 for the chordal spline
    public float alpha = 0.5f;
	
    /////////////////////////////

    void Update()
    {
        CatmulRom();
    }
	
    void CatmulRom()
    {
        newPoints.Clear();

        Vector2 p0 = transform.GetChild(0).position;
        Vector2 p1 = transform.GetChild(1).position;
        Vector2 p2 = transform.GetChild(2).position;
        Vector2 p3 = transform.GetChild(3).position;

        float t0 = 0.0f;
        float start = GetT(t0, p0, p1);
        float end = GetT(start, p1, p2);
        float t3 = GetT(end, p2, p3);

        for (float t=start; t<end; t+=((end-start)/(float)numberOfPoints))
        {
            Vector2 A1 = (start-t)/(start-t0)*p0 + (t-t0)/(start-t0)*p1;
            Vector2 A2 = (end-t)/(end-start)*p1 + (t-start)/(end-start)*p2;
            Vector2 A3 = (t3-t)/(t3-end)*p2 + (t-end)/(t3-end)*p3;
		    
            Vector2 B1 = (end-t)/(end-t0)*A1 + (t-t0)/(end-t0)*A2;
            Vector2 B2 = (t3-t)/(t3-start)*A2 + (t-start)/(t3-start)*A3;
		    
            Vector2 C = (end-t)/(end-start)*B1 + (t-start)/(end-start)*B2;
		    
            newPoints.Add(C);
        }
    }

    float GetT(float t, Vector2 p0, Vector2 p1)
    {
        float a = Mathf.Pow((p1.x-p0.x), 2.0f) + Mathf.Pow((p1.y-p0.y), 2.0f);
        float b = Mathf.Pow(a, alpha * 0.5f);
	   
        return (b + t);
    }
	
    // Visualize the points
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        foreach (Vector2 temp in newPoints)
        {
            Vector3 pos = new Vector3(temp.x, temp.y, 0);
            Gizmos.DrawSphere(pos, 0.3f);
        }
    }
}