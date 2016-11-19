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
    //List<GameObject> waterObjectList = new List<GameObject>();
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
        //Create starting block
        CreateParticle(new Vector2(50, 50));
    }

    void Update()
    {
        if (timePassed > 0.05)
        {
            UpdateSim();
            timePassed = 0;
        }
        timePassed += Time.deltaTime;
    }

    void CreateParticle(Vector2 position)
    {
        int x = (int)position.x;
        int y = (int)position.y;

        particleDensityArray[x, y] = 1000;
        particleObjectArray[x, y] = (GameObject)Instantiate(waterObject, new Vector3(x, 0, y), Quaternion.identity);
        objectIndexList.Add(new Vector2(x, y));
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
        /*
        for (int i = 0; i < particleVelocityArray.GetLength(0); i++)
        {
            for (int j = 0; j < particleVelocityArray.GetLength(1); j++)
            {

            }
        }*/

        //int rowLength = particleDensityArray.GetLength(0);
        //int colLength = particleDensityArray.GetLength(1);
        //string arrayToPrint = "";

        //for (int i = 0; i < rowLength; i++)
        //{
        //    for (int j = 0; j < colLength; j++)
        //    {
        //        arrayToPrint += "{" + particleDensityArray[i, j] + "}" + " ";



        //        //Clamp between 0.01f and 1f to stop odd shapes

        //        //particleObjectArray[i, j].transform.localScale = new Vector3(particleObjectArray[i, j].transform.localScale.x, newScale, particleObjectArray[i, j].transform.localScale.z);
        //    }
        //}

        ////Debug.Log(arrayToPrint);
    }

    //Check neighbours
    void CheckNeighbourDensity(int i, int j, int neighbourIndex)
    {
        int thisX = i;
        int thisZ = j;
        int otherX = i;
        int otherZ = j;

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

        /*switch (neighbourIndex)
        {
            case 1:
                //If neigbour is within the array (so not out of bounds) and its density is less then this density
                if (i + 1 < particleDensityArray.GetLength(0) && density > particleDensityArray[i + 1, j])
                {
                    //Add density to other particle
                    particleDensityArray[i + 1, j] += baseFlowRate;
                    //Lower density of this particle
                    density -= baseFlowRate;
                    //Add velocity
                    particleVelocityArray[i + 1, j] = new Vector3(1, 0, 0);
                }
                break;

            case 2:
                if (i - 1 >= 0 && density > particleDensityArray[i - 1, j])
                {
                    particleDensityArray[i - 1, j] += baseFlowRate;
                    density -= baseFlowRate;

                    particleVelocityArray[i - 1, j] = new Vector3(-1, 0, 0);
                }
                break;

            case 3:
                if (j + 1 < particleDensityArray.GetLength(1) && density > particleDensityArray[i, j + 1])
                {
                    particleDensityArray[i, j + 1] += baseFlowRate;
                    density -= baseFlowRate;

                    particleVelocityArray[i , j + 1] = new Vector3(0, 0, 1);
                }
                break;

            case 4:
                if (j - 1 >= 0 && density > particleDensityArray[i, j - 1])
                {
                    particleDensityArray[i, j - 1] += baseFlowRate;
                    density -= baseFlowRate;

                    particleVelocityArray[i, j - 1] = new Vector3(0, 0, -1);
                }
                break;

            default:
                break;
        }*/
    }

    void CheckNeighbourVelocity(int i, int j, int neighbourIndex)
    {
        int thisX = i;
        int thisZ = j;
        int otherX = i;
        int otherZ = j;

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
            //ATTENTION NEED TO CHANGE SHOULDNT DO THIS WHILE LOOPING THROUGH IT
            objectIndexListBuffer.Add(new Vector2(otherX, otherZ));

        /*
        //Moving in x direction
        if (otherX - thisX >= 1 || otherX - thisX <= -1)
        {
            particleVelocityArray[thisX, thisZ].x -= 1;
            particleVelocityArray[otherX, otherZ].x += 1;
        }

        //Moving in z direction
        else if (otherZ - thisZ >= 1 || otherZ - thisZ <= -1)
        {
            particleVelocityArray[thisX, thisZ].z -= 1;
            particleVelocityArray[otherX, otherZ].z += 1;
        }*/
    }
}
