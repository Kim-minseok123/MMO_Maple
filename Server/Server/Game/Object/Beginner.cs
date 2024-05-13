﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public class Beginner : Player
    {
        public override int Attack { get { return random.Next(MinAttack, MaxAttack + 1); } }
        public int MaxAttack { get; set; }
        public int MinAttack { get; set; }
        public Beginner() : base()
        {
            MaxAttack = (Stat.Str * 4 + Stat.Dex) * WeaponDamage / 100;
            MinAttack = (int)((Stat.Str * 4 * 0.9 * 0.1 + Stat.Dex) * WeaponDamage / 100);
            if (MinAttack < 1) MinAttack = 1;
            if (MaxAttack < 3) MaxAttack = 3;
        }
    }
}
