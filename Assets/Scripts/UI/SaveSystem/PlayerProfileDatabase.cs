using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.UIElements;

public static class PlayerProfileDatabase
{
    private static string ProfilesFolder => Path.Combine(Application.persistentDataPath, "Profiles");

    private static void EnsureFolderExists()
    {
        if (!Directory.Exists(ProfilesFolder))
        {
            Directory.CreateDirectory(ProfilesFolder);
        }
    }

    private static string GetProfilePath(string playerName)
    {
        return Path.Combine(ProfilesFolder,$"{playerName}.json");
    }

    public static void SaveProfile(PlayerProfile profile)
    {
        EnsureFolderExists();

        string json = JsonUtility.ToJson(profile, true);

        File.WriteAllText(GetProfilePath(profile.playerName), json);
    }

    public static bool ProfileExists(string playerName)
    {
        return File.Exists(GetProfilePath(playerName));
    }

    public static PlayerProfile LoadProfile(string playerName)
    {
        string path = GetProfilePath(playerName);

        if (!File.Exists(path))
        {
            return null;
        }

        string json = File.ReadAllText(path);

        return JsonUtility.FromJson<PlayerProfile>(json);
    }
}
