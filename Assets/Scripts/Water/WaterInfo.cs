using UnityEngine;
using System.Collections;
using Codes.Linus.IntVectors;

public class WaterInfo : MonoBehaviour
{
    public int id;
    public float volume;
    public float previousVolume;

    public float xPositiveVolume;
    public float xNegativeVolume;
    public float zPositiveVolume;
    public float zNegativeVolume;

    public bool hasVolumeChanged;
    public bool fDone; 
    public bool fActive; 
    public Vector2i position;

    public WaterCell xPositiveNeighbour;
    public WaterCell xNegativeNeighbour;
    public WaterCell zPositiveNeighbour;
    public WaterCell zNegativeNeighbour;

    private WaterCell thisCell;

    void Start()
    {
        thisCell = WaterController.Current.waterCellArray[position.x, position.y];
        xPositiveNeighbour = thisCell.getNeighbourData(Direction.xPositive);
        xNegativeNeighbour = thisCell.getNeighbourData(Direction.xNegative);
        zPositiveNeighbour = thisCell.getNeighbourData(Direction.zPositive);
        zNegativeNeighbour = thisCell.getNeighbourData(Direction.zNegative);
    }

    void Update()
    {
        id = thisCell.id;
        volume = thisCell.volume;
        previousVolume = thisCell.previousVolume;
        fDone = thisCell.fDone;
        fActive = thisCell.fDone;

        if(xPositiveNeighbour != null)
            xPositiveVolume = xPositiveNeighbour.volume;

        if (xNegativeNeighbour != null)
            xNegativeVolume = xNegativeNeighbour.volume;

        if (zPositiveNeighbour != null)
            zPositiveVolume = zPositiveNeighbour.volume;

        if (zNegativeNeighbour != null)
            zNegativeVolume = zNegativeNeighbour.volume;
    }

}