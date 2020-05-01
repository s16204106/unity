﻿using System;

public class Player: ICloneable
{
    public int ID;
    public string playerName;
    public int Health;
    public int Gold;

    public object Clone()
    {
        return this.MemberwiseClone();
    }
}