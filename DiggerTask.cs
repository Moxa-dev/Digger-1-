using System;
using Avalonia.Input;
using Digger.Architecture;

namespace Digger
{
    public class Terrain : ICreature
    {
        public CreatureCommand Act(int x, int y)
        {
            return new CreatureCommand { DeltaX = 0, DeltaY = 0 };
        }

        public bool DeadInConflict(ICreature conflictedObject)
        {
            return true;
        }

        public int GetDrawingPriority()
        {
            return 0;
        }

        public string GetImageFileName()
        {
            return "Terrain.png";
        }
    }

    public class Player : ICreature
    {
        public static int X, Y;

        public CreatureCommand Act(int x, int y)
        {
            X = x;
            Y = y;
            var command = new CreatureCommand { DeltaX = 0, DeltaY = 0 };
            
            if (Game.KeyPressed == Key.Left && x > 0) command.DeltaX = -1;
            if (Game.KeyPressed == Key.Right && x < Game.MapWidth - 1) command.DeltaX = 1;
            if (Game.KeyPressed == Key.Up && y > 0) command.DeltaY = -1;
            if (Game.KeyPressed == Key.Down && y < Game.MapHeight - 1) command.DeltaY = 1;
            
            return command;
        }

        public bool DeadInConflict(ICreature conflictedObject)
        {
            return conflictedObject is Monster || conflictedObject is Sack;
        }

        public int GetDrawingPriority()
        {
            return 1;
        }

        public string GetImageFileName()
        {
            return "Digger.png";
        }
    }

    public class Gold : ICreature
    {
        public CreatureCommand Act(int x, int y)
        {
            return new CreatureCommand { DeltaX = 0, DeltaY = 0 };
        }

        public bool DeadInConflict(ICreature conflictedObject)
        {
            if (conflictedObject is Player)
            {
                Game.Scores += 10;
                return true;
            }
            return conflictedObject is Monster;
        }

        public int GetDrawingPriority()
        {
            return 3;
        }

        public string GetImageFileName()
        {
            return "Gold.png";
        }
    }

    public class Sack : ICreature
    {
        private int falling;

        public CreatureCommand Act(int x, int y)
        {
            if (y + 1 >= Game.MapHeight)
                return new CreatureCommand { DeltaX = 0, DeltaY = 0 };

            ICreature? cellBelow = Game.Map[x, y + 1];
            
            if (cellBelow == null || (falling > 0 && cellBelow is Player))
            {
                falling++;
                return new CreatureCommand { DeltaX = 0, DeltaY = 1 };
            }

            if (falling > 1)
                return new CreatureCommand { TransformTo = new Gold() };

            falling = 0;
            return new CreatureCommand { DeltaX = 0, DeltaY = 0 };
        }

        public bool DeadInConflict(ICreature conflictedObject)
        {
            return false;
        }

        public int GetDrawingPriority()
        {
            return 2;
        }

        public string GetImageFileName()
        {
            return "Sack.png";
        }
    }

    public class Monster : ICreature
    {
        public CreatureCommand Act(int x, int y)
        {
            // If there's no player on the map, stay still
            if (!HasPlayerOnMap())
                return new CreatureCommand { DeltaX = 0, DeltaY = 0 };

            // Calculate movement direction towards player
            int dx = Math.Sign(Player.X - x);
            int dy = Math.Sign(Player.Y - y);

            // Try horizontal movement first
            if (dx != 0 && CanMove(x + dx, y))
                return new CreatureCommand { DeltaX = dx, DeltaY = 0 };

            // If horizontal movement is not possible, try vertical
            if (dy != 0 && CanMove(x, y + dy))
                return new CreatureCommand { DeltaX = 0, DeltaY = dy };

            // If no movement is possible, stay still
            return new CreatureCommand { DeltaX = 0, DeltaY = 0 };
        }

        private bool HasPlayerOnMap()
        {
            for (int x = 0; x < Game.MapWidth; x++)
                for (int y = 0; y < Game.MapHeight; y++)
                    if (Game.Map[x, y] is Player)
                        return true;
            return false;
        }

        private bool CanMove(int x, int y)
        {
            // Check boundaries
            if (x < 0 || y < 0 || x >= Game.MapWidth || y >= Game.MapHeight)
                return false;

            ICreature? creature = Game.Map[x, y];
            
            // Monster can move to empty space, player location, or gold
            return creature == null || 
                   creature is Player || 
                   creature is Gold;
        }

        public bool DeadInConflict(ICreature conflictedObject)
        {
            return conflictedObject is Monster || conflictedObject is Sack;
        }

        public int GetDrawingPriority()
        {
            return 2;
        }

        public string GetImageFileName()
        {
            return "Monster.png";
        }
    }
}