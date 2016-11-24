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

    float maxDensity = 100;
    float minDensity = 0;
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
        CreateParticle(new Vector2(3, 3), 10000);
    }

    void Update()
    {
        if (timePassed > 0.5)
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
        //Add to keep track
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

            //waterCellArray[i, j].data.isResting = CheckIfShouldRest(i, j);

            //If resting, don't do any comparisions //SHOULD IGNORE IF JUST CREATED PARTICLE IF NOT IT WILL NEVER SPAWN NEW ONES
            if (waterCellArray[i, j].isResting)
            return;

            //If this particle has water in it and is not at the lowest possible volume
            if (waterCellArray[i, j].volume > minDensity) //NEED TO MAKE THIS DIFFERENCE BETWEEN 2 BLOCKS SO THEY ARENT ALWAYS TRYING TO FLOW WATER WHEN IT IS LIKE 0.1^10 THANKS BEN YOU BEAUITIFUL MAN
            {
                //Choose a random order to check the directions in, this prevents the water always going one way 
                //List<Direction> poss = new List<Direction> { Direction.xNegative, Direction.xPositive, Direction.zNegative, Direction.zPositive };
                //while (poss.Count >= 1)
                //{
                //    int randIndex = Random.Range(0, poss.Count);
                //    Direction randDir = poss[randIndex];
                //    poss.RemoveAt(randIndex);

                //    //CheckNeighbourVelocity(i, j, randDir);
                //}

                List<Direction> poss = new List<Direction> { Direction.xNegative, Direction.xPositive, Direction.zNegative, Direction.zPositive };
                while (poss.Count >= 1)
                {
                    int randIndex = Random.Range(0, poss.Count);
                    Direction randDir = poss[randIndex];
                    poss.RemoveAt(randIndex);

                    CheckNeighbourDensity(i, j, randDir);
                }

                //Move water block
                float heightPos = Mathf.Clamp(waterCellArray[i, j].volume / 5, 0.01f, 1f);
                //Add the height of the terrain
                heightPos += 1;
                if (waterCellArray[i, j].volume >= minDensity)
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
    void CheckNeighbourDensity(int i, int j, Direction direction)
    {
        //If block is not empty it cant move there, exit out of function
        //As border of array is all marked as not free, this should stop any comparisions out of the array bounds
        if (!isBlockEmptyArray[i, j] && !waterCellArray[i, j].isInRange(direction))
            return;

        float volume = waterCellArray[i, j].volume;
        float neighbourVolume = waterCellArray[i, j].getNeighbourData(direction).volume;

        //Set the previous volume to current volume
        waterCellArray[i, j].previousVolume = volume;

        //If neigbour volume is less then this volume
        float flowAmount = (volume - neighbourVolume) * baseFlowRate;
        //Stop doing this twice by ensuring this id appears before neighbour id, if not then calculations between these two cells have already been applied
        if (waterCellArray[i, j].id < waterCellArray[i, j].getNeighbourData(direction).id)
        {
            //If object has not yet been instantiated, add it to the index list so it will be
            if (waterCellArray[i, j].getNeighbourData(direction).previousVolume == -1)
                //Adds object to buffer
                objectIndexListBuffer.Add(new Vector2(i, j) + getVec2FromDirection(direction));

            waterCellArray[i, j].setNeighbourData(direction, WaterDataType.volume, neighbourVolume + flowAmount, Vector3.zero);
            waterCellArray[i, j].volume -= flowAmount;
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

    //void CheckNeighbourVelocity(int i, int j, Direction direction)
    //{
    //    int thisX = i;
    //    int thisZ = j;
    //    int otherX = i;
    //    int otherZ = j;

    //    if (!isBlockEmptyArray[otherX, otherZ])
    //        return;

    //    Vector3 velocity = waterCellArray[thisX, thisZ].velocity;

    //    switch (neighbourIndex)
    //    {
    //        case 0:
    //            otherX += 1;
    //            break;

    //        case 1:
    //            otherX -= 1;
    //            break;

    //        case 2:
    //            otherZ += 1;
    //            break;

    //        case 3:
    //            otherZ -= 1;
    //            break;

    //        default:
    //            Debug.Log("neigbour index out of range: " + neighbourIndex);
    //            break;
    //    }

    //    //Check within bounds of array, if not exit function
    //    if (!(otherX < waterCellArray.GetLength(0) && otherX >= 0))
    //        return;
    //    if (!(otherZ < waterCellArray.GetLength(1) && otherZ >= 0))
    //        return;

    //    //If this is checking the same direction as velocity
    //    if (velocity.x >= 1 && otherX - thisX >= 1)
    //    {
    //        //Continue going this way
    //        MoveParticleTo(thisX, thisZ, otherX, otherZ);
    //    }
    //}

    //bool CheckIfShouldRest(int thisX, int thisZ)
    //{
    //    for (int i = 0; i < waterCellArray.; i++)
    //    {
    //        for (int j = 0; j < waterCellArray.GetLength(1); j++)
    //        {
    //            waterCellArray
    //            //Debug.Log("Comparing " + volume + " with " + waterCellArray[thisX, thisZ].oldNeighbourDensity[i]);
    //            if (volume == waterCellArray[thisX, thisZ].oldNeighbourDensity[i])
    //            {
    //                waterCellArray[thisX, thisZ].hasNeighbourChanged[i] = false;
    //                i++;
    //            }
    //            else
    //            {
    //                waterCellArray[thisX, thisZ].hasNeighbourChanged[i] = true;
    //                i++;
    //            }
    //        }
    //    }
    //    //Sets the water as true if all neighbours have not changed since last update
    //    return !waterCellArray[thisX, thisZ].hasNeighbourChanged.Contains(true);
    //}

    //void MoveParticleTo(int thisX, int thisZ, int otherX, int otherZ)
    //{
    //    //Add volume to other particle
    //    waterCellArray[otherX, otherZ].volume += baseFlowRate;
    //    //Lower volume of this particle
    //    waterCellArray[thisX, thisZ].volume -= baseFlowRate;
    //    //Set velocity based on direction
    //    waterCellArray[thisX, thisZ].velocity = new Vector3(otherX - thisX, 0, otherZ - thisZ);

    //    //If object has not yet been instantiated, add it to the index list so it will be
    //    if (particleObjectArray[otherX, otherZ] == null)
    //        //Adds object to buffer
    //        objectIndexListBuffer.Add(new Vector2(otherX, otherZ));
    //}
}
