using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Codes.Linus.IntVectors;

public enum Direction { xPositive, xNegative, zPositive, zNegative };

public class WaterCell
{
    //GameObject this data refers to
    GameObject waterGameObject;
    //Keep track of each id
    public int id;
    public float volume;
    public float previousVolume;
    public bool fDone; //Done flag
    public bool fActive; //Active flag
    public Vector2i position;
    public float cellHeight;
    public NeighbourWaterCells neighbours;

    public WaterCell()
    {
        volume = 0;
        previousVolume = -1;
        position = Vector2i.zero;
        fDone = false;
        fActive = false;
        cellHeight = -1;

        //Must manually populate neighbours references if using this method
    }

    public WaterCell(float volume, Vector2i position, int id)
    {
        this.volume = volume;
        this.position = position;
        this.id = id;

        //Default values
        previousVolume = -1;
        fDone = false;
        fActive = false;
    }

    public void populateNeighbourReferences()
    {
        //Check if array index is in range, if it is assign reference
        if (isInRange(Direction.xPositive))
            neighbours.xPositive = WaterController.Current.waterCellArray[position.x + 1, position.y];

        if (isInRange(Direction.xNegative))
            neighbours.xNegative= WaterController.Current.waterCellArray[position.x - 1, position.y];

        if (isInRange(Direction.zPositive))
            neighbours.zPositive = WaterController.Current.waterCellArray[position.x, position.y + 1];

        if (isInRange(Direction.zNegative))
            neighbours.zNegative = WaterController.Current.waterCellArray[position.x, position.y - 1];
    }

    public void setGameObject(GameObject go)
    {
        waterGameObject = go;
        //Adjust the game object height to whatever it is currently set to (should be -1) when this function is called
        waterGameObject.transform.position = new Vector3(waterGameObject.transform.position.x, cellHeight, waterGameObject.transform.position.z);
    }

    public GameObject getGameObject()
    {
        return waterGameObject;
    }

    public void transformGameObject(Vector2i position)
    {
        waterGameObject.transform.position = new Vector3(position.x, cellHeight, position.y);
        this.position = position;
    }

    public void setCellHeight()
    {
        cellHeight = volume;
        //Adjust height of cell
        waterGameObject.transform.position = new Vector3(position.x, cellHeight, position.y);
    }

    public WaterCell getNeighbourData(Direction direction)
    {
        switch (direction)
        {
            case Direction.xPositive:
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
                if (position.y + 1 < zLength)
                    return true;
                return false;

            case Direction.zNegative:
                if (position.y - 1 >= 0)
                    return true;
                return false;

            default:
                Debug.Log("Default case in: isInRange(). This shouldn't happen.");
                return false;
        }
    }
}

public struct NeighbourWaterCells
{
    public WaterCell xPositive;
    public WaterCell xNegative;
    public WaterCell zPositive;
    public WaterCell zNegative;
}