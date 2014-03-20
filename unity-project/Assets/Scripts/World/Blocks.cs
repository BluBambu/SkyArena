using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using System.Collections;

class Air
{
    public const byte ID = 0;
    public const float resistance = 0;
}

class Grass
{
    public const byte ID = 1;
    public const float resistance = 5;
}

class Dirt
{
    public const byte ID = 2;
    public const float resistance = 5;
}

class Stone
{
    public const byte ID = 3;
    public const float resistance = 10;
}

class BedRock
{
    public const byte ID = 4;
    public const float resistance = -1;
}

public class Blocks
{
    public static float GetResistance(byte ID)
    {
        switch (ID)
        {
            case Air.ID:
                return Air.resistance;
            case Grass.ID:
                return Grass.resistance;
            case Dirt.ID:
                return Dirt.resistance;
            case Stone.ID:
                return Stone.resistance;
            case BedRock.ID:
                return BedRock.resistance;
        }
        return -1;
    }
    public static string GetName(byte ID)
    {
        switch (ID)
        {
            case Air.ID:
                return "Air";
            case Grass.ID:
                return "Grass";
            case Dirt.ID:
                return "Dirt";
            case Stone.ID:
                return "Stone";
            case BedRock.ID:
                return "BedRock";
        }
        return null;
    }
}