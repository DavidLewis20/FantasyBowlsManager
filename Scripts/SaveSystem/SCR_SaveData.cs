using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SCR_SaveData //code adapted from Beever (2023)
{
    public List<SCR_Player> playerPool;

    public List<SCR_Player> usersTeam;

    public int managerWins;

    public int managerLosses;

    public int gamesPlayed;

    public int money;

    public string managerName;

    public string lastSavedDate;

    public int gamesPlayedToday;

    //save to Json
    public string ToJson()
    {
        return JsonUtility.ToJson(this, true);
    }

    //load from Json
    public void LoadFromJson(string newJsonData)
    {
        JsonUtility.FromJsonOverwrite(newJsonData, this);
    }
    //end of adapted code
}
