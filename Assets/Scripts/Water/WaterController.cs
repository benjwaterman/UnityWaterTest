﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Codes.Linus.IntVectors; //Int vectors from https://github.com/LinusVanElswijk/Unity-Int-Vectors

public class WaterController : MonoBehaviour {
    //Singleton reference
    public static WaterController Current;

    public WaterController() {
        Current = this;
    }

    void Awake() {
        Current = this;
    }

    //For debugging water
    public const bool fDebug = false;

    //Layers the water should collide with
    public LayerMask WaterCollisionLayers;
    public GameObject waterObject;
    public const int gridSizeX = 101;
    public const int gridSizeY = 101;

    public WaterCell[,] waterCellArray = new WaterCell[gridSizeX, gridSizeY];

    //Which water layout should be used
    public int WaterLayout = 0;
    //Constant water
    public bool ConstantWaterFlow = false;

    //Store whether space is empty or not
    int[,] worldHeightArray = new int[gridSizeX, gridSizeY];

    //Vec2i references to water cells
    //Potentially can make this static array and have a stop value, eg when value is negative, ignore teh rest of the array
    List<Vector2i> activeCellIndexListA = new List<Vector2i>();
    //Have 2 as 1 acts as a buffer
    List<Vector2i> activeCellIndexListB = new List<Vector2i>();
    //Used to store the temporary list
    List<Vector2i> tempList = new List<Vector2i>();

    //Minimum difference between two cells before they water will move
    float minVolume = 0.005f;
    //The rate at which the water flows
    float baseFlowRate = 0.45f; //0.25
    float flowRate = 15;
    float timePassed = 0;

    Direction[] neighboursToCompare = { Direction.xPositive, Direction.xNegative, Direction.zPositive, Direction.zNegative };

    //Mesh stuff
    Vector3[] vertices = new Vector3[gridSizeX * gridSizeY];
    Vector3[] normals;
    Vector2[] uvs = new Vector2[gridSizeX * gridSizeY];
    int[] triangles;

    //Bool to store paused state
    bool bIsPaused = false;

    //Make sure everything is initiallised before starting update
    bool hasFinishedSetup = false;

    void Start() {
        //Initialise cell array
        InitialiseCellArray();

        //Build world height array before anything else
        BuildWorldHeightArray();

        //Create starting cell(s)
        //UpdateCellVolume(90, 5, 1000);
        //UpdateCellVolume(90, 55, 1000);
        //UpdateCellVolume(90, 95, 1000);
        //UpdateCellVolume(90, 25, 1000);
        //UpdateCellVolume(90, 75, 1000);

        //Fill river
        SpawnWater(WaterLayout);

        //Initialise the mesh variables
        UpdateMesh();

        //Assign mesh to gameobject
        Mesh waterMesh = new Mesh();
        gameObject.AddComponent<MeshFilter>().mesh = waterMesh;

        //Apply variables to mesh
        ApplyMesh();

        //Apply mesh collider
        UpdateMeshCollider();

        StartCoroutine(Wait());
    }

    //Wait before allowing update to start
    IEnumerator Wait() {
        yield return new WaitForSeconds(0.1f);
        hasFinishedSetup = true;
    }

    void Update() {
        if(!hasFinishedSetup) {
            return;
        }

        baseFlowRate = flowRate * Time.deltaTime;

        //If not paused update time passed
        if (!bIsPaused) {

            //Flow coming into the river
            //for (int i = 0; i < 22; i++) {
            //    UpdateCellVolume(79 + i, 100, 4);
            //    UpdateCellVolume(79 + i, 99, 4);
            //    UpdateCellVolume(79 + i, 98, 4);
            //}

            //Number of times to run sim per frame
            for (int pass = 0; pass < 1; pass++) {
                UpdateCells();

                //Keep updating cells to constantly be full
                if(ConstantWaterFlow) {
                    SpawnWater(WaterLayout);
                }
            }

            //Updates height of gameobjects, for debugging
            if (fDebug)
                dbUpdateGameObjects();

        }

        //Update mesh
        UpdateMesh();
        ApplyMesh();
    }

    void SpawnWater(int layoutNumber) {
        switch(layoutNumber) {
            //Right river
            case 0:
                for (int x = 0; x < 22; x++) {
                    for (int y = 0; y < 101; y++) {
                        UpdateCellVolume(79 + x, y, 4);
                    }
                }
                break;

            //Four corners
            case 1:
                for (int x = 0; x < 22; x++) {
                    for (int y = 0; y < 22; y++) {
                        UpdateCellVolume(79 + x, y + 1, 4);
                    }
                }

                for (int x = 0; x < 22; x++) {
                    for (int y = 0; y < 22; y++) {
                        UpdateCellVolume(1 + x, y + 1, 4);
                    }
                }

                for (int x = 0; x < 22; x++) {
                    for (int y = 0; y < 22; y++) {
                        UpdateCellVolume(1 + x, 79 + y, 4);
                    }
                }

                for (int x = 0; x < 22; x++) {
                    for (int y = 0; y < 22; y++) {
                        UpdateCellVolume(79 + x, 79 + y, 4);
                    }
                }
                break;

            //Small square
            case 2:
                for (int x = 0; x < 4; x++) {
                    for (int y = 0; y < 4; y++) {
                        UpdateCellVolume(48 + x, y + 48, 2);
                    }
                }
                break;

            //Right river
            default:
                for (int x = 0; x < 22; x++) {
                    for (int y = 0; y < 101; y++) {
                        UpdateCellVolume(79 + x, y, 4);
                    }
                }
                break;
        }
    }

    public void Pause() {
        bIsPaused = true;
        UpdateMeshCollider();
    }

    public void Resume() {
        bIsPaused = false;
    }

    public void RefreshWorld() {
        BuildWorldHeightArray();
    }

    //Use this instead of recalculating entire world for individual points
    public void UpdateWorldHeightArray(Vector2i[] positions, int value) {
        foreach (Vector2i vec2 in positions) {
            int i = vec2.x;
            int j = vec2.y;

            int height = 0;
            //Check if area has any colliders in, only compare with specified layers
            var hitColliders = Physics.OverlapSphere(new Vector3(i, height, j), 0.51f, WaterCollisionLayers);
            if (hitColliders.Length > 0) {
                //If there is something here, don't update the array with the value, move on to next point
                continue;
            }
            worldHeightArray[i, j] = height;
            //Border of array should be classed as solid, make it the highest possible value
            if (i == 0 || i == worldHeightArray.GetLength(0) || j == 0 || j == worldHeightArray.GetLength(1))
                worldHeightArray[i, j] = int.MaxValue;

            //If there is nothing here then update to given vale
            if (worldHeightArray[i, j] == 0) {
                worldHeightArray[i, j] = value;

            }
        }
    }

    //Code adapted from http://wiki.unity3d.com/index.php/ProceduralPrimitives
    void UpdateMesh() {
        int vertIndex = 0;
        for (int i = 0; i < gridSizeX; i++) {
            for (int j = 0; j < gridSizeY; j++) {
                Vector3 position = new Vector3(waterCellArray[i, j].position.x, waterCellArray[i, j].volume, waterCellArray[i, j].position.y);
                vertices[vertIndex++] = position;
            }
        }

        vertices = new Vector3[gridSizeX * gridSizeY];
        for (int z = 0; z < gridSizeY; z++) {
            // [ -length / 2, length / 2 ]
            //float zPos = ((float)z / (gridSizeY - 1) - .5f) * gridSizeX;
            for (int x = 0; x < gridSizeX; x++) {
                // [ -width / 2, width / 2 ]
                //float xPos = ((float)x / (gridSizeX - 1) - .5f) * gridSizeY;
                float xPos = waterCellArray[x, z].position.x;
                float zPos = waterCellArray[x, z].position.y;
                vertices[x + z * gridSizeX] = new Vector3(xPos, waterCellArray[x, z].volume, zPos);
            }
        }

        normals = new Vector3[vertices.Length];
        for (int n = 0; n < normals.Length; n++)
            normals[n] = Vector3.up;

        for (int v = 0; v < gridSizeY; v++) {
            for (int u = 0; u < gridSizeX; u++) {
                uvs[u + v * gridSizeX] = new Vector2((float)u / (gridSizeX - 1), (float)v / (gridSizeY - 1));
            }
        }

        int nbFaces = (gridSizeX - 1) * (gridSizeY - 1);
        triangles = new int[nbFaces * 6];
        int t = 0;
        for (int face = 0; face < nbFaces; face++) {
            // Retrieve lower left corner from face ind
            int i = face % (gridSizeX - 1) + (face / (gridSizeY - 1) * gridSizeX);

            triangles[t++] = i + gridSizeX;
            triangles[t++] = i + 1;
            triangles[t++] = i;

            triangles[t++] = i + gridSizeX;
            triangles[t++] = i + gridSizeX + 1;
            triangles[t++] = i + 1;
        }

    }

    void ApplyMesh() {
        //Get mesh from gameobject
        Mesh waterMesh = gameObject.GetComponent<MeshFilter>().mesh;

        //Applies values. NOTE: Must be in this order
        waterMesh.vertices = vertices;
        waterMesh.normals = normals;
        waterMesh.uv = uvs;
        waterMesh.triangles = triangles;

        waterMesh.RecalculateBounds();
        waterMesh.RecalculateNormals();
        ;
    }

    void UpdateMeshCollider() {
        Mesh waterMesh = gameObject.GetComponent<MeshFilter>().mesh;
        MeshCollider waterMeshCollider = GetComponent<MeshCollider>();
        waterMeshCollider.sharedMesh = waterMesh;
    }

    void InitialiseCellArray() {
        int id = 0;
        for (int i = 0; i < gridSizeX; i++) {
            for (int j = 0; j < gridSizeY; j++) {
                //Iniitialise cell array at current position
                waterCellArray[i, j] = new WaterCell(0, new Vector2i(i, j), id++);
                //Create cell at position with -1 volume
                CreateCell(new Vector2i(i, j), 0);
                //Ensure all cells initially are inactive
                waterCellArray[i, j].fActive = false;
                //Add to active cell index, after first run through of this list all inactive cells will drop off, this is just to save manually added active cells to the list
                activeCellIndexListA.Add(new Vector2i(i, j));
            }
        }

        //Have to do this after all cells have been initialised otherwise neighbours wont exist
        for (int i = 0; i < gridSizeX; i++) {
            for (int j = 0; j < gridSizeY; j++) {
                //Populate neighbours
                waterCellArray[i, j].populateNeighbourReferences();
            }
        }
    }

    void CreateCell(Vector2i position, int volume) {
        int x = position.x;
        int y = position.y;

        //Set volume
        waterCellArray[x, y].volume = volume;

        //Create gameobjects for water cells, for debugging
        if (fDebug)
            dbCreateGameObject(position, volume);
    }

    public void UpdateCellVolume(int x, int y, float volume) {
        //Check cell is in range
        if (x < waterCellArray.GetLength(0) && x >= 0 && y < waterCellArray.GetLength(1) && y >= 0) {
            waterCellArray[x, y].volume = volume;
            waterCellArray[x, y].fActive = true;
        }
        else {
            Debug.Log("Created cell is not in range: x: " + x + " y: " + y);
        }
    }

    //Debugging
    void dbCreateGameObject(Vector2i position, int volume) {
        int x = position.x;
        int y = position.y;

        //Create gameobject and assign it
        waterCellArray[x, y].setGameObject((GameObject)Instantiate(waterObject, new Vector3(x, -1, y), Quaternion.identity));
        waterCellArray[x, y].getGameObject().GetComponent<WaterInfo>().position = new Vector2i(x, y);
    }

    //Debugging
    void dbUpdateGameObjects() {
        foreach (Vector2i index in activeCellIndexListB) {
            waterCellArray[index.x, index.y].getGameObject().transform.position = new Vector3(waterCellArray[index.x, index.y].position.x,
                waterCellArray[index.x, index.y].volume,
                waterCellArray[index.x, index.y].position.y);
        }
    }

    void BuildWorldHeightArray() {
        for (int i = 0; i < worldHeightArray.GetLength(0); i++) {
            for (int j = 0; j < worldHeightArray.GetLength(1); j++) {
                //Initialise all to being empty
                worldHeightArray[i, j] = 0;
                //Store height we're currently checking
                int height = 0;
                //Check if area has any colliders in
                var hitColliders = Physics.OverlapSphere(new Vector3(i, height, j), 0.51f, WaterCollisionLayers);
                //While there is something in this spot, keep searching upwards until there is a space
                while (hitColliders.Length > 0) {
                    height++;
                    hitColliders = Physics.OverlapSphere(new Vector3(i, height, j), 0.51f, WaterCollisionLayers);
                }
                //Store the height we got to 
                worldHeightArray[i, j] = height;
                //Border of array should be classed as solid, make it the highest possible value
                if (i == 0 || i == worldHeightArray.GetLength(0) || j == 0 || j == worldHeightArray.GetLength(1))
                    worldHeightArray[i, j] = int.MaxValue;
            }
        }
    }

    void UpdateCells() {
        //For each cell in cell index list A
        foreach (Vector2i index in activeCellIndexListA) {
            //And if cell is active
            //if (waterCellArray[index.x, index.y].fActive)
            {
                //Compare against all neighbours
                foreach (Direction dir in neighboursToCompare) {
                    //If neighbour exists and is in range
                    if (index.x < gridSizeX - 1 && index.y < gridSizeY - 1 && index.x > 0 && index.y > 0) {
                        //Get addition vector from direction we are checking
                        Vector2i additionVec2i = new Vector2i(0, 0);
                        switch (dir) {
                            case Direction.xPositive:
                                additionVec2i = new Vector2i(1, 0);
                                break;

                            case Direction.xNegative:
                                additionVec2i = new Vector2i(-1, 0);
                                break;

                            case Direction.zPositive:
                                additionVec2i = new Vector2i(0, 1);
                                break;

                            case Direction.zNegative:
                                additionVec2i = new Vector2i(0, -1);
                                break;
                        }

                        //Check for solids in world height array, if this cell is less than the height of the solid, break out
                        if (waterCellArray[index.x, index.y].volume < worldHeightArray[index.x + additionVec2i.x, index.y + additionVec2i.y]) {
                            break;
                        }

                        //If this block is in a solid, volume should be 0
                        if (worldHeightArray[index.x, index.y] > 0) {
                            waterCellArray[index.x, index.y].volume = 0;
                            break;
                        }

                        //Get reference to neighbour cell
                        WaterCell neighbourCell = waterCellArray[index.x + additionVec2i.x, index.y + additionVec2i.y];
                        //If neighbour is not done
                        if (!neighbourCell.fDone) {
                            //If difference between volumes is greater than min amount
                            if (Mathf.Abs(waterCellArray[index.x, index.y].volume - neighbourCell.volume) > minVolume) {
                                //Set previous volume to current volume
                                waterCellArray[index.x, index.y].previousVolume = waterCellArray[index.x, index.y].volume;
                                //Adjust this volume
                                waterCellArray[index.x, index.y].volume -= (waterCellArray[index.x, index.y].volume - neighbourCell.volume) * baseFlowRate;
                                //Adjust neighbour volume using previous volume
                                neighbourCell.volume += (waterCellArray[index.x, index.y].previousVolume - neighbourCell.volume) * baseFlowRate;

                                //If neighbour is not active
                                if (!neighbourCell.fActive) {
                                    neighbourCell.fActive = true;
                                    activeCellIndexListB.Add(neighbourCell.position);
                                }
                                //If this cell is not active
                                if (!waterCellArray[index.x, index.y].fActive) {
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
        }

        //For cells in list B
        foreach (Vector2i index in activeCellIndexListB) {
            //Reset flags
            //waterCellArray[index.x, index.y].fActive = false;
            waterCellArray[index.x, index.y].fDone = false;
        }

        //Flip lists, it is done like this to prevent copying references and do a deep copy instead
        //A assigned to temp list
        tempList.AddRange(activeCellIndexListA);
        //B assigned to A
        activeCellIndexListA.Clear();
        activeCellIndexListA.AddRange(activeCellIndexListB);
        //A (tempList) assigned to B
        activeCellIndexListB.Clear();
        activeCellIndexListB.AddRange(tempList);
        //Clear temp list
        tempList.Clear();
    }
}
