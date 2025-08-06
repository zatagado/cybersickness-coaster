using System.IO;
using UnityEngine;

public static class CybersickData
{
    public static string PID { get; private set; }
    public static int Session { get; private set; }

    public static void LoadData()
    {
        string path = Application.dataPath + "/cybersickness.txt";
        // string path = Application.dataPath + "/cybersickness.txt"; // Used for when the builds are sent out.
        if (File.Exists(path))
        {
            string[] lines = File.ReadAllLines(path);

            PID = lines[1].Substring(4);
            try
            {
                Session = int.Parse(lines[2].Substring(8));
            }
            catch
            {
                throw new System.Exception("Encountered exception when trying to parse PID. Make sure that the client has a PID.");
            }
        }
        else
        {
            Debug.LogError($"Could not find save file. Should be in path {Application.dataPath}.");
        }
    }

    public static void SaveData()
    {
        // disable when session finished
        string path = Application.dataPath + "/cybersickness.txt";
        // string path = Application.dataPath + "/cybersickness.txt"; // Used for when the builds are sent out.
        if (File.Exists(path))
        {
            string[] lines = File.ReadAllLines(path);

            PID = lines[1].Substring(4);
            try
            {
                Session = int.Parse(lines[2].Substring(8));
            }
            catch
            {
                throw new System.Exception("Encountered exception when trying to parse PID. Make sure that the client has a PID.");
            }
        }
        else
        {
            Debug.Log("Could not find save file.");
        }
    }

    /*
    public static bool CheckIfSessionCompleted()
    {
        string path = Application.persistentDataPath + "/cybersickness.txt";
        // string path = Application.dataPath + "/cybersickness.txt"; // Used for when the builds are sent out.
        if (File.Exists(path))
        {
            string[] lines = File.ReadAllLines(path);

            return lines[1].Equals("CURRENT SESSION COMPLETE");
        }
        else
        {
            Debug.Log("Could not find save file. Creating new file.");
            File.WriteAllText(path, "===> DO NOT EDIT <===\nPID=\nSession=\n");
            return false;
        }
    }

    public static void MarkSessionCompleted()
    {
        string path = Application.persistentDataPath + "/cybersickness.txt";
        // string path = Application.dataPath + "/cybersickness.txt"; // Used for when the builds are sent out.
        File.WriteAllText(path, "===> DO NOT EDIT <===\nCURRENT SESSION COMPLETE\n");
    }
    */
}
