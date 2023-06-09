﻿using Roguelike_WolframGPT;
using System;
using System.Numerics;

namespace SimpleRoguelike
{
    class Program
    {
        // Initialize the grid to the size of the console window
        static char[,] grid = new char[Console.WindowWidth, Console.WindowHeight];
        static bool[,] visibilityGrid = new bool[Console.WindowWidth, Console.WindowHeight];
        static int playerX = 10;
        static int playerY = 10;

        // Initialize the player object
        public static Player player = new Player(playerX, playerY, 100, 10, 5);

        static int startX, startY;
        static int visibilityRadius = 5;

        // Create a list to store the console messages
        static List<string> consoleMessages = new List<string>();

        // Create a single instance of Random with a seed value based on the current time in ticks
        static Random random = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);

        //static List<Tuple<int, int>> enemies = new List<Tuple<int, int>>();
        static List<Enemy> enemies = new List<Enemy>();

        static void Main(string[] args)
        {
            // Set the console window's buffer height to be equal to its window height
            Console.BufferHeight = Console.WindowHeight;

            InitializeGrid();
            Console.CursorVisible = false;

            startX = (Console.WindowWidth - grid.GetLength(0)) / 2;
            startY = (Console.WindowHeight - grid.GetLength(1)) / 2;

            MovePlayer(0, 0);  // Update the visibility grid

            PrintToConsole("Hello World!");

            ConsoleKeyInfo keyInfo;
            do
            {
                PrintGrid();

                // Clear the input buffer
                while (Console.KeyAvailable)
                {
                    Console.ReadKey(true);
                }

                // Read the next key press
                keyInfo = Console.ReadKey(true);

                // Process the key press
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
                    // roomX = random.Next(1, grid.GetLength(0) - 10);
                    // roomY = random.Next(1, grid.GetLength(1) - 10);

                    roomWidth = random.Next(5, 20);
                    roomHeight = random.Next(5, 8);
                    
                    roomX = random.Next(1, grid.GetLength(0) - roomWidth - 1);
                    roomY = random.Next(1, grid.GetLength(1) - roomHeight - 1);


                  
                }
                while (DoesRoomOverlap(roomX, roomY, roomWidth, roomHeight));

                // Create the room
                CreateRoom(roomX, roomY, roomWidth, roomHeight);

                // Store the center point of the room
                int centerX = roomX + roomWidth / 2;
                int centerY = roomY + roomHeight / 2;
                roomCenters.Add(Tuple.Create(centerX, centerY));

                // Spawn an enemy in the center of the room, except for the first room
                if (i > 0)
                {
                    SpawnEnemy(centerX, centerY);
                }

                // If this is not the first room, create a tunnel to a random existing room
                if (i > 0)
                {
                    int otherRoomIndex = random.Next(i); // Random index of an existing room
                    CreateTunnel(roomCenters[i].Item1, roomCenters[i].Item2, roomCenters[otherRoomIndex].Item1, roomCenters[otherRoomIndex].Item2);
                }
            }

            // Set the player's initial position to be within the first room
            playerX = roomCenters[0].Item1;
            playerY = roomCenters[0].Item2;
        }




        static void CreateRoom(int x, int y, int width, int height)
        {
            // Draw the floor of the room
            for (int i = y + 1; i < y + height - 1; i++)
            {
                for (int j = x + 1; j < x + width - 1; j++)
                {
                    grid[j, i] = '.';
                }
            }

            // Draw the walls of the room
            for (int i = y; i < y + height; i++)
            {
                for (int j = x; j < x + width; j++)
                {
                    if (i == y || i == y + height - 1 || j == x || j == x + width - 1)
                    {
                        grid[j, i] = '#';
                    }
                }
            }
        }


        static void CreateTunnel(int startX, int startY, int endX, int endY)
        {
            // Draw the floor of the tunnel
            for (int x = Math.Min(startX, endX); x <= Math.Max(startX, endX); x++)
            {
                grid[x, startY] = '.';
            }
            for (int y = Math.Min(startY, endY); y <= Math.Max(startY, endY); y++)
            {
                grid[endX, y] = '.';
            }

            // Draw the walls of the tunnel
            for (int x = Math.Min(startX, endX) - 1; x <= Math.Max(startX, endX) + 1; x++)
            {
                for (int y = startY - 1; y <= startY + 1; y++)
                {
                    if (x >= 0 && x < grid.GetLength(0) && y >= 0 && y < grid.GetLength(1) && grid[x, y] != '.')
                    {
                        grid[x, y] = '#';
                    }
                }
            }
            for (int y = Math.Min(startY, endY) - 1; y <= Math.Max(startY, endY) + 1; y++)
            {
                for (int x = endX - 1; x <= endX + 1; x++)
                {
                    if (x >= 0 && x < grid.GetLength(0) && y >= 0 && y < grid.GetLength(1) && grid[x, y] != '.')
                    {
                        grid[x, y] = '#';
                    }
                }
            }
        }




        static void InitializeGrid()
        {


            for (int y = 0; y < grid.GetLength(1) - 4; y++)
            {
                for (int x = 0; x < grid.GetLength(0); x++)
                {
                    grid[x, y] = ' ';
                }
            }

            // Fill the game console area with spaces
            for (int y = grid.GetLength(1) - 4; y < grid.GetLength(1); y++)
            {
                for (int x = 0; x < grid.GetLength(0); x++)
                {
                    grid[x, y] = ' ';
                }
            }

            // Create the border of the game console
            for (int x = 0; x < grid.GetLength(0); x++)
            {
                grid[x, grid.GetLength(1) - 4] = '-';
            }

            // Create a number of rooms
            CreateRooms(8);

           

            grid[player.X, player.Y] = '@';
        }

        static void PrintToConsole(string message)
        {
            // Add the new message to the list
            consoleMessages.Add(message);

            // Keep only the last 3 messages to fit in the console
            while (consoleMessages.Count > 3)
            {
                consoleMessages.RemoveAt(0);
            }

            // Print the messages to the console
            for (int i = 0; i < consoleMessages.Count; i++)
            {
                Console.SetCursorPosition(0, grid.GetLength(1) - 3 + i);
                Console.Write(consoleMessages[i].PadRight(grid.GetLength(0)));
            }
        }

        static bool DoesRoomOverlap(int x, int y, int width, int height)
        {
            for (int i = y; i < y + height; i++)
            {
                for (int j = x; j < x + width; j++)
                {
                    // If the proposed room would overlap with an existing room or tunnel, return true
                    if (grid[j, i] == '.' || grid[j, i] == '#' || grid[j, i] == '&')
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
            Console.SetCursorPosition(startX, startY);
            for (int y = 0; y < grid.GetLength(1) - 4; y++)
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

            // Draw the enemies
            foreach (var enemy in enemies)
            {
                if (visibilityGrid[enemy.X, enemy.Y])
                {
                    Console.SetCursorPosition(startX + enemy.X, startY + enemy.Y);
                    Console.Write('&');
                }
            }


            // Draw the border for the console
            for (int x = 0; x < grid.GetLength(0); x++)
            {
                Console.SetCursorPosition(startX + x, startY + grid.GetLength(1) - 4);
                Console.Write('-');
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

            // Check if the new position is a wall or an enemy
            if (grid[newX, newY] != '#' && grid[newX, newY] != '&')
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

                // Redraw the enemies
                foreach (var enemy in enemies)
                {
                    grid[enemy.X, enemy.Y] = '&';
                }

                // Make the game console always visible
                for (int y = grid.GetLength(1) - 4; y < grid.GetLength(1); y++)
                {
                    for (int x = 0; x < grid.GetLength(0); x++)
                    {
                        visibilityGrid[x, y] = true;
                    }
                }
            }
            else if (grid[newX, newY] == '&')
            {
                // Print a message to the game console if the player tries to move into an enemy
                PrintToConsole("You cannot move into an enemy!");
            }
        }



        static void SpawnEnemy(int x, int y)
        {
            // Create a new enemy with random stats
            Enemy enemy = new Enemy(x, y, random.Next(10, 20), random.Next(1, 5), random.Next(1, 5));

            // Add the enemy to the list of enemies
            enemies.Add(enemy);

            // Draw the enemy on the grid
            grid[x, y] = '&';
        }






    }
}
