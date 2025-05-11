
using System.Data;



class Program
{

    static readonly char[] keys_char = Enumerable.Range('a', 26).Select(i => (char)i).ToArray();
    static readonly char[] doors_char = keys_char.Select(char.ToUpper).ToArray();
    static readonly (int dx, int dy)[] Direct = { (-1, 0), (1, 0), (0, -1), (0, 1) };
    static int rows, columns;
    static List<Point>[] maze;
    static Dictionary<char, int> keysIndex;



    static List<List<char>> GetInput()
    {
        var data = new List<List<char>>();
        string line;
        while ((line = Console.ReadLine()) != null && line != "")
        {
            data.Add(line.ToCharArray().ToList());
        }
        return data;
    }


    static int Solve(List<List<char>> data)
    {
        rows = data.Count;
        columns = data[0].Count;
        var robotPosition = new List<(int, int)>(4);
        var keyPosition = new Dictionary<char, (int, int)>();
        for (var i = 0; i < rows; i++)
        {
            for (var j = 0; j < columns; j++)
            {
                var pos = data[i][j];
                if (pos is '@')
                {
                    robotPosition.Add((i, j));
                    data[i][j] = '.';
                }
                if (pos >= 'a' && pos <= 'z')
                {
                    keyPosition[pos] = (i, j);
                }
            }
        }



        var countKeys = keyPosition.Count();
        keysIndex = keyPosition.Keys
            .OrderBy(c => c)
            .Select((c, v) => new { c, v })
            .ToDictionary(i => i.c, i => i.v);

        var keyMask = (1 << countKeys) - 1;
        var nodes = robotPosition.Count + countKeys;
        maze = new List<Point>[nodes];

        for (int i = 0; i < nodes; i++)
        {
            maze[i] = new List<Point>();
        }

        for (var i = 0; i < robotPosition.Count; i++)
        {
            BFS(data, i, robotPosition[i], robotPosition);
        }

        foreach (var (key, value) in keyPosition)
        {
            int keyNode = robotPosition.Count + keysIndex[key];
            BFS(data, keyNode, value, robotPosition);
        }
        return Dijkstra(robotPosition, keyMask);



    }

    private static void BFS(List<List<char>> data, int robot, (int row, int column) source, List<(int r, int c)> starts)
    {
        var queue = new Queue<(int i, int j, int dist, int mask)>();
        var visited = new bool[rows, columns];
        queue.Enqueue((source.row, source.column, 0, 0));
        visited[source.row, source.column] = true;


        while (queue.Count > 0)
        {
            var currentNode = queue.Dequeue();
            foreach (var dir in Direct)
            {

                int newRow = currentNode.i + dir.dx;
                int newColumn = currentNode.j + dir.dy;
                if (OutOfBounds(newRow, newColumn)) continue;
                if (visited[newRow, newColumn]) continue;
                char point = data[newRow][newColumn];

                if (point == '#') continue;
                int newMask = currentNode.mask;

                if (point >= 'A' && point <= 'Z')
                {
                    int doorIndex = Array.IndexOf(doors_char, point);
                    if (doorIndex >= 0) newMask |= (1 << doorIndex);
                }
                visited[newRow, newColumn] = true;

                if (IsKey(point))
                {
                    int keyNode = starts.Count() + keysIndex[point];
                    maze[robot].Add(new Point
                    {
                        NextNode = keyNode,
                        Distance = currentNode.dist + 1,
                        KeyMask = newMask
                    });
                }
                queue.Enqueue((newRow, newColumn, currentNode.dist + 1, newMask));
            }
        }


    }

    private static int Dijkstra(List<(int r, int c)> robotPositions, int allKeysMask)
    {
        var queue = new PriorityQueue<MazeState, int>();
        var bestDistances = new Dictionary<string, int>();

        var initialState = new MazeState
        {
            Position = Enumerable.Range(0, robotPositions.Count).ToArray(),
            Keys = 0
        };

        string initialStateKey = initialState.Encode();
        bestDistances[initialStateKey] = 0;
        queue.Enqueue(initialState, 0);

        while (queue.Count > 0)
        {
            queue.TryDequeue(out MazeState currentState, out int currentDist);

            if (currentState.Keys == allKeysMask)
                return currentDist;

            string stateKey = currentState.Encode();
            if (currentDist > bestDistances.GetValueOrDefault(stateKey, int.MaxValue))
                continue;

            // Try moving each robot
            for (int robotIndex = 0; robotIndex < robotPositions.Count; robotIndex++)
            {
                int currentNode = currentState.Position[robotIndex];

                foreach (var edge in maze[currentNode])
                {
                    int keyBit = 1 << (edge.NextNode - robotPositions.Count);

                    // Skip if we already have this key
                    if ((currentState.Keys & keyBit) != 0)
                        continue;

                    // Skip if we don't have required keys
                    if ((edge.KeyMask & currentState.Keys) != edge.KeyMask)
                        continue;

                    // Create new state
                    var newNodes = (int[])currentState.Position.Clone();
                    newNodes[robotIndex] = edge.NextNode;

                    var newState = new MazeState
                    {
                        Position = newNodes,
                        Keys = currentState.Keys | keyBit
                    };

                    int newDist = currentDist + edge.Distance;
                    string newStateKey = newState.Encode();

                    if (bestDistances.TryGetValue(newStateKey, out int existingDist) && newDist >= existingDist)
                        continue;

                    bestDistances[newStateKey] = newDist;
                    queue.Enqueue(newState, newDist);
                }
            }
        }

        return -1;
    }

    private static bool OutOfBounds(int row, int column)
    {
        var check = row < 0 || row >= rows || column < 0 || column >= columns;
        return check;
    }

    private static bool IsKey(char point)
    {
        var check = point >= 'a' && point <= 'z';
        return check;
    }


    static void Main()
    {
        var data = GetInput();
        int result = Solve(data);

        if (result == -1)
        {
            Console.WriteLine("No solution found");
        }
        else
        {
            Console.WriteLine(result);
        }
    }
}

class Point
{
    public int NextNode { get; set; }
    public int Distance { get; set; }
    public int KeyMask { get; set; }
}

class MazeState : IEquatable<MazeState>
{
    public int[] Position { get; set; }
    public int Keys { get; set; }

    public bool Equals(MazeState? obj)
    {
        if (obj is null) return false;
        if (!(this.GetHashCode() == obj.GetHashCode())) return false;
        return this.Position.Equals(obj.Position) && this.Keys == obj.Keys;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as MazeState);
    }

    public override int GetHashCode()
    {
        return Encode().GetHashCode();
    }

    public string Encode()
    {
        var returned = string.Join(',', Position) + "|" + Keys;
        return returned;
    }

}