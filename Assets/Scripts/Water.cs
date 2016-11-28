using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
    public NeighbourWaterCells neighbours;

    //Store list of what has been compared, to stop comparing the same 2 cells twice
    List<Vector2> comparisonsList = new List<Vector2>();

    public WaterCell()
    {
        volume = 0;
        previousVolume = -1;
        hasVolumeChanged = true;
        isResting = false;
        velocity = Vector3.zero;
        position = Vector3.zero;

        //Must manually populate neighbours references if using this method
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
            neighbours.xPositive = WaterController.Current.waterCellArray[(int)position.x + 1, (int)position.z];

        if (isInRange(Direction.xNegative))
            neighbours.xNegative= WaterController.Current.waterCellArray[(int)position.x - 1, (int)position.z];

        if (isInRange(Direction.zPositive))
            neighbours.zPositive = WaterController.Current.waterCellArray[(int)position.x, (int)position.z + 1];

        if (isInRange(Direction.zNegative))
            neighbours.zNegative = WaterController.Current.waterCellArray[(int)position.x, (int)position.z - 1];
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
                if (neighbours.xPositive == null)
                {
                    int i = 0;
                }
                return neighbours.xPositive;
            case Direction.xNegative:
                return neighbours.xNegative;
            case Direction.zPositive:
                return neighbours.zPositive;
            case Direction.zNegative:
                return neighbours.zNegative;
            default:
                return null;
        }
    }

    public NeighbourWaterCells getNeighbourData()
    {
        return neighbours;
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
                    neighbours.xPositive.velocity = vel;
                    break;
                case Direction.xNegative:
                    neighbours.xNegative.velocity = vel;
                    break;
                case Direction.zPositive:
                    neighbours.zPositive.velocity = vel;
                    break;
                case Direction.zNegative:
                    neighbours.zNegative.velocity = vel;
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
                    neighbours.xPositive.volume = vol;
                    break;
                case Direction.xNegative:
                    neighbours.xNegative.volume = vol;
                    break;
                case Direction.zPositive:
                    neighbours.zPositive.volume = vol;
                    break;
                case Direction.zNegative:
                    neighbours.zNegative.volume = vol;
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

    //Set this block has been compared with x, z
    public void setComparedWith(int x, int z)
    {
        comparisonsList.Add(new Vector2(x, z));
    }
    public void setComparedWith(Vector2 cellPosition)
    {
        comparisonsList.Add(cellPosition);
    }

    //Check if this block has been compared with x, z
    public bool hasComparedWith(int x, int z)
    {
        if(comparisonsList.Contains(new Vector2((int) x, (int) z)))
        {
            return true;
        }

        return false;
    }
    public bool hasComparedWith(Vector2 cellPosition)
    {
        if (comparisonsList.Contains(cellPosition))
        {
            return true;
        }

        return false;
    }

    //Clear what has been compared, should happen start of each update
    public void clearComparisons()
    {
        comparisonsList.Clear();
    }
}

public struct NeighbourWaterCells
{
    public WaterCell xPositive;
    public WaterCell xNegative;
    public WaterCell zPositive;
    public WaterCell zNegative;
}