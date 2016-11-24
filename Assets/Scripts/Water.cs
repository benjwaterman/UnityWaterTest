using UnityEngine;
using System.Collections;

public enum Direction { xPositive, xNegative, zPositive, zNegative };
public enum WaterDataType { velocity, volume };

public class WaterCell
{
    //GameObject this data refers to
    GameObject waterGameObject;
    //Keep track of each id
    public int id;
    public float volume;
    public float previousVolume;
    public bool hasVolumeChanged;
    public bool isResting;
    public Vector3 position;
    public Vector3 velocity;
    public NeighbourWaterCell neighbour;

    public WaterCell()
    {
        volume = 0;
        previousVolume = -1;
        hasVolumeChanged = true;
        isResting = false;
        velocity = Vector3.zero;
        position = Vector3.zero;

        //Must manually populate neighbour references if using this method
    }

    public WaterCell(float volume, Vector3 velocity, Vector3 position, int id)
    {
        this.volume = volume;
        this.velocity = velocity;
        this.position = position;
        this.id = id;

        //Default values
        previousVolume = -1;
        hasVolumeChanged = true;
        isResting = false;
    }

    public void populateNeighbourReferences()
    {
        //Check if array index is in range, if it is assign reference
        if (isInRange(Direction.xPositive))
            neighbour.xPositive = WaterController.Current.waterCellArray[(int)position.x + 1, (int)position.z];

        if (isInRange(Direction.xNegative))
            neighbour.xNegative= WaterController.Current.waterCellArray[(int)position.x - 1, (int)position.z];

        if (isInRange(Direction.zPositive))
            neighbour.zPositive = WaterController.Current.waterCellArray[(int)position.x, (int)position.z + 1];

        if (isInRange(Direction.zNegative))
            neighbour.zNegative = WaterController.Current.waterCellArray[(int)position.x, (int)position.z - 1];
    }

    public void setGameObject(GameObject go)
    {
        waterGameObject = go;
    }

    public GameObject getGameObject()
    {
        return waterGameObject;
    }

    public void transformGameObject(Vector3 position)
    {
        waterGameObject.transform.position = position;
        this.position = position;
    }

    public WaterCell getNeighbourData(Direction direction)
    {
        switch (direction)
        {
            case Direction.xPositive:
                if (neighbour.xPositive == null)
                {
                    int i = 0;
                }
                return neighbour.xPositive;
            case Direction.xNegative:
                return neighbour.xNegative;
            case Direction.zPositive:
                return neighbour.zPositive;
            case Direction.zNegative:
                return neighbour.zNegative;
            default:
                return null;
        }
    }

    public NeighbourWaterCell getNeighbourData()
    {
        return neighbour;
    }

    public void setNeighbourData(Direction direction, WaterDataType dataType, float vol, Vector3 vel)
    {
        if (dataType == WaterDataType.velocity)
        {
            if (vel == null)
            {
                Debug.Log("Velocity cannot be null");
                return;
            }
            switch (direction)
            {
                case Direction.xPositive:
                    neighbour.xPositive.velocity = vel;
                    break;
                case Direction.xNegative:
                    neighbour.xNegative.velocity = vel;
                    break;
                case Direction.zPositive:
                    neighbour.zPositive.velocity = vel;
                    break;
                case Direction.zNegative:
                    neighbour.zNegative.velocity = vel;
                    break;
                default:
                    break;
            }
        }

        else if (dataType == WaterDataType.volume)
        {
            switch (direction)
            {
                case Direction.xPositive:
                    neighbour.xPositive.volume = vol;
                    break;
                case Direction.xNegative:
                    neighbour.xNegative.volume = vol;
                    break;
                case Direction.zPositive:
                    neighbour.zPositive.volume = vol;
                    break;
                case Direction.zNegative:
                    neighbour.zNegative.volume = vol;
                    break;
                default:
                    break;
            }
        }
    }

    public bool isInRange(Direction dir)
    {
        int xLength = WaterController.Current.waterCellArray.GetLength(0);
        int zLength = WaterController.Current.waterCellArray.GetLength(1);
        switch (dir)
        {
            case Direction.xPositive:
                if (position.x + 1 < xLength)
                    return true;
                return false;

            case Direction.xNegative:
                if (position.x - 1 >= 0)
                    return true;
                return false;

            case Direction.zPositive:
                if (position.z + 1 < zLength)
                    return true;
                return false;

            case Direction.zNegative:
                if (position.z - 1 >= 0)
                    return true;
                return false;

            default:
                return false;
        }
    }
}

public struct NeighbourWaterCell
{
    public WaterCell xPositive;
    public WaterCell xNegative;
    public WaterCell zPositive;
    public WaterCell zNegative;
}