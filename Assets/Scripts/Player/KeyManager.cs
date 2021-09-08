using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Keeps track of keys, automatically added by Player. Do NOT add manually.
/// </summary>
public static class KeyManager
{
    private static readonly Dictionary<KeyColor, bool> obtained = new Dictionary<KeyColor, bool>();

    public static void AddKey(KeyColor keyColor)
    {
        if (HasKey(keyColor))
        {
            // throw new System.ArgumentException($"'{keyColor}' has already been added to 'KeyManager'");
        }

        obtained[keyColor] = true;
    }

    public static bool HasKey(KeyColor keyColor)
    {
        return obtained.ContainsKey(keyColor) && obtained[keyColor];
    }
}


/// <summary>
/// Enum for all keys, add as necessary.
/// </summary>
public enum KeyColor
{
    MAIN_DOOR
}
