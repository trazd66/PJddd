using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
public class CatmullRomSpline
{

    // Parametric constant: 0.0 for the uniform spline, 0.5 for the centripetal spline, 1.0 for the chordal spline
    public static float alpha = 0.5f;

	static float GetT(float t, Vector3 p0, Vector3 p1)
	{
		float a = math.pow((p1.x - p0.x), 2.0f) + math.pow((p1.y - p0.y), 2.0f) + math.pow((p1.z - p0.z), 2.0f);
		float b = math.pow(a, alpha * 0.5f);

		return (b + t);
	}

	public static List<Vector3> getPoints(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3,int numOfPoints = 10)
	{
		List<Vector3> newPoints = new List<Vector3>();

		float t0 = 0.0f;
		float t1 = GetT(t0, p0, p1);
		float t2 = GetT(t1, p1, p2);
		float t3 = GetT(t2, p2, p3);

		for (float t = t1; t < t2; t += ((t2 - t1) / (float)numOfPoints))
		{
			Vector3 A1 = (t1 - t) / (t1 - t0) * p0 + (t - t0) / (t1 - t0) * p1;
			Vector3 A2 = (t2 - t) / (t2 - t1) * p1 + (t - t1) / (t2 - t1) * p2;
			Vector3 A3 = (t3 - t) / (t3 - t2) * p2 + (t - t2) / (t3 - t2) * p3;

			Vector3 B1 = (t2 - t) / (t2 - t0) * A1 + (t - t0) / (t2 - t0) * A2;
			Vector3 B2 = (t3 - t) / (t3 - t1) * A2 + (t - t1) / (t3 - t1) * A3;

			Vector3 C = (t2 - t) / (t2 - t1) * B1 + (t - t1) / (t2 - t1) * B2;

			newPoints.Add(C);
		}

		return newPoints;
	}

}
