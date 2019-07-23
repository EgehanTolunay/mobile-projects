using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Khalreon.Attributes;

public class BallLevelGenerator : MonoBehaviour
{
    public const float RING_FULL_RADIUS = 1.7f;
    public const float RING_INNER_RADIUS = 1.5f;
    public const float BALL_RADIUS = 0.7f;

    [Header("Pool Setting")]

    [SerializeField]
    private int initialCountFromEach = 10;

    [SerializeField]
    private List<GameObject> prefabTiles;

    [SerializeField]
    private float tileForwardSize = 1.5f;

    private CataloguePool<int,BallPlatformTile> tilePool;

    private int tileTypeIteration = 0;

    [SerializeField]
    private GameObject prefabPickup;

    [SerializeField]
    private int initialPickupSafeCount = 10;

    [SerializeField]
    [Range(0, 1)]
    private float basePickupPossibility = 0;

    [SerializeField][Range(0,1)]
    private float pickupPossibilityStep = 0.1f;

    private SingularPool<Pickup> pickupPool;

    private new Camera camera;

    [SerializeField]
    private float minimumCameraDistance = 10;

    [SerializeField]
    private float cleanupDistance = 30;

    [SerializeField]
    private Timer cleanupTimer = new Timer(5);

    [SerializeField][NumericLabel]
    private int totalTileCount = 0;
    [SerializeField]
    [NumericLabel]
    private int pickupStepIteration = 0;

    [SerializeField]
    [Disable]
    private Vector3 lastWorldPoint;

    [SerializeField]
    [Disable]
    private Vector3 lastPickupWorldPoint;

    [SerializeField][NumericLabel]
    private float pickupPossibility = 0;

    

    private void Start()
    {
        //Get main camera;
        camera = Camera.main;

        List<BallPlatformTile> tilePrefabs = new List<BallPlatformTile>();

        prefabTiles.RemoveAll((t) =>
        {
            BallPlatformTile tile = t.GetComponent<BallPlatformTile>();

            if (tile != null)
            {
                tilePrefabs.Add(tile);
            }
            return tile == null;
        }
        );

        Dictionary<int, BallPlatformTile> tileCatalogue = new Dictionary<int, BallPlatformTile>();
        foreach (BallPlatformTile tile in tilePrefabs)
        {
            if (!tileCatalogue.ContainsKey(tile.Type))
            {
                tileCatalogue.Add(tile.Type, tile);
            }
        }

        tilePool = new CataloguePool<int, BallPlatformTile>(initialCountFromEach, transform, tileCatalogue);
        foreach (BallPlatformTile tile in tilePool.InitializePool())
        {
            tile.InitializeAsPoolItem(true);
        }

        Pickup pickup = prefabPickup.GetComponent<Pickup>();
        pickupPool = new SingularPool<Pickup>(initialCountFromEach, transform, pickup);
        foreach (Pickup item in pickupPool.InitializePool())
        {
            item.InitializeAsPoolItem();
        }

        GenerateTiles();
    }

    private void Update()
    {
        GenerateTiles();

        GeneratePickups();

        Cleanup();
    }

    bool HasTilesEscapedCamera()
    {
        return Vector3.Distance(lastWorldPoint, camera.transform.position) > minimumCameraDistance;
    }

    bool HasPickupsEscapedCamera()
    {
        return Vector3.Distance(lastPickupWorldPoint, camera.transform.position) > minimumCameraDistance;
    }

    [SerializeField]
    private Vector2 horizontalPickupSpawnRange = new Vector2(-1,1);
    public float RandomHorizontalSpawnPoint
    {
        get { return Random.Range(horizontalPickupSpawnRange.x, horizontalPickupSpawnRange.y); }
    }
    private Dictionary<Pickup, PickupData> pickupDatas = new Dictionary<Pickup, PickupData>();

    private class PickupData
    {
        public int iteration;
    }

    [SerializeField][Range(0,5f)]
    private float playerReactionTime = 0.2f;

    void GeneratePickups()
    {
        while (!HasPickupsEscapedCamera())
        {
            pickupStepIteration += 1;

            if (totalTileCount > initialPickupSafeCount)
            {
                float pickupRoll = Random.Range(0f, 1f);
                if (pickupRoll > 1 - Mathf.Clamp01(pickupPossibility + basePickupPossibility))
                {
                    pickupPossibility = 0;

                    //In order to make this playable we need to consider passability of the level.
                    //We got a random. But we need to consider Ring size, ball size, player reaction time.
                    //And with these we need to ensure a passable structure by looking at past ring position.

                    float randomPosition = RandomHorizontalSpawnPoint;
                    float horizontalPosition = randomPosition;
                    //
                    //Ok first the iteration difference between past and current.
                    Pickup pastPickup = pickupPool.LastInstantiated;

                    if(pastPickup != null && pickupDatas.ContainsKey(pastPickup))
                    {
                        float pastPickupHorizontalPoint = pastPickup.transform.position.x;
                        PickupData pastPickupData = pickupDatas[pastPickup];

                        float horizontalDifference = randomPosition - pastPickupHorizontalPoint;

                        int iterationDifference = pickupStepIteration - pastPickupData.iteration;

                        Debug.Log("Iteration Difference :" + iterationDifference);

                        float maxAllowedDistance = (iterationDifference * (RING_INNER_RADIUS - BALL_RADIUS)) / (playerReactionTime + 1);

                        if (Mathf.Abs(horizontalDifference) > maxAllowedDistance)
                        {
                            horizontalPosition = pastPickupHorizontalPoint + Mathf.Sign(horizontalDifference) * maxAllowedDistance;
                        }

                        Debug.Log("Max allowed distance:"+maxAllowedDistance);
                    }

                    Vector3 pickupPosition = new Vector3(horizontalPosition, BallController.MaxBallHeight, ((float)pickupStepIteration) * BallController.MoveStep + BallController.MoveStep/2);

                    InstallPickup(pickupPosition);
                }
                else
                {
                    pickupPossibility += pickupPossibilityStep;
                    pickupPossibility = Mathf.Clamp01(pickupPossibility);
                }
            }
        }
    }

    void GenerateTiles()
    {
        while (!HasTilesEscapedCamera())
        {
            Vector3 tilePosition = new Vector3(0, 0, totalTileCount * tileForwardSize);

            InstallTile(tilePosition);
            
        }
    }

    bool DistanceSurpassed<T>(T tile, Vector3 lastWorldPoint) where T : MonoBehaviour
    {
        return Vector3.Distance(lastWorldPoint, tile.transform.position) > minimumCameraDistance + cleanupDistance;
    }

    void Cleanup()
    {
        cleanupTimer.Tick();

        if (cleanupTimer.IsFinished)
        {
            List<BallPlatformTile> tiles = tilePool.InstantiatedList.FindAll((t)=>DistanceSurpassed(t,lastWorldPoint));
            while (tiles.Count > 0)
            {
                ReturnTile(tiles[0]);
                tiles.RemoveAt(0);
            }

            List<Pickup> pickups = pickupPool.InstantiatedList.FindAll((t) => DistanceSurpassed(t, lastPickupWorldPoint));
            while (pickups.Count > 0)
            {
                ReturnPickup(pickups[0]);
                pickups.RemoveAt(0);
            }


            cleanupTimer.Reset();
        }
    }


    public void InstallTile(Vector3 position)
    {
        BallPlatformTile installedTile = tilePool.GetFromPool(tileTypeIteration, (t)=>t.InitializeAsPoolItem(false));
        installedTile.transform.position = position;

        lastWorldPoint = position;

        tileTypeIteration += 1;

        totalTileCount += 1;

        if (tileTypeIteration >= tilePool.TypeCount)
        {
            tileTypeIteration = 0;
        }

        installedTile.ApplyInstallProcess();
    }

    public void InstallPickup(Vector3 position)
    {
        Pickup installedPickup = pickupPool.GetFromPool((t)=> t.InitializeAsPoolItem());
        installedPickup.transform.position = position;

        lastPickupWorldPoint = position;
        
        PickupData newData = new PickupData();
        newData.iteration = pickupStepIteration;

        pickupDatas.Add(installedPickup, newData);

        installedPickup.ApplyInstallProcess();
    }

    public void ReturnTile(BallPlatformTile tile)
    {
        tile.ApplyReturnProcess();
        tile.OnFullHidden += OnTileDisappear;
    }

    private void OnTileDisappear(BallPlatformTile tile)
    {
        tilePool.ReturnToPool(tile.Type, tile);
        tile.OnFullHidden -= OnTileDisappear;
    }

    public void ReturnPickup(Pickup pickup)
    {
        pickup.ApplyReturnProcess();
        pickup.OnCompletelyHidden += OnPickupDisappear;
    }

    private void OnPickupDisappear(Pickup pickup)
    {
        pickupPool.ReturnToPool(pickup);
        pickupDatas.Remove(pickup);
        pickup.OnCompletelyHidden -= OnPickupDisappear;
    }
}
