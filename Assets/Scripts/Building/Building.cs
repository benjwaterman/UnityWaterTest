using UnityEngine;
using System.Collections;

public enum BuildingType { Sandbags, Wall };

public abstract class Building : MonoBehaviour {

    public string buildingName;
    public string buildingHealth;
    public int buildingCost;
}
