using System;

namespace SimpleRoguelike
{
    class Program
    {
        // Initialize the grid to the size of the console window
        static char[,] grid = new char[Console.WindowWidth, Console.WindowHeight];
        static bool[,] visibilityGrid = new bool[Console.WindowWidth, Console.WindowHeight];
        static int playerX = 10;
        static int playerY = 10;
        static int startX, startY;
        static int visibilityRadius = 5;

        // Create a single instance of Random with a seed value based on the current time in ticks
        static Random random = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);



        static void Main(string[] args)
        {
            // Set the console window's buffer height to be equal to its window height
            Console.BufferHeight = Console.WindowHeight;


            ConsoleKeyInfo keyInfo;
            InitializeGrid();
            Console.CursorVisible = false;


            startX = (Console.WindowWidth - grid.GetLength(0)) / 2;
            startY = (Console.WindowHeight - grid.GetLength(1)) / 2;

            MovePlayer(0, 0);  // Update the visibility grid

            do
            {
                PrintGrid();
                keyInfo = Console.ReadKey(true);

                switch (keyInfo.Key)
                {
                    case ConsoleKey.UpArrow:
                        MovePlayer(0, -1);
                        break;
                    case ConsoleKey.DownArrow:
                        MovePlayer(0, 1);
                        break;
                    case ConsoleKey.LeftArrow:
                        MovePlayer(-1, 0);
                        break;
                    case ConsoleKey.RightArrow:
                        MovePlayer(1, 0);
                        break;
                }
            }
            while (keyInfo.Key != ConsoleKey.Escape);
        }

        static void CreateRooms(int numberOfRooms)
        {
            // Create a list to store the center point of each room
            List<Tuple<int, int>> roomCenters = new List<Tuple<int, int>>();

            for (int i = 0; i < numberOfRooms; i++)
            {
                int roomX, roomY, roomWidth, roomHeight;

                do
                {
                    // Generate random position and size for the room
                    roomX = random.Next(1, grid.GetLength(0) - 10);
                    roomY = random.Next(1, grid.GetLength(1) - 10);
                    roomWidth = random.Next(5, 10);
                    roomHeight = random.Next(5, 10);
                }
                while (DoesRoomOverlap(roomX, roomY, roomWidth, roomHeight));

                // Create the room
                CreateRoom(roomX, roomY, roomWidth, roomHeight);

                // Store the center point of the room
                roomCenters.Add(Tuple.Create(roomX + roomWidth / 2, roomY + roomHeight / 2));
            }

            // Set the player's initial position to be within the first room
            playerX = roomCenters[0].Item1;
            playerY = roomCenters[0].Item2;

            // Create tunnels between the rooms
            for (int i = 0; i < roomCenters.Count - 1; i++)
            {
                CreateTunnel(roomCenters[i].Item1, roomCenters[i].Item2, roomCenters[i + 1].Item1, roomCenters[i + 1].Item2);
            }
        }


        static void CreateRoom(int x, int y, int width, int height)
        {
            for (int i = y; i < y + height; i++)
            {
                for (int j = x; j < x + width; j++)
                {
                    grid[j, i] = '.';
                }
            }
        }

        static void CreateTunnel(int x1, int y1, int x2, int y2)
        {
            // Create a horizontal tunnel from x1 to x2
            for (int x = Math.Min(x1, x2); x <= Math.Max(x1, x2); x++)
            {
                grid[x, y1] = '.';
            }

            // Create a vertical tunnel from y1 to y2
            for (int y = Math.Min(y1, y2); y <= Math.Max(y1, y2); y++)
            {
                grid[x2, y] = '.';
            }
        }



        static void InitializeGrid()
        {


            for (int y = 0; y < grid.GetLength(1); y++)
            {
                for (int x = 0; x < grid.GetLength(0); x++)
                {
                    grid[x, y] = '#';
                }
            }

            /*
             // Create two random rooms
             int room1X = random.Next(grid.GetLength(0) / 4, 3 * grid.GetLength(0) / 4);
             int room1Y = random.Next(grid.GetLength(1) / 4, 3 * grid.GetLength(1) / 4);

             int room1Width = random.Next(5, Math.Min(10, grid.GetLength(0) / 2));
             int room1Height = random.Next(5, Math.Min(10, grid.GetLength(1) / 2));

             int room2X = random.Next(grid.GetLength(0) / 4, 3 * grid.GetLength(0) / 4);
             int room2Y = random.Next(grid.GetLength(1) / 4, 3 * grid.GetLength(1) / 4);

             int room2Width = random.Next(5, Math.Min(10, grid.GetLength(0) - room2X));
             int room2Height = random.Next(5, Math.Min(10, grid.GetLength(1) - room2Y));

             CreateRoom(room1X, room1Y, room1Width, room1Height);
             CreateRoom(room2X, room2Y, room2Width, room2Height);

             // Set the player's initial position to be within the first room
             playerX = room1X + room1Width / 2;
             playerY = room1Y + room1Height / 2;

             // Create a tunnel between the rooms
             int tunnelStartX = room1X + room1Width / 2;
             int tunnelStartY = room1Y + room1Height / 2;
             int tunnelEndX = room2X + room2Width / 2;
             int tunnelEndY = room2Y + room2Height / 2;

             CreateTunnel(tunnelStartX, tunnelStartY, tunnelEndX, tunnelEndY);
            */

            // Create a number of rooms
            CreateRooms(6);

            grid[playerX, playerY] = '@';
        }

        static bool DoesRoomOverlap(int x, int y, int width, int height)
        {
            for (int i = y; i < y + height; i++)
            {
                for (int j = x; j < x + width; j++)
                {
                    // If the proposed room would overlap with an existing room or tunnel, return true
                    if (grid[j, i] == '.')
                    {
                        return true;
                    }
                }
            }

            // If the proposed room does not overlap with any existing rooms or tunnels, return false
            return false;
        }


        static void PrintGrid()
        {
            Console.SetCursorPosition(0, 0);

            for (int y = 0; y < grid.GetLength(1); y++)
            {
                for (int x = 0; x < grid.GetLength(0); x++)
                {
                    if (visibilityGrid[x, y])
                    {
                        Console.Write(grid[x, y]);
                    }
                    else
                    {
                        Console.Write(' ');
                    }
                }
            }
        }

        static void UpdateVisibility()
        {
            for (int y = Math.Max(0, playerY - visibilityRadius); y <= Math.Min(grid.GetLength(1) - 1, playerY + visibilityRadius); y++)
            {
                for (int x = Math.Max(0, playerX - visibilityRadius); x <= Math.Min(grid.GetLength(0) - 1, playerX + visibilityRadius); x++)
                {
                    foreach (var point in BresenhamLine(playerX, playerY, x, y))
                    {
                        visibilityGrid[point.Item1, point.Item2] = true;
                        if (grid[point.Item1, point.Item2] == '#')
                        {
                            break;
                        }
                    }
                }
            }
        }

        static IEnumerable<Tuple<int, int>> BresenhamLine(int x0, int y0, int x1, int y1)
        {
            int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = -Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int err = dx + dy, e2;

            while (true)
            {
                yield return Tuple.Create(x0, y0);
                if (x0 == x1 && y0 == y1) break;
                e2 = 2 * err;
                if (e2 >= dy) { err += dy; x0 += sx; }
                if (e2 <= dx) { err += dx; y0 += sy; }
            }
        }



        static void MovePlayer(int dx, int dy)
        {
            int newX = playerX + dx;
            int newY = playerY + dy;

            // Check if the new position is a wall
            if (grid[newX, newY] != '#')
            {
                // Clear the old player position
                grid[playerX, playerY] = '.';

                // Update the player position
                playerX = newX;
                playerY = newY;

                // Draw the player at the new position
                grid[playerX, playerY] = '@';

                // Update the visibility grid

                UpdateVisibility();

              
            }
        }








    }
}
