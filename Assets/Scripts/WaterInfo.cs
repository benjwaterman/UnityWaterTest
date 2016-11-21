using UnityEngine;
using System.Collections;

public class WaterInfo : MonoBehaviour
{
    public float density;
    public Vector3 velocity;
    public bool isResting;
    public float[] neighbourDensity = new float[4];
    public bool[] hasNeighbourChanged = new bool[4];

}
