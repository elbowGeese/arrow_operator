using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public static class Leaderboard
{
    public static List<LeaderEntry> GetTopSixEntries()
    {
        // get existing data
        string leaderString = PlayerPrefs.GetString("Leaderboard");
        LeaderWrapper leaderWrapped = JsonUtility.FromJson<LeaderWrapper>(leaderString);
        List<LeaderEntry> topSixEntries = new List<LeaderEntry>();
        if (leaderWrapped != null)
        {
            for (int i = 0; i < 6; i++)
            {
                if (leaderWrapped.entries.Count > i)
                {
                    topSixEntries.Add(leaderWrapped.entries[i]);
                }
            }
        }

        return topSixEntries;
    }

    public static void AddToLeaderboard(string new_name, int new_score)
    {
        // get existing data
        string leaderString = PlayerPrefs.GetString("Leaderboard");
        LeaderWrapper leaderWrapped = JsonUtility.FromJson<LeaderWrapper>(leaderString);

        // add new score to existing data
        bool inserted = false;
        if (leaderWrapped != null)
        {
            for (int i = 0; i < leaderWrapped.entries.Count; i++)
            {
                // check if new score is bigger
                if (CompareScores(new_score, leaderWrapped.entries[i].score))
                {
                    leaderWrapped.entries.Insert(i, new LeaderEntry() { name = new_name, score = new_score });
                    inserted = true;
                    break;
                }
            }
            if (!inserted)
            {
                leaderWrapped.entries.Add(new LeaderEntry() { name = new_name, score = new_score });
            }
        }
        else
        {
            leaderWrapped = new LeaderWrapper();
            leaderWrapped.entries = new List<LeaderEntry>();
            leaderWrapped.entries.Add(new LeaderEntry() { name = new_name, score = new_score });
        }

        // save data
        PlayerPrefs.SetString("Leaderboard", JsonUtility.ToJson(leaderWrapped));
    }

    private static bool CompareScores(int score1, int score2)
    {
        return score1 > score2;
    }
}

[System.Serializable]
public class LeaderEntry
{
    public string name;
    public int score;
}

[System.Serializable]
public class LeaderWrapper
{
    public List<LeaderEntry> entries;
}