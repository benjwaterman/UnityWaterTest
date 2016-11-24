using UnityEngine;
using System.Collections;

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
    public bool isResting;
    public Vector2 position;

    public WaterCell xPositiveNeighbour;
    public WaterCell xNegativeNeighbour;
    public WaterCell zPositiveNeighbour;
    public WaterCell zNegativeNeighbour;

    private WaterCell thisCell;

    void Start()
    {
        thisCell = WaterController.Current.waterCellArray[(int)position.x, (int)position.y];
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
        hasVolumeChanged = thisCell.hasVolumeChanged;
        isResting = thisCell.isResting;

        xPositiveVolume = xPositiveNeighbour.volume;
        xNegativeVolume = xNegativeNeighbour.volume;
        zPositiveVolume = zPositiveNeighbour.volume;
        zNegativeVolume = zNegativeNeighbour.volume;
    }

}