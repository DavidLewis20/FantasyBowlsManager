using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Assignment2/PlayerType", menuName = "PlayerType")]
public class SCR_PlayerTypes : ScriptableObject
{
    //affects the overall rating of the player. Platinum players have very high stats whilst bronze tend to have lower stats
    public enum PlayerLevels
    {
        BRONZE, SILVER, GOLD, PLATINUM
    }

    public enum GrowthLevels
    {
        LOW, MEDIUM, HIGH
    }

    [Header("Player strengths")]
    public bool isAccurate; //Increases accuracy rating if true

    public bool isStrong; //Increases power rating if true

    public bool isConsistent; //Increases control rating if true

    public bool isExperienced; //Increases experience rating if true

    public PlayerLevels rankingLevel; //set skill level

    public GrowthLevels growthPotential; //set growth potential
}
