using UnityEngine;

public static class CheckpointManager
{
    public static bool FirstSave { get; set; } = false;
    public static bool AtBoss { get; set; } = false;

    private static Vector3 playerPos;
    private static Quaternion playerRot;
    private static int playerHealth, playerMana;

    public static void SaveGame()
    {
        var player = Player.Instance;
        playerPos = player.transform.position;
        playerRot = player.transform.rotation;
        playerHealth = player.GetHealth();
        playerMana = player.GetMana();
    }

    public static void LoadGame()
    {
        var player = Player.Instance;
        player.transform.position = playerPos;
        player.transform.rotation = playerRot;
        player.SetHealth(playerHealth);
        player.SetMana(playerMana);
    }
}