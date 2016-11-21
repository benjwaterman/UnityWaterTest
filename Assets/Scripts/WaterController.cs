using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class WaterController : MonoBehaviour
{
    public GameObject waterObject;
    public static int gridSizeX = 100;
    public static int gridSizeY = 100;
    public static int gridSizeZ = 100;

    /*    //Store velocity of each particle
    Vector3[,,] particleVelocityArray = new Vector3[gridSizeX, gridSizeY, gridSizeZ];
    //Store density of each particle
    float[,,] particleDensityArray = new float[gridSizeX, gridSizeY, gridSizeZ];
    //Store the particles
    GameObject[,,] particleObjectArray = new GameObject[gridSizeX, gridSizeY, gridSizeZ];
    //Store whether space is empty or not
    bool[,,] isBlockEmptyArray = new bool[gridSizeX, gridSizeY, gridSizeZ];*/

    Water[,] waterBlockArray = new Water[gridSizeX, gridSizeY];

    //Store velocity of each particle
    //Vector3[,] particleVelocityArray = new Vector3[gridSizeX, gridSizeY];
    //Store density of each particle
    //float[,] particleDensityArray = new float[gridSizeX, gridSizeY];
    //Store the particles
    GameObject[,] particleObjectArray = new GameObject[gridSizeX, gridSizeY];
    //Store whether space is empty or not
    bool[,] isBlockEmptyArray = new bool[gridSizeX, gridSizeY];
    //Store references to water in other arrays
    List<Vector2> objectIndexList = new List<Vector2>();
    //Buffer so objectIndexList isn't edited while looping through it
    List<Vector2> objectIndexListBuffer = new List<Vector2>();
    
    float maxDensity = 100;
    float minDensity = 1;
    float baseFlowRate = 1;
    float timePassed = 0;
    float waterCounter = 1;

    void Start()
    {
        for (int i = 0; i < waterBlockArray.GetLength(0); i++)
        {
            for (int j = 0; j < waterBlockArray.GetLength(1); j++)
            {
                waterBlockArray[i, j] = new Water();
                waterBlockArray[i, j].velocity = Vector3.zero;
                waterBlockArray[i, j].density = 0;
            }
        }
        //Build block empty array before anything else
        BuildBlockEmptyArray();

        //Create starting block
        CreateParticle(new Vector2(0, 0), 100);
    }

    void Update()
    {
        if (timePassed > 1)
        {
            UpdateSim();
            timePassed = 0;
        }
        timePassed += Time.deltaTime;
    }

    void CreateParticle(Vector2 position, int startDensity)
    {
        int x = (int)position.x;
        int y = (int)position.y;

        waterBlockArray[x, y].density = startDensity;
        particleObjectArray[x, y] = (GameObject)Instantiate(waterObject, new Vector3(x, 0, y), Quaternion.identity);
        objectIndexList.Add(new Vector2(x, y));
    }

    void BuildBlockEmptyArray()
    {
        for(int i = 0; i < isBlockEmptyArray.GetLength(0); i++)
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

            waterBlockArray[i, j].isResting = CheckIfShouldRest(i, j);

            //If resting, don't do any comparisions //SHOULD IGNORE IF JUST CREATED PARTICLE IF NOT IT WILL NEVER SPAWN NEW ONES
            //if (waterBlockArray[i, j].isResting)
                //return;

            //If this particle has water in it and is not at the lowest possible density
            if (waterBlockArray[i, j].density > minDensity)
            {
                //Choose a random order to check the directions in, this prevents the water always going one way 
                List<int> poss = new List<int> { 0, 1, 2, 3 };
                while (poss.Count >= 1)
                {
                    int randIndex = Random.Range(0, poss.Count);
                    int randNum = poss[randIndex];
                    poss.RemoveAt(randIndex);

                    CheckNeighbourVelocity(i, j, randNum);
                }

                poss = new List<int> { 0, 1, 2, 3 };
                while (poss.Count >= 1)
                {
                    int randIndex = Random.Range(0, poss.Count);
                    int randNum = poss[randIndex];
                    poss.RemoveAt(randIndex);

                    CheckNeighbourDensity(i, j, randNum);
                }

                //Move water block
                float heightPos = Mathf.Clamp(waterBlockArray[i, j].density / 5, 0.01f, 1f);
                //Add the height of the terrain
                heightPos += 1;
                if (waterBlockArray[i, j].density >= minDensity)
                {
                    //Create new water object if one doesnt exist at new positions
                    if (particleObjectArray[i, j] == null)
                    {
                        particleObjectArray[i, j] = (GameObject)Instantiate(waterObject, new Vector3(i, heightPos, j), Quaternion.identity);// Quaternion.Euler(90, 0, 0));

                        //Debugging purposes
                        particleObjectArray[i, j].name = "Water object " + waterCounter++;
                    }
                    //Else adjust object
                    else
                    {
                        particleObjectArray[i, j].transform.position = new Vector3(i, heightPos, j);
                    }

                    //Update the density and velocity on the object
                    WaterInfo water = particleObjectArray[i, j].GetComponent<WaterInfo>();
                    water.density = waterBlockArray[i, j].density;
                    water.velocity = waterBlockArray[i, j].velocity;
                    water.isResting = waterBlockArray[i, j].isResting;
                    water.oldNeighbourDensity = waterBlockArray[i, j].oldNeighbourDensity;
                    water.currentNeighbourDensity = waterBlockArray[i, j].currentNeighbourDensity;
                    water.hasNeighbourChanged = waterBlockArray[i, j].hasNeighbourChanged;

                    //particleObjectArray[i, j].transform.position = new Vector3(i, heightPos, j);
                    //waterObjectList.Add((GameObject)Instantiate(waterObject, new Vector3(i, heightPos, j), Quaternion.identity));
                }
            }
        }
    }

    //Check neighbours
    void CheckNeighbourDensity(int i, int j, int neighbourIndex)
    {
        int thisX = i;
        int thisZ = j;
        int otherX = i;
        int otherZ = j;

        //If block is not empty it cant move there, exit out of function
        if (!isBlockEmptyArray[otherX, otherZ])
            return;

        float density = waterBlockArray[thisX, thisZ].density;

        switch (neighbourIndex)
        {
            case 0:
                otherX += 1;
                break;

            case 1:
                otherX -= 1;
                break;

            case 2:
                otherZ += 1;
                break;

            case 3:
                otherZ -= 1;
                break;

            default:
                Debug.Log("neigbour index out of range: " + neighbourIndex);
                break;
        }

        //Check within bounds of array, if not exit function
        if (!(otherX < waterBlockArray.GetLength(0) && otherX >= 0))
            return;
        if (!(otherZ < waterBlockArray.GetLength(1) && otherZ >= 0))
            return;

        //If neigbour density is less then this density
        if (density > waterBlockArray[otherX, otherZ].density)
        {
            MoveParticleTo(thisX, thisZ, otherX, otherZ);
        }

        //Set the old density
        waterBlockArray[thisX, thisZ].oldNeighbourDensity = waterBlockArray[thisX, thisZ].currentNeighbourDensity;
        //Set the current density
        waterBlockArray[thisX, thisZ].currentNeighbourDensity[neighbourIndex] = waterBlockArray[otherX, otherZ].density;

        /*

        //If old density == current density
        if (waterBlockArray[thisX, thisZ].oldNeighbourDensity[neighbourIndex] == waterBlockArray[otherX, otherZ].density)
        {
            //Density has changed since last update
            waterBlockArray[thisX, thisZ].hasNeighbourChanged[i] = true;
            //Set the old density
            waterBlockArray[thisX, thisZ].oldNeighbourDensity = waterBlockArray[thisX, thisZ].currentNeighbourDensity;
            //Set the current density
            waterBlockArray[thisX, thisZ].currentNeighbourDensity[i] = waterBlockArray[otherX, otherZ].density;
        }
        */
    }

    void CheckNeighbourVelocity(int i, int j, int neighbourIndex)
    {
        int thisX = i;
        int thisZ = j;
        int otherX = i;
        int otherZ = j;

        if (!isBlockEmptyArray[otherX, otherZ])
            return;

        Vector3 velocity = waterBlockArray[thisX, thisZ].velocity;

        switch (neighbourIndex)
        {
            case 0:
                otherX += 1;
                break;

            case 1:
                otherX -= 1;
                break;

            case 2:
                otherZ += 1;
                break;

            case 3:
                otherZ -= 1;
                break;

            default:
                Debug.Log("neigbour index out of range: " + neighbourIndex);
                break;
        }

        //Check within bounds of array, if not exit function
        if (!(otherX < waterBlockArray.GetLength(0) && otherX >= 0))
            return;
        if (!(otherZ < waterBlockArray.GetLength(1) && otherZ >= 0))
            return;

        //If this is checking the same direction as velocity
        if (velocity.x >= 1 && otherX - thisX >= 1)
        {
            //Continue going this way
            MoveParticleTo(thisX, thisZ, otherX, otherZ);
        }
    }

    bool CheckIfShouldRest(int thisX, int thisZ)
    {
        int i = 0;
        // If current neighbour density == old density, use i++ rather than another foreach
        foreach (float density in waterBlockArray[thisX, thisZ].currentNeighbourDensity)
        {
            //Debug.Log("Comparing " + density + " with " + waterBlockArray[thisX, thisZ].oldNeighbourDensity[i]);
            if ((int)density == (int)waterBlockArray[thisX, thisZ].oldNeighbourDensity[i])
            {
                waterBlockArray[thisX, thisZ].hasNeighbourChanged[i] = false;
                i++;
            }
            else
            {
                waterBlockArray[thisX, thisZ].hasNeighbourChanged[i] = true;
                i++;
            }
        }
        //Sets the water as true if all neighbours have not changed since last update
        return !waterBlockArray[thisX, thisZ].hasNeighbourChanged.Contains(true);
    }

    void MoveParticleTo(int thisX, int thisZ, int otherX, int otherZ)
    {
        //Add density to other particle
        waterBlockArray[otherX, otherZ].density += baseFlowRate;
        //Lower density of this particle
        waterBlockArray[thisX, thisZ].density -= baseFlowRate;
        //Set velocity based on direction
        waterBlockArray[thisX, thisZ].velocity = new Vector3(otherX - thisX, 0, otherZ - thisZ);

        //If object has not yet been instantiated, add it to the index list so it will be
        if (particleObjectArray[otherX, otherZ] == null)
            //Adds object to buffer
            objectIndexListBuffer.Add(new Vector2(otherX, otherZ));
    }
}
