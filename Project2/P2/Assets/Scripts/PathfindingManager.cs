﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using Assets.Scripts.IAJ.Unity.Pathfinding;
using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures;
using Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics;
using Assets.Scripts.IAJ.Unity.Utils;
public class PathfindingManager : MonoBehaviour
{

    //Struct for default positions
    [Serializable]
    public struct defaultPos
    {
        public int index;
        public Vector2 startingPos;
        public Vector2 goalPos;
    }

    // List with the strutcs
    public List<defaultPos> defaultPositions;

    //Grid configuration
    public int width;
    public int height;
    public float cellSize;
    public string gridName;
    static string gridPath;
    string[,] textLines;

    //Essential Pathfind classes 
    public AStarPathfinding pathfinding { get; set; }

    //The Visual Grid
    private GameObject[,] visualGrid;
    public GameObject gridNodePrefab;

    //Private fields for internal use only
    Vector3 startingPosition = new Vector3();
    Vector3 goalPosition = new Vector3();
    int startingX, startingY;
    int goalX, goalY;

    //public properties
    public bool partialPath;
    public bool UseEuclidean;
    public String searchAlgorithm;

    //Public Debug options
    public bool showCoordinates;
    Text debugCoordinates;
    Text debugG;
    Text debugF;
    Text debugH;
    Text debugWalkable;
    Text debugNodesExplored;
    Text debugMaxOpenSize;
    Text debugSearchTime;

    //Path
    List<NodeRecord> solution;

    private void Start()
    {
        gridPath = "Assets/Resources/" + gridName + ".txt";

        //Handling grid and its configurations
        textLines = new string[height, width];
        HandleTextFile();

        IHeuristic heuristic;
        if (UseEuclidean)
        {
            heuristic = new EuclideanDistance();
        }
        else
        {
            heuristic = new ZeroHeuristic();
        }

        switch (this.searchAlgorithm)
        {
            case "A*":
                this.pathfinding = new AStarPathfinding(width, height, cellSize, new SimpleUnorderedNodeList(), new ClosedDictionary(), heuristic);
                break;
            case "NodeArrayA*":
                this.pathfinding = new NodeArrayAStarPathfinding(width, height, cellSize, heuristic);
                break;
            case "JPS+":
                break;
            default:
                this.pathfinding = new AStarPathfinding(width, height, cellSize, new SimpleUnorderedNodeList(), new ClosedDictionary(), heuristic);
                break;
        }

        visualGrid = new GameObject[width, height];
        GridMapVisual();
        pathfinding.grid.OnGridValueChanged += Grid_OnGridValueChange;

        // Retrieving the Debug Components
        var debugTexts = GameObject.Find("InfoPanel").GetComponentsInChildren<Text>();
        debugCoordinates = debugTexts[0];
        debugH = debugTexts[1];
        debugG = debugTexts[2];
        debugF = debugTexts[3];
        debugWalkable = debugTexts[4];

        debugNodesExplored = debugTexts[6];
        debugMaxOpenSize = debugTexts[7];
        debugSearchTime = debugTexts[8];
    }

    // Update is called once per frame
    void Update()
    {

        // The first mouse click goes here, it defines the starting position;
        if (Input.GetMouseButtonDown(0) && startingPosition == new Vector3())
        {
            this.ClearGrid();

            startingPosition = RandomHelper.GetMouseWorldPosition();
            pathfinding.grid.GetXY(startingPosition, out startingX, out startingY);
            if (pathfinding.grid.GetGridObject(startingX, startingY).isWalkable)
            {
                this.SetObjectColor(startingX, startingY, Color.cyan);
            }
            else startingPosition = new Vector3();

        }

        // If we already have a starting posintion once we clicked we have our goalPosition
        else if (Input.GetMouseButtonDown(0) && goalPosition == new Vector3())
        {
            goalPosition = RandomHelper.GetMouseWorldPosition();

            pathfinding.grid.GetXY(startingPosition, out startingX, out startingY);

            pathfinding.grid.GetXY(goalPosition, out goalX, out goalY);

            if (pathfinding.grid.GetGridObject(goalX, goalY).isWalkable)
            {
                this.SetObjectColor(goalX, goalY, Color.green);
                InitializeSearch(startingX, startingY, goalX, goalY);
                startingPosition = new Vector3();
                goalPosition = new Vector3();
            }
            else
            {
                goalPosition = new Vector3();

            }

        }

        // Space clears the grid
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ClearGrid();
        }


        // If you press 1-5 keys you pathfinding will use default positions

        int index = 0;
        if (Input.GetKeyDown(KeyCode.Keypad1))
            index = 1;
        else if (Input.GetKeyDown(KeyCode.Keypad2))
            index = 2;
        else if (Input.GetKeyDown(KeyCode.Keypad3))
            index = 3;
        else if (Input.GetKeyDown(KeyCode.Keypad4))
            index = 4;
        else if (Input.GetKeyDown(KeyCode.Keypad5))
            index = 5;
        if (index != 0)
        {
            var def = defaultPositions.Find(x => x.index == index);
            startingX = (int)def.startingPos.x;
            startingY = (int)def.startingPos.y;
            goalX = (int)def.goalPos.x;
            goalY = (int)def.goalPos.y;
            InitializeSearch((int)def.startingPos.x, (int)def.startingPos.y, (int)def.goalPos.x, (int)def.goalPos.y);
        }

        // Text Debuggers
        var currentPosition = RandomHelper.GetMouseWorldPosition();
        if (currentPosition != null)
        {
            int x, y;
            if (pathfinding.grid != null)
            {
                pathfinding.grid.GetXY(currentPosition, out x, out y);
                if (x != -1 && y != -1)
                {
                    var node = pathfinding.grid.GetGridObject(x, y);
                    if (node != null)
                    {
                        debugCoordinates.text = " x:" + x + "; y:" + y;
                        debugG.text = "G:" + node.gCost;
                        debugF.text = "F:" + node.fCost;
                        debugH.text = "H:" + node.hCost;
                        debugWalkable.text = "IsWalkable:" + node.isWalkable;

                        debugNodesExplored.text = "# Nodes Explored: " + pathfinding.TotalExploredNodes;
                        debugMaxOpenSize.text = "Max Open Size: " + pathfinding.MaxOpenNodes;
                        debugSearchTime.text = "Total Search Time: " + pathfinding.TotalProcessingTime;
                    }
                }

            }
        }

        if (this.pathfinding.InProgress)
        {
            var finished = this.pathfinding.Search(out this.solution, partialPath);

            if (finished)
            {
                this.pathfinding.InProgress = false;
                DrawPath(this.solution);

                // Text Debuggers
                debugNodesExplored.text = "# Nodes Explored: " + pathfinding.TotalExploredNodes;
                debugMaxOpenSize.text = "Max Open Size: " + pathfinding.MaxOpenNodes;
                debugSearchTime.text = "Total Search Time: " + pathfinding.TotalProcessingTime;
            }

            this.pathfinding.TotalProcessingTime += Time.deltaTime;

        }

    }


    public void InitializeSearch(int _startingX, int _startingY, int _goalX, int _goalY)
    {
        this.SetObjectColor(startingX, startingY, Color.cyan);
        this.SetObjectColor(goalX, goalY, Color.green);

        this.pathfinding.InitializePathfindingSearch(_startingX, _startingY, _goalX, _goalY);

    }

    public void DrawPath(List<NodeRecord> path)
    {
        int index = 0;
        foreach (var p in path)
        {
            index += 1;
            if (index == 1)
            {
                this.SetObjectColor(p.x, p.y, Color.cyan);
                continue;
            }

            if (index == path.Count)
            {
                this.SetObjectColor(p.x, p.y, Color.green + new Color(0.5f, 0.0f, 0.5f));
                break;
            }

            this.SetObjectColor(p.x, p.y, Color.green);
        }
    }

    // Create the grid according to the text file set in the "Assets/Resources/grid.txt"
    private void GridMapVisual()
    {

        //Informing the grid of nodes that are not walkable
        for (int i = 0; i < textLines.GetLength(0); i++)
            for (int j = 0; j < textLines.GetLength(1); j++)

                if (textLines[i, j] == "1")
                {
                    var node = pathfinding.grid.GetGridObject(j, height - i - 1);

                    node.isWalkable = false;
                    pathfinding.grid.SetGridObject(node.x, node.y, node);

                }


        for (int x = 0; x < pathfinding.grid.getWidth(); x++)
            for (int y = 0; y < pathfinding.grid.getHeight(); y++)
            {

                visualGrid[x, y] = CreateGridObject(this.gridNodePrefab, pathfinding.grid.GetGridObject(x, y)?.ToString(), cellSize, pathfinding.grid.GetWorldPosition(x, y) + new Vector3(cellSize, 2, cellSize) * 0.5f, 40, Color.black, Color.white);
            }
        UpdateGrid();
    }

    // Instantiating a Grid Object from the prefab, I know, its a lot of small line in a row but its working :)
    private GameObject CreateGridObject(GameObject prefab, string value, float cellsize, Vector3 position, int fontSize, Color fontColor, Color imageColor)
    {

        var obj = GameObject.Instantiate(prefab);
        Transform transform = obj.transform;
        transform.localScale = new Vector3(cellsize - 1, cellSize - 1, cellSize - 1);
        transform.localPosition = position;

        if (showCoordinates)
        {
            TextMesh text = obj.GetComponentInChildren<TextMesh>();
            text.text = value;
            text.fontSize = fontSize;
            text.color = fontColor;
        }
        SpriteRenderer s = obj.GetComponent<SpriteRenderer>();
        s.color = imageColor;
        return obj;

    }

    // Reset the Grid to black and white
    public void ClearGrid()
    {
        for (int x = 0; x < pathfinding.grid.getWidth(); x++)
            for (int y = 0; y < pathfinding.grid.getHeight(); y++)
            {
                if (pathfinding.grid.GetGridObject(x, y).isWalkable)
                    this.SetObjectColor(x, y, Color.white);
                else this.SetObjectColor(x, y, Color.black);
            }

        goalPosition = new Vector3();
        startingPosition = new Vector3();
        this.pathfinding.InProgress = false;

    }

    //Setting the color of the Node 
    public void SetObjectColor(int x, int y, Color color)
    {
        visualGrid[x, y].GetComponent<SpriteRenderer>().color = color;
    }


    public void UpdateGrid()
    {
        for (int x = 0; x < pathfinding.grid.getWidth(); x++)
            for (int y = 0; y < pathfinding.grid.getHeight(); y++)
            {
                NodeRecord node = pathfinding.grid.GetGridObject(x, y);
                if (!node.isWalkable)
                    this.SetObjectColor(x, y, Color.black);
                else if (node.status == NodeStatus.Open)
                    this.SetObjectColor(x, y, Color.blue);
                else if (node.status == NodeStatus.Closed)
                    this.SetObjectColor(x, y, Color.red);

            }
    }

    private void Grid_OnGridValueChange(object sender, Assets.Scripts.Grid.Grid<NodeRecord>.OnGridValueChangedEventArgs e)
    {
        NodeRecord node = pathfinding.grid.GetGridObject(e.x, e.y);
        if (node != null)
        {

            if (!node.isWalkable)
                this.SetObjectColor(e.x, e.y, Color.black);
            else if (node.status == NodeStatus.Open)
                this.SetObjectColor(e.x, e.y, Color.blue);
            else if (node.status == NodeStatus.Closed)
                this.SetObjectColor(e.x, e.y, Color.red);
        }
    }

    // Reading the text file that where the grid "definition" is stored
    public void HandleTextFile()
    {

        //Read the text from directly from the test.txt file
        StreamReader reader = new StreamReader(gridPath);
        var fileContent = reader.ReadToEnd();
        reader.Close();
        var lines = fileContent.Split("\n"[0]);

        int i = 0;
        foreach (var l in lines)
        {
            var words = l.Split();
            var j = 0;

            var w = words[0];

            foreach (var letter in w)
            {
                textLines[i, j] = letter.ToString();
                j++;

                if (j == textLines.GetLength(1))
                    break;
            }

            i++;
            if (i == textLines.GetLength(0))
                break;
        }


    }

}
