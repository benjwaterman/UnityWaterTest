using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaterController : MonoBehaviour
{
    public GameObject waterObject;
    public static int gridSizeX = 100;
    public static int gridSizeY = 100;

    //Store velocity of each particle
    Vector3[,] particleVelocityArray = new Vector3[gridSizeX, gridSizeY];
    //Store density of each particle
    float[,] particleDensityArray = new float[gridSizeX, gridSizeY];
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
        for (int i = 0; i < particleVelocityArray.GetLength(0); i++)
        {
            for (int j = 0; j < particleVelocityArray.GetLength(1); j++)
            {
                particleVelocityArray[i, j] = Vector3.zero;
                particleDensityArray[i, j] = 0;
            }
        }
        //Build block empty array before anything else
        BuildBlockEmptyArray();

        //Create starting block
        CreateParticle(new Vector2(0, 0), 10000);
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

    void CreateParticle(Vector2 position, int startDensity)
    {
        int x = (int)position.x;
        int y = (int)position.y;

        particleDensityArray[x, y] = startDensity;
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

            //If this particle has water in it and is not at the lowest possible density
            if (particleDensityArray[i, j] > minDensity)
            {
                //Choose a random order to check the directions in, this prevents the water always going one way 
                List<int> poss = new List<int> { 1, 2, 3, 4 };
                while (poss.Count >= 1)
                {
                    int randIndex = Random.Range(0, poss.Count);
                    int randNum = poss[randIndex];
                    poss.RemoveAt(randIndex);

                    //CheckNeighbourVelocity(i, j, randNum);
                }

                poss = new List<int> { 1, 2, 3, 4 };
                while (poss.Count >= 1)
                {
                    int randIndex = Random.Range(0, poss.Count);
                    int randNum = poss[randIndex];
                    poss.RemoveAt(randIndex);

                    CheckNeighbourDensity(i, j, randNum);
                }

                //Move water block
                float heightPos = Mathf.Clamp(particleDensityArray[i, j] / 5, 0.01f, 1f);
                //Add the height of the terrain
                heightPos += 1;
                if (particleDensityArray[i, j] >= minDensity)
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
                    Water water = particleObjectArray[i, j].GetComponent<Water>();
                    water.density = particleDensityArray[i, j];
                    water.velocity = particleVelocityArray[i, j];

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

        float density = particleDensityArray[thisX, thisZ];

        switch (neighbourIndex)
        {
            case 1:
                otherX += 1;
                break;

            case 2:
                otherX -= 1;
                break;

            case 3:
                otherZ += 1;
                break;

            case 4:
                otherZ -= 1;
                break;

            default:
                break;
        }

        //Check within bounds of array, if not exit function
        if (!(otherX < particleDensityArray.GetLength(0) && otherX >= 0))
            return;
        if (!(otherZ < particleDensityArray.GetLength(1) && otherZ >= 0))
            return;

        //If neigbour density is less then this density
        if (density > particleDensityArray[otherX, otherZ])
        {
            MoveParticleTo(thisX, thisZ, otherX, otherZ);
        }
    }

    void CheckNeighbourVelocity(int i, int j, int neighbourIndex)
    {
        int thisX = i;
        int thisZ = j;
        int otherX = i;
        int otherZ = j;

        if (!isBlockEmptyArray[otherX, otherZ])
            return;

        Vector3 velocity = particleVelocityArray[thisX, thisZ];

        switch (neighbourIndex)
        {
            case 1:
                otherX += 1;
                break;

            case 2:
                otherX -= 1;
                break;

            case 3:
                otherZ += 1;
                break;

            case 4:
                otherZ -= 1;
                break;

            default:
                break;
        }

        //Check within bounds of array, if not exit function
        if (!(otherX < particleVelocityArray.GetLength(0) && otherX >= 0))
            return;
        if (!(otherZ < particleVelocityArray.GetLength(1) && otherZ >= 0))
            return;

        //If this is checking the same direction as velocity
        if (velocity.x >= 1 && otherX - thisX >= 1)
        {
            //Continue going this way
            MoveParticleTo(thisX, thisZ, otherX, otherZ);
        }
    }

    void MoveParticleTo(int thisX, int thisZ, int otherX, int otherZ)
    {
        //Add density to other particle
        particleDensityArray[otherX, otherZ] += baseFlowRate;
        //Lower density of this particle
        particleDensityArray[thisX, thisZ] -= baseFlowRate;
        //Set velocity based on direction
        particleVelocityArray[thisX, thisZ] = new Vector3(otherX - thisX, 0, otherZ - thisZ);

        //If object has not yet been instantiated, add it to the index list so it will be
        if (particleObjectArray[otherX, otherZ] == null)
            //Adds object to buffer
            objectIndexListBuffer.Add(new Vector2(otherX, otherZ));
    }
}
