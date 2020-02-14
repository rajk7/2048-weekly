using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class Move : MonoBehaviour
{
    public static int gridWidth = 4, gridHeight = 4;

    public static Transform[,] grid = new Transform[gridWidth, gridHeight];

    public static NotATile[,] previousGrid = new NotATile[gridWidth, gridHeight];

    public static NotATile[,] saveGrid = new NotATile[gridWidth, gridHeight];
    
    public Canvas gameOverCanvas;

    public Text gameScoreText;

    public Text bestScoreText;

    public int score = 0;

    private int previousScore = 0;

    private int savedScore = 0;

    public CFDebug debug;

    private int numberOffCoroutinesRunning = 0;

    private bool generatedNewTileThisTurn = true;

    public AudioClip moveTilesSound;
    
    public AudioClip mergeTilesSound;
    
    private AudioSource audioSource;

    bool madeFirstMove = false;

    bool savedGame = false;


    // Start is called before the first frame update
    void Start()
    {
        // PlayerPrefs.SetInt("bestscore",0);

        GenerateNewTile(2);

        audioSource = transform.GetComponent<AudioSource>();

        UpdateBestscore();


    }

    // Update is called once per frame
    void Update()
    {
        if (numberOffCoroutinesRunning == 0)
        {
            if (!generatedNewTileThisTurn)
            {
                generatedNewTileThisTurn = true;
                GenerateNewTile(1);
            }

            if(!CheckGameOver())
            {
                CheckUserInput();
            }
            else
            {
                SaveBestScore();
                UpdateScore();
                gameOverCanvas.gameObject.SetActive(true);
            }
        }
    }

    void CheckUserInput()
    {
        bool down = Input.GetKeyDown(KeyCode.DownArrow), up = Input.GetKeyDown(KeyCode.UpArrow), left = Input.GetKeyDown(KeyCode.LeftArrow), right = Input.GetKeyDown(KeyCode.RightArrow);
        
        if (down || up || left || right)
        {
            if (!madeFirstMove)
            madeFirstMove = true;

            StorePreiousTiles();

            PrepareTilesForMerging();

            if(down)
            {
                debug.Add("Player Pressed Key","down","checkuserInput");
                MoveAllTiles(Vector2.down);
            }

            if (up)
            {
                debug.Add("Player Pressed Key", "up", "checkuserInput");
                MoveAllTiles(Vector2.up);
            }

            if (left)
            {
                debug.Add("Player Pressed Key", "left", "checkuserInput");
                MoveAllTiles(Vector2.left);
            }

            if (right)
            {
                debug.Add("Player Pressed Key", "right", "checkuserInput");
                MoveAllTiles(Vector2.right);
            }
        } 
    }

    private void StorePreiousTiles()
    {
        previousScore = score;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
            Transform tempTile = grid[x, y];

            previousGrid[x, y] = null;

            if (tempTile != null)
            {
                NotATile notATile = new NotATile();

                notATile.location = tempTile.localPosition;
                notATile.value = tempTile.GetComponent<Tile>().tileValue;

                previousGrid[x, y] = notATile;
            }
        }
    }
    }

    void UpdateScore()
    {
        gameScoreText.text = score.ToString("000000000");
    }

    void UpdateBestscore()
    {
        bestScoreText.text = PlayerPrefs.GetInt("bestscore").ToString();
    }

    void SaveBestScore()
    {
        int oldBestScore = PlayerPrefs.GetInt("bestscore");

        if (score > oldBestScore)
        {
            PlayerPrefs.SetInt("bestscore", score);
        }
    }
    
    bool CheckGameOver()
    {


        if (transform.childCount < gridWidth * gridHeight)
        {
            debug.Add("Check Game Over", "false-Empty Spaces", "checkgameover");
            return false;
        }


        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++) 
            {
                Transform CurrentTile = grid[x,y];
                Transform tileBelow = null;
                Transform tileBeside = null;

                if(y !=0 )
                tileBelow = grid[x, y-1];

                if(x!=gridWidth - 1)
                tileBeside = grid[x+1, y];

                if(tileBeside != null)
                {
                    if(CurrentTile.GetComponent<Tile>().tileValue == tileBeside.GetComponent<Tile>().tileValue)
                    {

                        debug.Add("Check Game Over", "false-Tile Beside", "checkgameover");
                        return false;

                    }
                }

                if (tileBelow != null)
                {
                    if (CurrentTile.GetComponent<Tile>().tileValue == tileBelow.GetComponent<Tile>().tileValue)
                    {

                        debug.Add("Check Game Over", "false-Empty Spaces", "checkgameover");
                        return false;
                    }
                }
            }
        }

        debug.Add("Check Game Over", "True", "checkgameover");
        return true;
    }

    void MoveAllTiles(Vector2 direction)
    {
        int tilesMovedCount = 0;
        UpdateGrid();

        if(direction == Vector2.left)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if(grid[x,y] != null)
                    {
                        if(MoveTile(grid[x, y], direction))
                        tilesMovedCount++;
                    }
                }
            }
        }

        if (direction == Vector2.right)
        {
            for (int x = gridWidth -1; x >=0; x--)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (grid[x, y] != null)
                    {
                        if (MoveTile(grid[x, y], direction))
                            tilesMovedCount++;
                    }
                }
            }
        }

        if (direction == Vector2.down)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (grid[x, y] != null)
                    {
                        if (MoveTile(grid[x, y], direction))
                            tilesMovedCount++;
                    }
                }
            }
        }

        if (direction == Vector2.up)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = gridHeight-1; y >= 0; y--)
                {
                    if (grid[x, y] != null)
                    {
                        if (MoveTile(grid[x, y], direction))
                            tilesMovedCount++;
                    }
                }
            }
        }

        if(tilesMovedCount !=0)
        {
            generatedNewTileThisTurn = false;

            audioSource.PlayOneShot(moveTilesSound);
        }



        for (int y = 0; y < gridHeight; ++y)
        {
            for (int x = 0; x < gridWidth; ++x)
            {
                if(grid[x,y] != null) 
                {
                    Transform t = grid[x,y];

                    StartCoroutine(SlideTile(t.gameObject, 10f));
                }
            }
        }
    }

    bool MoveTile (Transform tile, Vector2 direction)
    {
        Vector2 startPos = tile.localPosition;
        Vector2 phantomTilePosition = tile.localPosition;

        tile.GetComponent<Tile>().startingPosition = startPos;

        while (true)
        {
            phantomTilePosition += direction;
            Vector2 previousPosition = phantomTilePosition - direction;


            if (CheckIsInsideGrid(phantomTilePosition))
            {
                if (CheckIsAtValidPosition(phantomTilePosition))
                {
                    tile.GetComponent<Tile>().moveToPosition = phantomTilePosition;
                    
                    grid[(int)previousPosition.x, (int)previousPosition.y] = null;
                    grid[(int)phantomTilePosition.x, (int)phantomTilePosition.y] = tile;

                }
                else 
                {
                    if (!CheckAndCombineTiles(tile,phantomTilePosition, previousPosition))
                    {

                        phantomTilePosition += -direction;
                        
                        tile.GetComponent<Tile>().moveToPosition = phantomTilePosition;
                        
                        if(phantomTilePosition == startPos)
                        {

                            return false;

                        }
                        else
                        {

                            return true;
                        }
                    }
                }
            } else 
            {
                phantomTilePosition += -direction;
                tile.GetComponent<Tile>().moveToPosition = phantomTilePosition;

                if (phantomTilePosition == startPos)
                {
                    return false;
                }
                 else
                {
                    return true;
                }
            }
        }
    }

        bool CheckAndCombineTiles (Transform movingTile, Vector2 phantomTilePosition, Vector2 previousPosition)
             {
                debug.Add("CheckeneComblneTlles", movingTile.name, "checkandcomblnetiles" + movingTile.localPosition.ToString());
                
                Vector2 pos = movingTile.transform.localPosition;
                
                Transform collidingTile = grid[(int)phantomTilePosition.x, (int)phantomTilePosition.y];
                
                int movingTileValue = movingTile.GetComponent<Tile>().tileValue;
                int collidingTileValue = collidingTile.GetComponent<Tile>().tileValue;
                
                if (movingTileValue == collidingTileValue && !movingTile.GetComponent<Tile>().mergedThisTurn && !collidingTile.GetComponent<Tile >().mergedThisTurn)
                 {
                    debug.Add("CheckAndiembineTiles", "Inside", "uncheckandcombine");
                    
                    movingTile.GetComponent<Tile>().destroyMe =true;
                    
                    movingTile.GetComponent<Tile>().collidingTile = collidingTile;
                    
                    movingTile.GetComponent <Tile>().moveToPosition = phantomTilePosition;

                    grid[(int)previousPosition.x, (int)previousPosition.y] = null;
                    grid[(int)phantomTilePosition.x, (int)phantomTilePosition.y] = movingTile;
                    
                    movingTile.GetComponent<Tile>().willMergeWithCollidingTile = true;
                    
                    UpdateScore();
                    
                    return true;

                }

                return false;
            }

            void GenerateNewTile(int howMany)
            {
                for(int i = 0; i < howMany; i++)
                {

                    Vector2 locationForNewTile = GetRandomLocationForNewTile();
                    
                    string tile = "tile_2";
                    
                    float chanceOfTwo = UnityEngine.Random.Range(0f,1f);
                    
                    if(chanceOfTwo>0.9f)
                    {

                        tile = "tile_4";
                    }

                    GameObject newTile = (GameObject)Instantiate(Resources.Load(tile, typeof(GameObject)), locationForNewTile, Quaternion.identity);

                    newTile.transform.parent = transform;

                    grid[(int)newTile.transform.localPosition.x, (int)newTile.transform.localPosition.y] = newTile.transform;

                    newTile.transform.localScale = new Vector2(0, 0);

                    newTile.transform.localPosition = new Vector2(newTile.transform.localPosition.x + 0.5f, newTile.transform.localPosition.y + 0.5f);

                    StartCoroutine(NewTilePopIn(newTile, new Vector2(0, 0), new Vector2(1,1), 10f, newTile.transform.localPosition, new Vector2(newTile.transform.localPosition.x - 0.5f, newTile.transform.localPosition.y - 0.5f)));
                
                }
            }

    void UpdateGrid()
    {
        for (int y = 0; y < gridWidth; ++y)
            {
                for (int x = 0; x < gridHeight; ++x)
                {
                    if(grid[x,y] != null)
                    {
                        if(grid[x,y].parent == transform)
                        {
                            grid[x,y] = null;
                        }
                    }
                }
            }

            foreach (Transform tile in transform)
            {
                Vector2 v = new Vector2(Mathf.Round(tile.position.x),Mathf.Round(tile.position.y));
                
                grid[(int)v.x, (int)v.y] = tile;
            }
        }


    Vector2 GetRandomLocationForNewTile() 
    {
        List<int> x = new List<int>();
        List<int> y = new List<int>();
            
            for (int i = 0; i < gridWidth; i++)
            {
                for (int j = 0; j < gridHeight; j++)
                {
                    if (grid[i,j] == null)
                    {
                        x.Add(j);
                        y.Add(i);
                    }
                }
            }

        int randIndex = UnityEngine.Random.Range(0,x.Count);
        
        int randX = x.ElementAt(randIndex);
        int randY = y.ElementAt(randIndex);

        debug.Add("new Random Tile Location", randX + ", "+ randY, "randomLocation");

        return new Vector2(randX,randY);
    }

        bool CheckIsInsideGrid(Vector2 pos) 
        {
            if (pos.x >= 0 && pos.x <= gridWidth - 1 && pos.y >=0 && pos.y <= gridHeight - 1)
            {
                return true;
            }

            return false;
        }

        bool CheckIsAtValidPosition(Vector2 pos)
        {
            if(grid[(int)pos.x, (int) pos.y] == null)
            {
                return true;
            }

            return false;
        }

        void PrepareTilesForMerging()
        {
            foreach (Transform t in transform)
            {
                t.GetComponent<Tile>().mergedThisTurn = false;
            }
        }




        public void PlayAgain()
        {
            grid = new Transform[gridWidth, gridHeight];
            
            score = 0;

            List<GameObject> children = new List<GameObject>();

            foreach (Transform t in transform)
            {
                children.Add(t.gameObject);
            }

            children.ForEach(t => DestroyImmediate(t));

            gameOverCanvas.gameObject.SetActive(false);

            UpdateScore();

            UpdateBestscore();

            GenerateNewTile(2);
        }

        public void Undo()
        {
            if (madeFirstMove)
            {
                //debug.Add("Undo pressed", "Yes", "Undo Pressed", CFDebugObject.DebugMessageKind.Informational, false);

                score = previousScore;

                UpdateScore();

                foreach (Transform child in transform)
                {

                    Destroy(child.gameObject);
                }

                for (int x = 0 ; x< gridWidth; x++)
                {

                    for(int y = 0;y<gridHeight; y++)
                    {

                        grid[x,y] = null;

                        NotATile notATile = previousGrid[x, y];

                        if (notATile !=null)
                        {

                            int tileValue = notATile.value;
                            string newTileName = "tile_" + tileValue;

                            GameObject newTile = (GameObject)Instantiate(Resources.Load(newTileName, typeof(GameObject)),notATile.location, Quaternion.identity );

                            newTile.transform.parent = transform;

                            grid[x, y]= newTile.transform;
                        }
                    }
                }
            }
        }

        public void SaveGame()
        {
            savedGame = true;

            savedScore = score;

            for(int x = 0; x < gridWidth; x++)
            {
                for(int y = 0; y < gridHeight; y++)
                {
                    saveGrid[x, y] = null;

                    if(grid[x,y] != null)
                    {
                        Transform t = grid[x,y];

                        Vector2 location = t.localPosition;
                        int value = t.GetComponent<Tile>().tileValue;

                        NotATile notATile = new NotATile();

                        notATile.location = location;
                        notATile.value = value;

                        saveGrid[x,y] = notATile;
                    }
                }
            }
        }

        public  void LoadGame()
        {
            if (savedScore < score)
            {
                score = savedScore;

                UpdateScore();

                foreach(Transform child in transform)
                {
                    Destroy(child.gameObject);
                }

                for (int x=0; x < gridWidth; x++)
                {
                    for(int y=0; y < gridHeight; y++)
                    {
                        grid[x, y] = null;

                        NotATile notATile = saveGrid[x,y];

                        if(notATile != null)
                        {
                            int tileValue = notATile.value;
                            string newTileName = "tile_" + tileValue;

                            GameObject newTile = (GameObject)Instantiate(Resources.Load(newTileName, typeof(GameObject)), notATile.location, Quaternion.identity);

                            newTile.transform.parent = transform;

                            grid[x,y] = newTile.transform;
                        }
                    }
                }
            }
        }

        IEnumerator NewTilePopIn (GameObject tile, Vector2 initialScale, Vector2 finalScale, float timeScale, Vector2 initialposition, Vector2 finalPosition)
        {
            numberOffCoroutinesRunning++;


            float progress = 0;

            while(progress <= 1)
            {
                tile.transform.localScale = Vector2.Lerp(initialScale, finalScale, progress);
                tile.transform.localPosition = Vector2.Lerp(initialScale, finalScale, progress);
                progress += Time.deltaTime * timeScale;
                yield return null;
            }
            tile.transform.localScale = finalScale;
            tile.transform.localPosition = finalPosition;

            numberOffCoroutinesRunning--;
        }
    IEnumerator SlideTile (GameObject tile, float timeScale)
    {
        numberOffCoroutinesRunning++;

        float progress = 0;
        
        while (progress <= 1)
        {
            tile.transform.localPosition = Vector2.Lerp(tile.GetComponent<Tile>().startingPosition, tile.GetComponent<Tile>().moveToPosition, progress);
            progress += Time.deltaTime * timeScale;
            yield return null;
        }
        tile.transform.localPosition = tile.GetComponent<Tile>().moveToPosition;

        if(tile.GetComponent<Tile>().destroyMe)
        {
            int movingTileValue = tile.GetComponent<Tile>().tileValue;

            if(tile.GetComponent<Tile>().collidingTile != null)
            {
                    DestroyImmediate(tile.GetComponent<Tile>().collidingTile.gameObject);
            }

                Destroy(tile.gameObject);

                string newTileName = "tile_" + movingTileValue * 2;

                score += movingTileValue * 2;

                UpdateScore();

                audioSource.PlayOneShot(mergeTilesSound);

                GameObject newTile = (GameObject)Instantiate(Resources.Load(newTileName, typeof(GameObject)), tile.transform.localPosition, Quaternion.identity);

                newTile.transform.parent = transform;

                newTile.GetComponent<Tile>().mergedThisTurn = true;

                grid[(int)newTile.transform.localPosition.x, (int)newTile.transform.localPosition.y] = newTile.transform;

                newTile.transform.localScale = new Vector2(0, 0);

                newTile.transform.localPosition = new Vector2(newTile.transform.localPosition.x + 0.5f, newTile.transform.localPosition.y + 0.5f);

                yield return StartCoroutine(NewTilePopIn(newTile, new Vector2(0, 0), new Vector2(1, 1), 10f, newTile.transform.localPosition, new Vector2(newTile.transform.localPosition.x - 0.5f, newTile.transform.localPosition.y - 0.5f)));
            }
            numberOffCoroutinesRunning--;
        }
}

