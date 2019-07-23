using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MaxIntCounter
{
    private int value;

    [SerializeField]
    private int count = 2;

    private bool hasReached;
    public bool HasReached
    {
        get { return hasReached; }
    }

    public MaxIntCounter(int maxValue)
    {
        this.count = maxValue;
    }

    public void Add(int value)
    {
        this.value = value;

        if(this.value > count)
        {
            this.value = count;
            hasReached = true;
        }
    }

    public void Reset()
    {
        value = 0;
    }
}


public class RoadGenerator : MonoBehaviour
{
    [Header("Pool Setting")]
    [SerializeField]
    private List<GameObject> prefabObjects;

    private CataloguePool<RoadTile.Type, RoadTile> roadTilePool;

    [SerializeField]
    private GameObject rotatorPrefab;

    private SingularPool<Rotator> rotatorPool;

    [SerializeField][Range(5,30)]
    private int initialCountFromEach = 10;

    private Vector3 lastWorldPoint;

    private Dictionary<Vector2, RoadTile> activeTileMap;

    [SerializeField]
    private Vector2 standingTilePosition = Vector2.zero;

    [SerializeField]
    private Vector2 lastOutDirection = new Vector2(0,1);

    [Header("Generation Settings")]
    [SerializeField]
    private int minimumCameraDistance = 10;
    
    private new Camera camera;

    [SerializeField]
    private float turnPossibility = 0.5f;

    [SerializeField]
    private Timer cleanupCheckTimer = new Timer(4);

    private int tryCounter = 0;

    [SerializeField]
    private int oppositionCount;

    private void Awake()
    {
        //Get main camera;
        camera = Camera.main;

        List<RoadTile> tilePrefabs = new List<RoadTile>();

        activeTileMap = new Dictionary<Vector2, RoadTile>();

        prefabObjects.RemoveAll((t) =>
            {
                RoadTile tile = t.GetComponent<RoadTile>();

                if(tile != null)
                {
                    tilePrefabs.Add(tile);
                }
                return tile == null;
            }
        );

        Dictionary<RoadTile.Type, RoadTile> tileCatalogue = new Dictionary<RoadTile.Type, RoadTile>();
        foreach (RoadTile tile in tilePrefabs)
        {
            if (!tileCatalogue.ContainsKey(tile.TileType))
            {
                tileCatalogue.Add(tile.TileType, tile);
            }
        }
        
        roadTilePool = new CataloguePool<RoadTile.Type, RoadTile>(initialCountFromEach, transform, tileCatalogue);
        foreach(RoadTile tile in roadTilePool.InitializePool())
        {
            tile.InitializeAsPoolItem();
        }

        Rotator rotator = rotatorPrefab.GetComponent<Rotator>();
        rotatorPool = new SingularPool<Rotator>(initialCountFromEach, transform, rotator);
        foreach (Rotator item in rotatorPool.InitializePool())
        {
            item.InitializeAsPoolItem();
        }

        InstallTile(RoadTile.Type.Straight, Vector3.zero, Vector3.forward);
        standingTilePosition = lastOutDirection;
        lastOutDirection = GetNewOutDirection(RoadTile.Type.Straight, lastOutDirection);
        

        Generate();
    }

    private void Update()
    {
        Generate();
        Cleanup();
    }

    void Generate()
    {
        while (!HasEscapedCamera())
        {
            //Select a type.
            bool tileApproved = false;
            RoadTile.Type selectedType = RoadTile.Type.Straight;

            while (!tileApproved)
            {
                float turnRoll = Random.Range(0, 1f);

                if (turnRoll > 1 - turnPossibility)
                {
                    //Turn it is;
                    if (0.5f>Random.Range(0, 1f))
                    {
                        selectedType = RoadTile.Type.Left;
                        if (oppositionCount < 0)
                        {
                            selectedType = RoadTile.Type.Right;
                        }

                    }
                    else
                    {
                        selectedType = RoadTile.Type.Right;
                        if (oppositionCount > 0)
                        {
                            selectedType = RoadTile.Type.Left;
                        }
                    }

                }
                else
                {
                    //Straight!!!
                    selectedType = RoadTile.Type.Straight;
                }

                tileApproved = IsNextTileWillBeValid(selectedType);

                if (tileApproved)
                {
                    if(selectedType == RoadTile.Type.Left)
                    {
                        oppositionCount -= 1;
                        
                    }
                    else if(selectedType == RoadTile.Type.Right)
                    {
                        oppositionCount += 1;
                        
                    }

                    
                }

                tryCounter += 1;
                if(tryCounter == 10000)
                {
                    Debug.Log("I AM OUT at = " + roadTilePool.InstantiatedCount);
                    break;
                }
            }

            if(tryCounter == 10000)
            {
                enabled = false;
                break;
            }
            else
            tryCounter = 0;

            InstallTile(selectedType, roadTilePool.LastInstantiated.OutPoint.position, roadTilePool.LastInstantiated.OutPoint.forward);

            standingTilePosition += lastOutDirection;
            lastOutDirection = GetNewOutDirection(selectedType, lastOutDirection);
        }
    }

    Vector2 GetNewOutDirection(RoadTile.Type type, Vector2 lastOutDirection)
    {
        Vector2 newOutDirection = lastOutDirection;
        Vector2 left = Vector2.Perpendicular(lastOutDirection);
        Vector2 right = -left;

        if (type == RoadTile.Type.Right)
        {
            newOutDirection = right;
        }
        else if (type == RoadTile.Type.Left)
        {
            newOutDirection = left;
        }

        return newOutDirection;
    }

    

    bool IsNextTileWillBeValid(RoadTile.Type type)
    {
        Vector2 nextTilePosition = Vector2.zero;
        Vector2 simulatedOutDirection = GetNewOutDirection(type, lastOutDirection);
        nextTilePosition = lastOutDirection + simulatedOutDirection + standingTilePosition;

        return !activeTileMap.ContainsKey(nextTilePosition);
    }

    void Cleanup()
    {
        cleanupCheckTimer.Tick(1);

        if (cleanupCheckTimer.IsFinished)
        {
            List<RoadTile> tiles = roadTilePool.InstantiatedList.FindAll((t) => !t.IsBeingSeenByCamera(camera) && t.Passed);

            

            while(tiles.Count > 0)
            {
                ReturnToPool(tiles[0]);
                tiles.RemoveAt(0);
            }

            cleanupCheckTimer.Reset();
        }
    }

    bool HasEscapedCamera()
    {
        return Vector3.Distance(lastWorldPoint, camera.transform.position) > minimumCameraDistance;
    }

    public void InstallTile(RoadTile.Type type, Vector3 position, Vector3 direction)
    {
        RoadTile installedTile = roadTilePool.GetFromPool(type);
        installedTile.transform.position = position;
        installedTile.transform.rotation = Quaternion.LookRotation(direction);

        Vector2 mapPosition = standingTilePosition + lastOutDirection;

        activeTileMap.Add(mapPosition, installedTile);

        lastWorldPoint = position;

        if (type != RoadTile.Type.Straight)
        {
            Rotator rotator;

            RotatorLink rotatorLink = installedTile.GetComponent<RotatorLink>();

            if (roadTilePool[roadTilePool.InstantiatedCount - 2].TileType != type)
            {
                rotator = rotatorPool.GetFromPool();

                float clockwiseValue;
                if(installedTile.TileType == RoadTile.Type.Left)
                {
                    clockwiseValue = -1;
                }
                else
                {
                    clockwiseValue = 1;
                }
                rotator.ApplyInstallProcess(clockwiseValue);
                rotatorLink.InstallRotator(rotator);
            }
            else
            {
                rotatorLink.Rotator = roadTilePool[roadTilePool.InstantiatedCount - 2].GetComponent<RotatorLink>().Rotator;
            }
        }

        installedTile.ApplyInstallProcess(direction, position, mapPosition);
    }

    public void ReturnToPool(RoadTile tile)
    {
        tile.ApplyReturnProcess();

        if(tile.TileType != RoadTile.Type.Straight)
        {
            RotatorLink rotatorLink = tile.GetComponent<RotatorLink>();
            Rotator rotator = rotatorLink.DetachRotator();
            rotator.ApplyReturnProcess();
            rotatorPool.ReturnToPool(rotator);
        }

        roadTilePool.ReturnToPool(tile.TileType, tile);
        activeTileMap.Remove(tile.MapPosition);
    }
}
