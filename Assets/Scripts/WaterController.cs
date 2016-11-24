using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class WaterController : MonoBehaviour
{
    //Singleton reference
    public static WaterController Current;

    public WaterController()
    {
        Current = this;
    }

    public GameObject waterObject;
    public static int gridSizeX = 50;
    public static int gridSizeY = 50;
    public static int gridSizeZ = 100;

    /*    //Store velocity of each particle
    Vector3[,,] particleVelocityArray = new Vector3[gridSizeX, gridSizeY, gridSizeZ];
    //Store volume of each particle
    float[,,] particleDensityArray = new float[gridSizeX, gridSizeY, gridSizeZ];
    //Store the particles
    GameObject[,,] particleObjectArray = new GameObject[gridSizeX, gridSizeY, gridSizeZ];
    //Store whether space is empty or not
    bool[,,] isBlockEmptyArray = new bool[gridSizeX, gridSizeY, gridSizeZ];*/

    public WaterCell[,] waterCellArray = new WaterCell[gridSizeX, gridSizeY];

    //Store velocity of each particle
    //Vector3[,] particleVelocityArray = new Vector3[gridSizeX, gridSizeY];
    //Store volume of each particle
    //float[,] particleDensityArray = new float[gridSizeX, gridSizeY];
    //Store the particles
    //GameObject[,] particleObjectArray = new GameObject[gridSizeX, gridSizeY];
    //Store whether space is empty or not
    bool[,] isBlockEmptyArray = new bool[gridSizeX, gridSizeY];
    //Store references to water in other arrays
    List<Vector2> objectIndexList = new List<Vector2>();
    //Buffer so objectIndexList isn't edited while looping through it
    List<Vector2> objectIndexListBuffer = new List<Vector2>();

    float maxVolume = 100;
    float minVolume = 0.01f;
    float baseFlowRate = 0.1f;
    float timePassed = 0;
    float waterCounter = 1;

    void Start()
    {
        int id = 0;
        for (int i = 0; i < waterCellArray.GetLength(0); i++)
        {
            for (int j = 0; j < waterCellArray.GetLength(1); j++)
            {
                //Initialise all to 0 volume and 0 velocity
                waterCellArray[i, j] = new WaterCell(0, Vector3.zero, new Vector3(i, 1, j), id++);
            }
        }

        for (int i = 0; i < waterCellArray.GetLength(0); i++)
        {
            for (int j = 0; j < waterCellArray.GetLength(1); j++)
            {
                //Populate neighbours
                waterCellArray[i, j].populateNeighbourReferences();
            }
        }

        //Build block empty array before anything else
        BuildBlockEmptyArray();

        //Create starting block
        CreateParticle(new Vector2(1, 10), 10000);
        CreateParticle(new Vector2(10, 1), 10000);
    }

    void Update()
    {
        if (timePassed > 0)
        {
            UpdateSim();
            timePassed = 0;
        }
        timePassed += Time.deltaTime;
    }

    void CreateParticle(Vector2 position, int volume)
    {
        int x = (int)position.x;
        int y = (int)position.y;

        //Set volume
        waterCellArray[x, y].volume = volume;
        //Create gameobject
        waterCellArray[x, y].setGameObject((GameObject)Instantiate(waterObject, new Vector3(x, 1, y), Quaternion.identity));
        //Add to buffer to keep track of water
        objectIndexListBuffer.Add(new Vector2(x, y));
        waterCellArray[x, y].getGameObject().GetComponent<WaterInfo>().position = new Vector2(x, y);
    }

    void BuildBlockEmptyArray()
    {
        for (int i = 0; i < isBlockEmptyArray.GetLength(0); i++)
        {
            for (int j = 0; j < isBlockEmptyArray.GetLength(0); j++)
            {
                //Initialise all to being empty
                isBlockEmptyArray[i, j] = true;
                //Check if area has any colliders in
                var hitColliders = Physics.OverlapSphere(new Vector3(i, 1, j), 0.49f); //.OverlapBox(new Vector3(i, 1, j), new Vector3(0.49f, 0.49f, 0.49f));
                //If it hits a collider, block is not empty
                if (hitColliders.Length > 0)
                    isBlockEmptyArray[i, j] = false;
                //Border of array should be classed as solid
                if (i == 0 || i == isBlockEmptyArray.GetLength(0) || j == 0 || j == isBlockEmptyArray.GetLength(1))
                    isBlockEmptyArray[i, j] = false;
            }
        }
    }

    void UpdateSim()
    {
        //Add items from buffer
        objectIndexList.AddRange(objectIndexListBuffer);
        //Clear buffer
        objectIndexListBuffer.Clear();
        //Loop through stored object positions, saves us checking empty cells
        foreach (Vector2 position in objectIndexList)
        {
            int i = (int)position.x;
            int j = (int)position.y;

            //Set cells to resting or not
            //waterCellArray[i, j].isResting = CheckIfShouldRest(i, j);

            //If resting, don't do any comparisions //SHOULD IGNORE IF JUST CREATED PARTICLE IF NOT IT WILL NEVER SPAWN NEW ONES
            if (waterCellArray[i, j].isResting)
                return;

            //If this particle has water in it and is not at the lowest possible volume
            if (waterCellArray[i, j].volume > minVolume)
            {
                //Choose a random order to check the directions in, this prevents the water always going one way 
                List<Direction> poss = new List<Direction> { Direction.xNegative, Direction.xPositive, Direction.zNegative, Direction.zPositive };
                while (poss.Count >= 1)
                {
                    int randIndex = Random.Range(0, poss.Count);
                    Direction randDir = poss[randIndex];
                    poss.RemoveAt(randIndex);

                    CheckNeighbourVolume(i, j, randDir);
                }

                //Move water block
                float heightPos = Mathf.Clamp(waterCellArray[i, j].volume / 5, 0.01f, 1f);
                //Add the height of the terrain
                heightPos += 1;
                if (waterCellArray[i, j].volume >= minVolume)
                {
                    //Create new water object if one doesnt exist at new positions
                    if (waterCellArray[i, j].getGameObject() == null)
                    {
                        CreateParticle(new Vector2(i, j), 0);

                        //Debugging purposes
                        //particleObjectArray[i, j].name = "Water object " + waterCounter++;
                    }
                    //Else adjust object
                    else
                    {
                        waterCellArray[i, j].transformGameObject(new Vector3(i, heightPos, j));
                    }
                }
            }
        }
    }

    //Check neighbours
    void CheckNeighbourVolume(int i, int j, Direction direction)
    {
        //If block is not empty it cant move there, exit out of function
        //As border of array is all marked as not free, this should stop any comparisions out of the array bounds
        if (!isBlockEmptyArray[i, j] || !waterCellArray[i, j].isInRange(direction))
            return;

        float volume = waterCellArray[i, j].volume;
        float neighbourVolume = waterCellArray[i, j].getNeighbourData(direction).volume;

        //Set the previous volume to current volume
        waterCellArray[i, j].previousVolume = volume;

        //If neigbour volume is less then this volume
        float flowAmount = (volume - neighbourVolume) * baseFlowRate;
        //If flow amount is really small, ignore it
        if (flowAmount <= minVolume)
            return;

        //Stop doing this twice by ensuring this id appears before neighbour id, if not then calculations between these two cells have already been applied
        if (waterCellArray[i, j].id < waterCellArray[i, j].getNeighbourData(direction).id)
        {
            //If object has not yet been instantiated, add it to the index list so it will be
            if (waterCellArray[i, j].getNeighbourData(direction).previousVolume == -1)
                //Adds object to buffer
                objectIndexListBuffer.Add(new Vector2(i, j) + getVec2FromDirection(direction));

            //Update volume information
            waterCellArray[i, j].setNeighbourData(direction, WaterDataType.volume, neighbourVolume + flowAmount, Vector3.zero);
            waterCellArray[i, j].volume -= flowAmount;
        }

        //If min volume is greater than difference between volume this update and last, volume has not changed
        if(minVolume >= (waterCellArray[i, j].volume - waterCellArray[i, j].previousVolume))
        {
            waterCellArray[i, j].hasVolumeChanged = false;
        }
        else
        {
            waterCellArray[i, j].hasVolumeChanged = true;
        }
    }

    Vector2 getVec2FromDirection(Direction dir)
    {
        switch (dir)
        {
            case Direction.xPositive:
                return new Vector2(1, 0);
            case Direction.xNegative:
                return new Vector2(-1, 0);
            case Direction.zPositive:
                return new Vector2(0, 1);
            case Direction.zNegative:
                return new Vector2(0, -1);
            default:
                return Vector2.zero;
        }
    }

    bool CheckIfShouldRest(int thisX, int thisZ)
    {
        //Sets the water as true if all neighbours have not changed since last update
        if (!waterCellArray[thisX, thisZ].getNeighbourData().xPositive.hasVolumeChanged &&
            !waterCellArray[thisX, thisZ].getNeighbourData().xNegative.hasVolumeChanged &&
            !waterCellArray[thisX, thisZ].getNeighbourData().xPositive.hasVolumeChanged &&
            !waterCellArray[thisX, thisZ].getNeighbourData().zNegative.hasVolumeChanged)
            return true;   

        return false;
    }
}
