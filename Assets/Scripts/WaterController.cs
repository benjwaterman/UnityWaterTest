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
    int[,] worldHeightArray = new int[gridSizeX, gridSizeY];
    //Store references to water in other arrays
    List<Vector2> objectIndexList = new List<Vector2>();
    //Buffer so objectIndexList isn't edited while looping through it
    List<Vector2> objectIndexListBuffer = new List<Vector2>();

    float maxVolume = 1;
    float minVolume = 0.005f;
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
        //BuildBlockEmptyArray();
        BuildWorldHeightArray();

        string toWrite = "";
        foreach (var item in worldHeightArray)
        {
            toWrite += " " + item.ToString();
        }
        //Debug.Log(toWrite);

        //Create starting block
        CreateParticle(new Vector2(25, 25), 100000);
        //CreateParticle(new Vector2(20, 20), 10000);
    }

    void Update()
    {
        if (timePassed > 0.1)
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
        waterCellArray[x, y].setGameObject((GameObject)Instantiate(waterObject, new Vector3(x, -1, y), Quaternion.identity));
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

    void BuildWorldHeightArray()
    {
        for (int i = 0; i < worldHeightArray.GetLength(0); i++)
        {
            for (int j = 0; j < worldHeightArray.GetLength(0); j++)
            {
                //Initialise all to being empty
                worldHeightArray[i, j] = 0;
                //Store height we're currently checking
                int height = 0;
                //Check if area has any colliders in
                var hitColliders = Physics.OverlapSphere(new Vector3(i, height, j), 0.49f);
                //While there is something in this spot, keep searching upwards until there is a space
                while (hitColliders.Length > 0)
                {
                    height++;
                    hitColliders = Physics.OverlapSphere(new Vector3(i, height, j), 0.49f);   
                }
                //Store the height we got to 
                worldHeightArray[i, j] = height;
                //Border of array should be classed as solid, make it the highest possible value
                if (i == 0 || i == worldHeightArray.GetLength(0) || j == 0 || j == worldHeightArray.GetLength(1))
                    worldHeightArray[i, j] = int.MaxValue;
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

            //If volume difference is less than minVolume, water has not changed
            if (minVolume >= Mathf.Abs(waterCellArray[i, j].volume - waterCellArray[i, j].previousVolume))
            {
                waterCellArray[i, j].hasVolumeChanged = false;
            }
            else
            {
                waterCellArray[i, j].hasVolumeChanged = true;
                //Set all neighbours to not resting
                //waterCellArray[i, j].neighbours.xPositive.isResting = false;
                //waterCellArray[i, j].neighbours.xNegative.isResting = false;
                //waterCellArray[i, j].neighbours.zPositive.isResting = false;
                //waterCellArray[i, j].neighbours.zNegative.isResting = false;
            }
            //Set the previous volume to current volume
            waterCellArray[i, j].previousVolume = waterCellArray[i, j].volume;

            //Set cells to resting or not
            //waterCellArray[i, j].isResting = CheckIfShouldRest(i, j);

            //If resting, don't do any comparisions //SHOULD IGNORE IF JUST CREATED PARTICLE IF NOT IT WILL NEVER SPAWN NEW ONES
            if (waterCellArray[i, j].isResting)
                return;

            //Clear comparisons from last update
            waterCellArray[i, j].clearComparisons();

            //If this particle has water in it and is not at the lowest possible volume
            if (waterCellArray[i, j].volume >= minVolume)
            {
                //Choose a random order to check the directions in, this prevents the water always going one way 
                //List<Direction> poss = new List<Direction> { Direction.xNegative, Direction.xPositive, Direction.zNegative, Direction.zPositive };
                //while (poss.Count >= 1)
                //{
                //    int randIndex = Random.Range(0, poss.Count);
                //    Direction randDir = poss[randIndex];
                //    poss.RemoveAt(randIndex);

                //    CheckNeighbourVolume(i, j, randDir);
                //}

                for(int k = 0; k < 4; k++)
                {
                    Direction direction;
                    switch(k)
                    {
                        case 0:
                            direction = Direction.xPositive;
                            break;

                        case 1:
                            direction = Direction.xNegative;
                            break;

                        case 2:
                            direction = Direction.zPositive;
                            break;

                        case 3:
                            direction = Direction.zNegative;
                            break;

                        default:
                            direction = Direction.xPositive;
                            Debug.Log("Default case in switch statement, this shouldn't happen");
                            break;
                    }
                    CheckNeighbourVolume(i, j, direction);
                }

                //Move water block
                float heightMod = waterCellArray[i, j].volume / maxVolume; //Mathf.Clamp(waterCellArray[i, j].volume / 5, 0.01f, 1f);
                //Instead of the whole cell sitting on top of the terrain, only a small part of it will
                heightMod -= 0.8f;
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
                        int numFullCells = (int) (waterCellArray[i, j].volume / maxVolume);
                        waterCellArray[i, j].transformGameObject(new Vector3(i, heightMod, j));
                    }
                }
            }
        }
    }

    //Check neighbours
    void CheckNeighbourVolume(int i, int j, Direction direction)
    {
        //If block is not empty it cant move there, exit out of function
        //As the border of the array are all marked as not free, this should stop any comparisions out of the array bounds
        if (!waterCellArray[i, j].isInRange(direction))
            return;

        //If this cell has NOT been compared with the other cell this update
        if (!waterCellArray[i, j].hasComparedWith(getVec2FromDirection(direction)))
        {
            float volume = waterCellArray[i, j].volume;
            float neighbourVolume = waterCellArray[i, j].getNeighbourData(direction).volume;
            int otherX = i + (int)getVec2FromDirection(direction).x;
            int otherZ = j + (int)getVec2FromDirection(direction).y;

            //CHECK IF NEIGHBOUR IS LOWER ELEVATION, IF IT IS ALWAYS MOVE THERE AND MAYBE MOVE A LARGER AMOUNT DEPENDING ON DELTA HEIGHT
            //ALSO NEED TO CHANGE IT TO IF ON TOP OF HEIGHT DO NOT INCLUDE TERRAIN HEIGHT IN DENSITY

            //If there is less water than is capable of reaching a certain height, exit
            if ((int) (volume / maxVolume) < worldHeightArray[otherX, otherZ])
                return;

            //If neigbour volume is less then this volume
            float flowAmount = (volume - neighbourVolume) * baseFlowRate;
            //If flow amount is really small (less than min volume), ignore it
            if (Mathf.Abs(flowAmount) <= minVolume)
                return;

            //If object has not yet been instantiated, add it to the index list so it will be
            if (waterCellArray[i, j].getNeighbourData(direction).previousVolume == -1)
                //Adds object to buffer
                objectIndexListBuffer.Add(new Vector2(i, j) + getVec2FromDirection(direction));

            //Update volume information
            waterCellArray[i, j].setNeighbourData(direction, WaterDataType.volume, neighbourVolume + flowAmount, Vector3.zero);
            waterCellArray[i, j].volume -= flowAmount;

            //This cell has been compared with other cell
            waterCellArray[i, j].setComparedWith(getVec2FromDirection(direction));
            //Other cell has been compared with this cell
            if (!waterCellArray[i, j].isInRange(direction))
                waterCellArray[otherX, otherZ].setComparedWith(i, j);
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
        NeighbourWaterCells neighbourCells = waterCellArray[thisX, thisZ].getNeighbourData();

        //If any neighbour doesn't exist return false
        if (neighbourCells.xPositive == null ||
            neighbourCells.xNegative == null ||
            neighbourCells.xPositive == null ||
            neighbourCells.zNegative == null)
            return false;

        //If neighbours have volume of -1, they have not yet been created
        if (neighbourCells.xPositive.volume == -1 ||
            neighbourCells.xNegative.volume == -1 ||
            neighbourCells.xPositive.volume == -1 ||
            neighbourCells.zNegative.volume == -1)
            return false;

        //Sets the water as true if all neighbours have not changed since last update
        if (!neighbourCells.xPositive.hasVolumeChanged &&
            !neighbourCells.xNegative.hasVolumeChanged &&
            !neighbourCells.xPositive.hasVolumeChanged &&
            !neighbourCells.zNegative.hasVolumeChanged)
            return true;

        return false;
    }
}
