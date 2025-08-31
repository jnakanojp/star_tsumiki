using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int width = 10;
    public int height = 20;
    public float dropInterval = 1f;

    private Transform[,] grid;
    private Piece current;
    private float dropTimer;
    private Color[] colors;

    private void Awake()
    {
        grid = new Transform[width, height];
        colors = new Color[]
        {
            Color.cyan,
            Color.yellow,
            Color.magenta,
            Color.green,
            Color.red,
            new Color(1f, 0.5f, 0f),
            Color.blue
        };
    }

    private void Start()
    {
        SpawnPiece();
        dropTimer = dropInterval;
    }

    private void Update()
    {
        if (current == null)
            return;

        HandleInput();

        dropTimer -= Time.deltaTime;
        if (dropTimer <= 0f)
        {
            Move(Vector2Int.down);
            dropTimer = dropInterval;
        }
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
            Move(Vector2Int.left);
        else if (Input.GetKeyDown(KeyCode.RightArrow))
            Move(Vector2Int.right);

        if (Input.GetKeyDown(KeyCode.DownArrow))
            Move(Vector2Int.down);

        if (Input.GetKeyDown(KeyCode.UpArrow))
            Rotate();
    }

    private void SpawnPiece()
    {
        current = new Piece();
        int shapeIndex = Random.Range(0, Piece.Shapes.Length);
        current.Initialize(shapeIndex, this);

        if (!IsValidPosition(current, current.position))
        {
            Debug.Log("Game Over");
            enabled = false;
        }
    }

    private bool Move(Vector2Int direction)
    {
        Vector2Int newPos = current.position + direction;
        if (IsValidPosition(current, newPos))
        {
            current.position = newPos;
            current.UpdateBlocks();
            return true;
        }
        else if (direction == Vector2Int.down)
        {
            SetPiece();
        }
        return false;
    }

    private void Rotate()
    {
        current.Rotate();
        if (!IsValidPosition(current, current.position))
            current.RotateBack();
        else
            current.UpdateBlocks();
    }

    private bool IsValidPosition(Piece piece, Vector2Int pos)
    {
        foreach (Vector2Int cell in piece.Cells)
        {
            Vector2Int coord = cell + pos;
            if (coord.x < 0 || coord.x >= width || coord.y < 0 || coord.y >= height)
                return false;
            if (grid[coord.x, coord.y] != null)
                return false;
        }
        return true;
    }

    private void SetPiece()
    {
        foreach (Transform block in current.blocks)
        {
            Vector2Int coord = Vector2Int.RoundToInt(block.position);
            grid[coord.x, coord.y] = block;
        }
        ClearLines();
        SpawnPiece();
    }

    private void ClearLines()
    {
        for (int y = height - 1; y >= 0; y--)
        {
            if (IsLineFull(y))
            {
                ClearLine(y);
                MoveDownLinesAbove(y);
                y++;
            }
        }
    }

    private bool IsLineFull(int y)
    {
        for (int x = 0; x < width; x++)
        {
            if (grid[x, y] == null)
                return false;
        }
        return true;
    }

    private void ClearLine(int y)
    {
        for (int x = 0; x < width; x++)
        {
            Destroy(grid[x, y].gameObject);
            grid[x, y] = null;
        }
    }

    private void MoveDownLinesAbove(int y)
    {
        for (int i = y + 1; i < height; i++)
        {
            for (int x = 0; x < width; x++)
            {
                Transform t = grid[x, i];
                if (t != null)
                {
                    grid[x, i - 1] = t;
                    grid[x, i] = null;
                    t.position += Vector3.down;
                }
            }
        }
    }

    public class Piece
    {
        public static readonly Vector2Int[][] Shapes = new Vector2Int[][]
        {
            new [] { new Vector2Int(-1,0), new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(2,0) }, // I
            new [] { new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(0,1), new Vector2Int(1,1) }, // O
            new [] { new Vector2Int(-1,0), new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(0,1) }, // T
            new [] { new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(-1,1), new Vector2Int(0,1) }, // S
            new [] { new Vector2Int(-1,0), new Vector2Int(0,0), new Vector2Int(0,1), new Vector2Int(1,1) }, // Z
            new [] { new Vector2Int(-1,0), new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(-1,1) }, // J
            new [] { new Vector2Int(-1,0), new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(1,1) }  // L
        };

        public Vector2Int position;
        public Transform[] blocks;
        public int rotation;
        private int shape;
        private GameManager board;

        public Vector2Int[] Cells
        {
            get
            {
                Vector2Int[] cells = new Vector2Int[4];
                for (int i = 0; i < 4; i++)
                    cells[i] = RotateCell(Shapes[shape][i], rotation);
                return cells;
            }
        }

        public void Initialize(int shapeIndex, GameManager game)
        {
            shape = shapeIndex;
            board = game;
            rotation = 0;
            position = new Vector2Int(board.width / 2, board.height - 1);
            blocks = new Transform[4];
            Color color = board.colors[shapeIndex];
            for (int i = 0; i < 4; i++)
            {
                GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
                block.transform.localScale = Vector3.one;
                block.GetComponent<Renderer>().material.color = color;
                blocks[i] = block.transform;
            }
            UpdateBlocks();
        }

        public void UpdateBlocks()
        {
            Vector2Int[] cells = Cells;
            for (int i = 0; i < blocks.Length; i++)
            {
                Vector2Int cell = cells[i] + position;
                blocks[i].position = new Vector3(cell.x, cell.y, 0);
            }
        }

        public void Rotate()
        {
            rotation = (rotation + 1) % 4;
        }

        public void RotateBack()
        {
            rotation = (rotation + 3) % 4;
        }

        private Vector2Int RotateCell(Vector2Int cell, int rot)
        {
            switch (rot)
            {
                case 0: return cell;
                case 1: return new Vector2Int(-cell.y, cell.x);
                case 2: return new Vector2Int(-cell.x, -cell.y);
                case 3: return new Vector2Int(cell.y, -cell.x);
                default: return cell;
            }
        }
    }
}
