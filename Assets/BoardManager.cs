using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum Element{DIAMOND, GOLD, SILVER, BRONZE, TIN, LEAD, WOOD, GAP, NOTHING};

public class BoardManager : MonoBehaviour 
{
    [Header("objects to link pls")]
    public GameObject tilePrefab;
    public RectTransform holder;
    public Canvas canvas;
    private Camera cam;

    [Header("grid paramaters")]
	public int xSize;
    public int ySize; 
    public int tileSize;

    [Header("visuals")]
    public List<Sprite> sprites;
    public Element highestStartingElement;
    private List<Element> startingElements = new List<Element>();

    // some static lads
    public static int TileSize;
    public static float HalfTileSize;
    public static bool canInput = true;

    // mouse related
    private bool mouseIsDown = false;
    private Vector2 startMousePosition = new Vector2();
    private Vector2 currentDragDirection = Vector2.zero;
    public Vector2Int snappedDragDirection = Vector2Int.zero;
    private List<Vector2Int> directions = new List<Vector2Int>() {Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left};
    private float dragMagnitude = 0;
    public float triggerDragMagnitude = 32;
    public float dampLimitSoft = 32;
    public float dampLimitHard = 64;

    // them tiles
    private Tile currentlySelected = null;
    private Tile currentlySwapping = null;
    private Tile[,] tiles;
    private List<Tile> tilesUnderAudit = new List<Tile>();
    
    void Start()
    {
        cam = canvas.worldCamera;
        TileSize = tileSize;
        HalfTileSize = TileSize*.5f;
        Tile.OnTileSelected += tileSelected;
        defineStartingElements();
        buildBoard();
        randomizeTiles();
    }

    void defineStartingElements()
    {
        for(int i=0; i <= (int)highestStartingElement; i++)
        {
            startingElements.Add((Element)i);
        }
    }

    void buildBoard()
    {
        tiles = new Tile[xSize, ySize];

        for(int y = 0; y < ySize; y++)
        {
            for(int x = 0; x < xSize; x++)
            {
                GameObject tileOb = Instantiate(tilePrefab, holder);
                Tile tile = tileOb.GetComponent<Tile>();
                tile.SetBoardPosition(new Vector2Int(x,y));
                tile.SnapToBoardPositon();
                tiles[x, y] = tile;
            }
        }
    }

    void randomizeTiles()
    {
        foreach(Tile tile in tiles)
        {
            Element el = randomElementFrom(ref startingElements);
            tile.SetElement(el, sprites[(int)el]);
        }
    }

    public Element randomElementFrom(ref List<Element> including)
    {
        return including[Random.Range(0,including.Count)];
    }

    void tileSelected(Tile tile)
    {
        if(!canInput)
            return;
        canInput = false;
        currentlySelected = tile;
        currentlySelected.hilight(Vector2Int.zero);
        StartCoroutine(dragTileCR());
    }

    Vector2 mousePosToUi(RectTransform trans)
    {
        Vector2 uiPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(holder, Input.mousePosition, cam, out uiPos);
        return uiPos;
    }

    IEnumerator dragTileCR()
    {
        Vector2 startPoint = mousePosToUi(holder);
        Vector2 offset = startPoint - currentlySelected.GetCurrentActualPosition();
        Vector2 localPoint;
        Vector2Int previousDragDirection = Vector2Int.zero;

        while(Input.GetMouseButton(0))
        {
            localPoint = mousePosToUi(holder);

            // info about drag
            dragMagnitude = Vector2.Distance(startPoint, localPoint);
            currentDragDirection = (localPoint - startPoint).normalized;

            // check if you've triggered a direction
            if(dragMagnitude >= triggerDragMagnitude)
            {
                snappedDragDirection = getClosesetDirection(currentDragDirection);
                currentDragDirection = Vector2.Lerp(currentDragDirection, snappedDragDirection, .5f);
                checkForTileAtDragDirection();
            }
            else
            {
                snappedDragDirection = Vector2Int.zero;
            }

            if(snappedDragDirection != previousDragDirection)
            {
                previousDragDirection = snappedDragDirection;
                currentlySelected.hilight(snappedDragDirection); // update direction
            }

            Vector2 elasticPos = startPoint + (currentDragDirection * (calculateDamp(dragMagnitude, dampLimitSoft, dampLimitHard)));

            currentlySelected.EaseToPosition(elasticPos - offset);
            yield return null;
        }
        tileDropped();
    }

    bool isValidPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < xSize && pos.y >= 0 && pos.y < ySize;
    }

    void checkForTileAtDragDirection()
    {
        Vector2Int currentPos = currentlySelected.boardPosition;
        Vector2Int nextPos = currentPos + snappedDragDirection;
        if(isValidPosition(nextPos))
        {
            if(tiles[nextPos.x, nextPos.y] != currentlySwapping)
            {
                if(currentlySwapping != null)
                    currentlySwapping.resetScale(); // reset old one

                currentlySwapping = tiles[nextPos.x, nextPos.y];
                currentlySwapping.shrink();
            }
        }
        else
        {
            if(currentlySwapping != null)
                    currentlySwapping.resetScale(); // reset old one
            currentlySwapping = null;
        }
    }

    float calculateDamp(float x, float limitSoft, float limitHard)
    {
        if(x < limitSoft)
            return x;

        return ((limitSoft - limitHard) *
                Mathf.Pow(
                            (float)System.Math.E,
                            -((x - limitSoft) / (limitHard - limitSoft))
                        )
                )
                + limitHard;
    }

    Vector2Int getClosesetDirection(Vector2 fromDirection)
    {
        float biggestDot = 0;
        Vector2Int closestDir = Vector2Int.zero;
        foreach(Vector2Int dir in directions)
        {
            float dot = Vector2.Dot(fromDirection, dir);
            if(dot > biggestDot)
            {
                biggestDot = dot;
                closestDir = dir;
            }
        }

        return closestDir;
    }

    void tileDropped()
    {
        currentlySelected.resetScale();
        if(currentlySwapping == null)
        {
            currentlySelected.SpringToBoardPositon();
            canInput = true;
        }
        else
        {
            // moves--;
            swapTiles();
            Invoke("checkSwappedTiles", .25f);
        }
    }

    void swapTiles()
    {
        // copy values
        Vector2Int selectedTilePos = currentlySelected.boardPosition;
        Vector2Int swappingTilePos = currentlySwapping.boardPosition;

        // set boardPositions
        currentlySelected.SetBoardPosition(swappingTilePos);
        currentlySwapping.SetBoardPosition(selectedTilePos);

        // set array positions
        tiles[selectedTilePos.x, selectedTilePos.y] = currentlySwapping;
        tiles[swappingTilePos.x, swappingTilePos.y] = currentlySelected;

        // move tiles
        currentlySelected.MoveToBoardPositon();
        currentlySwapping.MoveToBoardPositon();

        // reset scales
        currentlySelected.resetScale();
        currentlySwapping.resetScale();
    }

    void checkSwappedTiles()
    {
        // maybe just check whole board
        print("matches for " + currentlySelected.element + ": " + floodMatch(currentlySelected.boardPosition, currentlySelected.element));
        print("matches for " + currentlySwapping.element + ": " + floodMatch(currentlySwapping.boardPosition, currentlySwapping.element));
    }

    void checkWholeBoardForMatches()
    {
        for(int y = 0; y < ySize; y++)
        {
            for(int x = 0; x < xSize; x++)
            {
                int matches = floodMatch(new Vector2Int(x,y), tiles[x,y].element);
                if(matches >= 3)
                {
                    print(tiles[x,y].element + " matches: " + matches);
                }

            }
        }
    }

    int floodMatch(Vector2Int pos, Element el)
    {
        int total = 0;
        if(isValidPosition(pos))
        {
            Tile t = tiles[pos.x, pos.y];
            if(t.element == el)
            {
                if(!t.hasBeenChecked)
                {
                    t.hasBeenChecked = true;
                    tilesUnderAudit.Add(t);
                    total++;
                    total += floodMatch(new Vector2Int(pos.x + 1, pos.y), el);
                    total += floodMatch(new Vector2Int(pos.x - 1, pos.y), el);
                    total += floodMatch(new Vector2Int(pos.x, pos.y + 1), el);
                    total += floodMatch(new Vector2Int(pos.x, pos.y - 1), el);
                }

            }
        }
        return total;
    }
}
