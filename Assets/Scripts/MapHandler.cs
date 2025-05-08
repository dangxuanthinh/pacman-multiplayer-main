using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;

public class MapHandler : NetworkBehaviour
{
    [SerializeField] private bool showGridCoordinate;
    [SerializeField] private GameObject floorPrefab;
    [SerializeField] private Wall wallPrefab;
    [SerializeField] private PortalParticle portalParticlePrefab;

    [SerializeField] private List<Color> portalColors = new List<Color>();

    private Vector3 horizontalRotation = new Vector3(0, 90, 90);
    private Vector3 verticalRotation = new Vector3(90, 45, 45);


    [SerializeField] private PacmanMap mapForScreenShot;

    public void SpawnMapForScreenShot()
    {
        var grid = mapForScreenShot.GetGrid();
        List<Node> teleportNodes = new List<Node>();
        for (int x = 0; x < grid.width; x++)
        {
            for (int y = 0; y < grid.height; y++)
            {
                Node node = grid.GetCellValue(x, y);
                if (node.teleportDesination != null)
                    teleportNodes.Add(node);
                if (showGridCoordinate)
                    Utils.CreateWorldText($"{node.x},{node.y}", transform, node.GetWorldPosition(), 2, Color.white);
                if (node.passable == false)
                {
                    Wall wall = Instantiate(wallPrefab, transform);
                    wall.isScaleRandom = false;
                    wall.GetComponentInChildren<MeshRenderer>().material.color = mapForScreenShot.colorTheme;
                    wall.transform.position = node.GetWorldPosition();
                }
            }
        }
        GameObject floor = Instantiate(floorPrefab, transform);
        floor.transform.localPosition = new Vector3(-0.5f, -0.5f, 0.5f);
        floor.transform.localScale = new Vector3(grid.width, grid.height, 0.1f);


        List<Node> spawnedPortalNodes = new List<Node>();
        int portalColorIndex = 0;
        foreach (Node teleportNode in teleportNodes)
        {
            if (spawnedPortalNodes.Contains(teleportNode)) continue;

            PortalParticle firstPortalParticle = Instantiate(portalParticlePrefab, transform);
            PortalParticle secondPortalParticle = Instantiate(portalParticlePrefab, transform);

            firstPortalParticle.transform.position = teleportNode.GetWorldPosition();
            secondPortalParticle.transform.position = teleportNode.teleportDesination.GetWorldPosition();

            if (teleportNode.y == teleportNode.teleportDesination.y)
            {
                firstPortalParticle.transform.eulerAngles = horizontalRotation;
                secondPortalParticle.transform.eulerAngles = horizontalRotation;
            }
            else if (teleportNode.x == teleportNode.teleportDesination.x)
            {
                firstPortalParticle.transform.eulerAngles = verticalRotation;
                secondPortalParticle.transform.eulerAngles = verticalRotation;
            }

            if (portalColorIndex >= portalColors.Count)
            {
                Debug.LogWarning("Not enough colors compare to portals");
                portalColorIndex = 0;
            }

            firstPortalParticle.SetColor(portalColors[portalColorIndex]);
            secondPortalParticle.SetColor(portalColors[portalColorIndex]);
            portalColorIndex++;

            spawnedPortalNodes.Add(teleportNode);
            spawnedPortalNodes.Add(teleportNode.teleportDesination);
        }
    }

    private void Start()
    {
        AudioManager.Instance.Stop("LobbyMusic");
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Escape))
        //{
        //    SpawnMapForScreenShot();
        //}
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    ScreenCapture.CaptureScreenshot(map.mapName + ".png");
        //}
    }

    public override void OnNetworkSpawn()
    {
        GameManager.Instance.OnMapSet += CreateMap;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        GameManager.Instance.OnMapSet -= CreateMap;
    }

    public void CreateMap()
    {
        CustomGrid<Node> grid = GameManager.Instance.grid;
        List<Node> teleportNodes = new List<Node>();
        for (int x = 0; x < grid.width; x++)
        {
            for (int y = 0; y < grid.height; y++)
            {
                Node node = grid.GetCellValue(x, y);
                if (node.teleportDesination != null)
                    teleportNodes.Add(node);
                if (showGridCoordinate)
                    Utils.CreateWorldText($"{node.x},{node.y}", transform, node.GetWorldPosition(), 2, Color.white);
                if (node.passable == false)
                {
                    Wall wall = Instantiate(wallPrefab, transform);
                    wall.isScaleRandom = false;
                    wall.GetComponentInChildren<MeshRenderer>().material.color = GameManager.Instance.map.colorTheme;
                    wall.transform.position = node.GetWorldPosition();
                }
            }
        }
        GameObject floor = Instantiate(floorPrefab, transform);
        floor.transform.localPosition = new Vector3(-0.5f, -0.5f, 0.5f);
        floor.transform.localScale = new Vector3(grid.width, grid.height, 0.1f);


        List<Node> spawnedPortalNodes = new List<Node>();
        int portalColorIndex = 0;
        foreach (Node teleportNode in teleportNodes)
        {
            if (spawnedPortalNodes.Contains(teleportNode)) continue;

            PortalParticle firstPortalParticle = Instantiate(portalParticlePrefab, transform);
            PortalParticle secondPortalParticle = Instantiate(portalParticlePrefab, transform);

            firstPortalParticle.transform.position = teleportNode.GetWorldPosition();
            secondPortalParticle.transform.position = teleportNode.teleportDesination.GetWorldPosition();

            if (teleportNode.y == teleportNode.teleportDesination.y)
            {
                firstPortalParticle.transform.eulerAngles = horizontalRotation;
                secondPortalParticle.transform.eulerAngles = horizontalRotation;
            }
            else if (teleportNode.x == teleportNode.teleportDesination.x)
            {
                firstPortalParticle.transform.eulerAngles = verticalRotation;
                secondPortalParticle.transform.eulerAngles = verticalRotation;
            }

            if (portalColorIndex >= portalColors.Count)
            {
                Debug.LogWarning("Not enough colors compare to portals");
                portalColorIndex = 0;
            }

            firstPortalParticle.SetColor(portalColors[portalColorIndex]);
            secondPortalParticle.SetColor(portalColors[portalColorIndex]);
            portalColorIndex++;

            spawnedPortalNodes.Add(teleportNode);
            spawnedPortalNodes.Add(teleportNode.teleportDesination);
        }
    }
}
