using UnityEngine;
using System.Collections;

public class Water
{
    public float density;
    public Vector3 velocity;
    public bool isResting;
    public float[] neighbourDensity = new float[4];
    public bool[] hasNeighbourChanged = new bool[4];

    public Water()
    {
        density = 0;
        velocity = Vector3.zero;
        isResting = false;

        for (int i = 0; i < neighbourDensity.Length; i++)
        {
            neighbourDensity[i] = -1;
            hasNeighbourChanged[i] = true;
        }
    }
}
