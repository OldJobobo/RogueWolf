using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike_WolframGPT
{
    public class Player
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Health { get; set; }
        public int AttackPower { get; set; }
        public int Defense { get; set; }

        public Player(int x, int y, int health, int attackPower, int defense)
        {
            X = x;
            Y = y;
            Health = health;
            AttackPower = attackPower;
            Defense = defense;
        }
    }

}
