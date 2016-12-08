using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Codes.Linus.IntVectors; //Int vectors from https://github.com/LinusVanElswijk/Unity-Int-Vectors

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

    public WaterCell[,] waterCellArray = new WaterCell[gridSizeX, gridSizeY];

    //Store whether space is empty or not
    int[,] worldHeightArray = new int[gridSizeX, gridSizeY];
    //Store references to water in other arrays
    List<Vector2> objectIndexList = new List<Vector2>();
    //Buffer so objectIndexList isn't edited while looping through it
    List<Vector2> objectIndexListBuffer = new List<Vector2>();

    //Vec2i references to water cells
    //Potentially can make this static array and have a stop value, eg when value is negative, ignore teh rest of the array
    List<Vector2i> activeCellIndexListA = new List<Vector2i>();
    //Have 2 as 1 acts as a buffer
    List<Vector2i> activeCellIndexListB = new List<Vector2i>();

    float maxVolume = 1;
    float minVolume = 0.005f;
    float baseFlowRate = 0.1f;
    float timePassed = 0;
    float waterCounter = 1;

    Direction[] neighboursToCompare = { Direction.xPositive, Direction.xNegative, Direction.zPositive, Direction.zNegative };

    void Start()
    {
        //Initialise cell array
        InitialiseCellArray();

        //Build world height array before anything else
        BuildWorldHeightArray();

        /*string toWrite = "";
        foreach (var item in worldHeightArray)
        {
            toWrite += " " + item.ToString();
        }
        Debug.Log(toWrite); */

        //Create starting cell
        waterCellArray[1, 1].volume = 100;
    }

    void Update()
    {
        if (timePassed > 0.0)
        {
            //UpdateSim();
            UpdateCells();
            timePassed = 0;
        }
        timePassed += Time.deltaTime;
    }

    void InitialiseCellArray()
    {
        int id = 0;
        for (int i = 0; i < waterCellArray.GetLength(0); i++)
        {
            for (int j = 0; j < waterCellArray.GetLength(1); j++)
            {
                //Iniitialise cell array at current position
                waterCellArray[i, j] = new WaterCell(0, new Vector2i(i, j), id++);
                //Create cell at position with -1 volume
                CreateCell(new Vector2i(i, j), 0);
                //Add to active cell index
                activeCellIndexListA.Add(new Vector2i(i, j));
            }
        }

        //Have to do this after all cells have been initialised
        for (int i = 0; i < waterCellArray.GetLength(0); i++)
        {
            for (int j = 0; j < waterCellArray.GetLength(1); j++)
            {
                //Populate neighbours
                waterCellArray[i, j].populateNeighbourReferences();
            }
        }
    }

    void CreateCell(Vector2i position, int volume)
    {
        int x = position.x;
        int y = position.y;

        //Set volume
        waterCellArray[x, y].volume = volume;

        //Create gameobject and assign it
        waterCellArray[x, y].setGameObject((GameObject)Instantiate(waterObject, new Vector3(x, -1, y), Quaternion.identity));
        //Add to buffer to keep track of water
        //objectIndexListBuffer.Add(new Vector2(x, y));
        waterCellArray[x, y].getGameObject().GetComponent<WaterInfo>().position = new Vector2i(x, y);
    }

    void BuildWorldHeightArray()
    {
        for (int i = 0; i < worldHeightArray.GetLength(0); i++)
        {
            for (int j = 0; j < worldHeightArray.GetLength(1); j++)
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

    void UpdateCells()
    {
        //For each cell in cell index list A
        foreach (Vector2i index in activeCellIndexListA)
        {
            //Compare against all neighbours
            foreach (Direction dir in neighboursToCompare)
            {
                //If neighbour exists
                if (waterCellArray[index.x, index.y].isInRange(dir))
                {
                    //If neighbour is not done
                    if (!waterCellArray[index.x, index.y].getNeighbourData(dir).fDone)
                    {
                        //If difference between volumes is greater than min amount
                        if (Mathf.Abs(waterCellArray[index.x, index.y].volume - waterCellArray[index.x, index.y].getNeighbourData(dir).volume) > minVolume)
                        {
                            //Adjust this volume
                            waterCellArray[index.x, index.y].volume -= (waterCellArray[index.x, index.y].volume - waterCellArray[index.x, index.y].getNeighbourData(dir).volume) * baseFlowRate;
                            //Adjust neighbour volume
                            waterCellArray[index.x, index.y].getNeighbourData(dir).volume += (waterCellArray[index.x, index.y].volume - waterCellArray[index.x, index.y].getNeighbourData(dir).volume) * baseFlowRate;

                            //If neighbour is not active
                            if (!waterCellArray[index.x, index.y].getNeighbourData(dir).fActive)
                            {
                                waterCellArray[index.x, index.y].getNeighbourData(dir).fActive = true;
                                activeCellIndexListB.Add(waterCellArray[index.x, index.y].getNeighbourData(dir).position);
                            }
                            //If this cell is not active
                            if (!waterCellArray[index.x, index.y].fActive)
                            {
                                waterCellArray[index.x, index.y].fActive = true;
                                activeCellIndexListB.Add(waterCellArray[index.x, index.y].position);
                            }
                        }

                    }
                }
            }
            //Set this cell to done
            waterCellArray[index.x, index.y].fDone = true;
        }

        //For cells in list B
        foreach (Vector2i index in activeCellIndexListB)
        {
            //Reset flags
            waterCellArray[index.x, index.y].fActive = false;
            waterCellArray[index.x, index.y].fDone = false;

            waterCellArray[index.x, index.y].setCellHeight(waterCellArray[index.x, index.y].volume);
        }

        //Flip lists, it is done like this to prevent copying references and do a deep copy instead
        List<Vector2i> tempList = new List<Vector2i>();
        //A assigned to temp list
        tempList.AddRange(activeCellIndexListA);
        //B assigned to A
        activeCellIndexListA.Clear();
        activeCellIndexListA.AddRange(activeCellIndexListB);
        //Clear list B
        activeCellIndexListB.Clear();
        //activeCellIndexListB.AddRange(tempList);
    }

    void UpdateSim()
    {
        //Add items from buffer
        objectIndexList.AddRange(objectIndexListBuffer);
        //Clear buffer
        objectIndexListBuffer.Clear();


        /*
        
        A is a reference to a list of vec2i or refs to objects
        B is a reference to a list  //rather than a list could statically sized array of vec2i, and stop spinning when you find one with negative values., and set the first value niegatie to "clear)
        // trying to minimize (eliminate allocations) - set PoD values on objects rather than create objects
        
        clear B
        for each active (A)//calcFlow
        {
            for each neighbour:
            {
                if they are not done:
                {
                    calc flow with N according to previousVolume
                    if flow > threshold
                    {
                        adjust their volume according to flowVolume
                        adjust our volume according to flowVolume

                        // if they are not active, then:
                        {
                            add them activeList (B) 
                            set activeFlag on them
                        }

                         // if we are not active, then:
                        {
                            add us to activeList (B) 
                            set activeFlag on us
                        }
                    }
                }
            }
            set our DONE flag
            previousVolume = volume (safe because WE are done)
        }        
        
        
        for each active (B)
        {
            //reset all done flags for B
            //reset all active flags for B
            
            //set renderable thing to the right height...
            
        }
        flip activeLists (A becomes B, and B comes A) (probably need a C temporary
        
        
        //for debug you might want to check for repetitions (before the stop mark) in A or in B.
        */



        //Loop through stored object positions, saves us checking empty cells
        foreach (Vector2 position in objectIndexList) // use vector2i - https://github.com/LinusVanElswijk/Unity-Int-Vectors
        {
            int i = (int)position.x;
            int j = (int)position.y;

            //If volume difference is less than minVolume, water has not changed
            if (minVolume >= Mathf.Abs(waterCellArray[i, j].volume - waterCellArray[i, j].previousVolume)) //split into a variable (flowVolume) and the comparison
            {
                waterCellArray[i, j].hasVolumeChanged = false; //remove, just use an "active list"
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
                for (int k = 0; k < 4; k++)
                {
                    Direction direction;
                    switch (k)
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
                int heightMod = (int)waterCellArray[i, j].volume; //Mathf.Clamp(waterCellArray[i, j].volume / 5, 0.01f, 1f);
                //Instead of the whole cell sitting on top of the terrain, only a small part of it will
                //heightMod -= 0.8f;
                if (waterCellArray[i, j].volume >= minVolume)
                {
                    //Create new water object if one doesnt exist at new positions
                    if (waterCellArray[i, j].getGameObject() == null)
                    {
                        CreateCell(new Vector2i(i, j), 0);

                        //Debugging purposes
                        //particleObjectArray[i, j].name = "Water object " + waterCounter++;
                    }
                    //Else adjust object
                    else
                    {
                        int numFullCells = (int)(waterCellArray[i, j].volume / maxVolume);
                        waterCellArray[i, j].transformGameObject(new Vector2i(i, j));
                    }
                }
            }
        }
    }

    //Check neighbours
    // INLINE ME - https://msdn.microsoft.com/en-us/library/system.runtime.compilerservices.methodimploptions(v=VS.110).aspx
    //Cant inline, dont think unity has correct .NET version
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
            if ((int)(volume / maxVolume) < worldHeightArray[otherX, otherZ])  //don't worry about max volume
                return;

            //If neigbour volume is less then this volume
            float flowAmount = (volume - neighbourVolume) * baseFlowRate; //??could move to a non-linear relationship?? 
            //If flow amount is really small (less than min volume), ignore it
            if (Mathf.Abs(flowAmount) <= minVolume)
                return;

            //If object has not yet been instantiated, add it to the index list so it will be
            if (waterCellArray[i, j].getNeighbourData(direction).previousVolume == -1)
                //Adds object to buffer
                objectIndexListBuffer.Add(new Vector2(i, j) + getVec2FromDirection(direction));

            //Update volume information
            waterCellArray[i, j].getNeighbourData(direction).volume += flowAmount;
            waterCellArray[i, j].volume -= flowAmount;

            //This cell has been compared with other cell
            waterCellArray[i, j].setComparedWith(getVec2FromDirection(direction));
            //Other cell has been compared with this cell
            if (!waterCellArray[i, j].isInRange(direction))
                waterCellArray[otherX, otherZ].setComparedWith(i, j);
        }
    }

    Vector2i getVec2FromDirection(Direction dir)
    {
        switch (dir)
        {
            case Direction.xPositive:
                return new Vector2i(1, 0);
            case Direction.xNegative:
                return new Vector2i(-1, 0);
            case Direction.zPositive:
                return new Vector2i(0, 1);
            case Direction.zNegative:
                return new Vector2i(0, -1);
            default:
                return Vector2i.zero;
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
