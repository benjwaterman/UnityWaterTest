using UnityEngine;
using System.Collections;

public enum Direction { xPositive, xNegative, zPositive, zNegative };
public enum WaterDataType { velocity, density };

public class Water
{
    //Stores the data for this water
    public WaterData data = new WaterData();
    //GameObject this data refers to
    GameObject waterGameObject;

    public Water()
    {
        data.density = 0;
        data.previousDensity = -1;
        data.hasDensityChanged = true;
        data.isResting = false;
        data.velocity = Vector3.zero;
        data.position = Vector3.zero;
    }

    public Water(float density, Vector3 velocity, Vector3 position)
    {
        data.density = density;
        data.velocity = velocity;
        data.position = position;

        //Default values
        data.previousDensity = -1;
        data.hasDensityChanged = true;
        data.isResting = false;
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
        data.position = position;
    }

    public WaterData getNeighbourData(Direction direction)
    {
        switch (direction)
        {
            case Direction.xPositive:
                if (!isInRange((int)data.position.x, direction))
                    return new WaterData(1000000);
                return WaterController.Current.waterBlockArray[(int)data.position.x + 1, (int)data.position.z].data;
            case Direction.xNegative:
                if (!isInRange((int)data.position.x, direction))
                    return new WaterData(1000000);
                return WaterController.Current.waterBlockArray[(int)data.position.x - 1, (int)data.position.z].data;
            case Direction.zPositive:
                if (!isInRange((int)data.position.z, direction))
                    return new WaterData(1000000);
                return WaterController.Current.waterBlockArray[(int)data.position.x, (int)data.position.z + 1].data;
            case Direction.zNegative:
                if (!isInRange((int)data.position.z, direction))
                    return new WaterData(1000000);
                return WaterController.Current.waterBlockArray[(int)data.position.x, (int)data.position.z - 1].data;
            default:
                return null;
        }
    }

    public void setNeighbourData(Direction direction, WaterDataType dataType, float dens, Vector3 vel)
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
                    WaterController.Current.waterBlockArray[(int)data.position.x + 1, (int)data.position.z].data.velocity = vel;
                    break;
                case Direction.xNegative:
                    WaterController.Current.waterBlockArray[(int)data.position.x - 1, (int)data.position.z].data.velocity = vel;
                    break;
                case Direction.zPositive:
                    WaterController.Current.waterBlockArray[(int)data.position.x, (int)data.position.z + 1].data.velocity = vel;
                    break;
                case Direction.zNegative:
                    WaterController.Current.waterBlockArray[(int)data.position.x, (int)data.position.z - 1].data.velocity = vel;
                    break;
                default:
                    break;
            }
        }

        else if (dataType == WaterDataType.density)
        {
            switch (direction)
            {
                case Direction.xPositive:
                    WaterController.Current.waterBlockArray[(int)data.position.x + 1, (int)data.position.z].data.density = dens;
                    break;
                case Direction.xNegative:
                    WaterController.Current.waterBlockArray[(int)data.position.x - 1, (int)data.position.z].data.density = dens;
                    break;
                case Direction.zPositive:
                    WaterController.Current.waterBlockArray[(int)data.position.x, (int)data.position.z + 1].data.density = dens;
                    break;
                case Direction.zNegative:
                    WaterController.Current.waterBlockArray[(int)data.position.x, (int)data.position.z - 1].data.density = dens;
                    break;
                default:
                    break;
            }
        }
    }

    bool isInRange(int position, Direction dir)
    {
        int xLength = WaterController.Current.waterBlockArray.GetLength(0);
        int zLength = WaterController.Current.waterBlockArray.GetLength(1);
        switch (dir)
        {
            case Direction.xPositive:
                if (position + 1 < xLength)
                    return true;
                return false;
            case Direction.xNegative:
                if (position - 1 >= 0)
                    return true;
                return false;
            case Direction.zPositive:
                if (position + 1 < zLength)
                    return true;
                return false;
            case Direction.zNegative:
                if (position - 1 >= 0)
                    return true;
                return false;
            default:
                return false;
        }
    }
}

public class WaterData
{
    public float density;
    public float previousDensity;
    public bool hasDensityChanged;
    public bool isResting;
    public Vector3 position;
    public Vector3 velocity;

    public WaterData()
    {
        density = 0;
        previousDensity = -1;
        hasDensityChanged = true;
        isResting = false;
        position = Vector3.zero;
        velocity = Vector3.zero;
    }

    public WaterData(float dens)
    {
        density = dens;
    }
}