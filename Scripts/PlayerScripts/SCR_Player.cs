using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable] //allows class to be saved
public class SCR_Player
{
    public SCR_PlayerTypes playerType;

    public int powerRating = 1; //Players with higher rating can make power shots travel further

    public int accuracyRating = 1; //Players with higher rating can make accuracy shots get closer to target

    public int controlRating = 1; //Players with higher rating make less errors, such as throwing the ball out of bounds

    public int experienceRating = 1; //Players with higher rating earn more prize money for the team and more likely to spot secrets

    public int cost = 1;

    public float growthRate = 1;

    public int XP = 0;

    public int statBoosts = 0;

    public string playerForename;

    public string playerSurname;

    public float overallRating; //used for player pricing

    //create a new player instance and set their stats
    public void GeneratePlayer()
    {
        powerRating = SetStat(playerType.isStrong);
        accuracyRating = SetStat(playerType.isAccurate);
        controlRating = SetStat(playerType.isConsistent);
        experienceRating = SetStat(playerType.isExperienced);
        SetGrowthRate();

        //calculate cost based on overall rating
        overallRating = ((float)powerRating + (float)accuracyRating + (float)controlRating + (float)experienceRating) / 4;
        float tempCost = Mathf.Pow(1.13f, overallRating);
        int roundedCost = Mathf.RoundToInt(tempCost);

        //set minimum price as $500, round to next 1000th if >$10k, otherwise, set price to next 10000th
        if (tempCost < 500f)
        {
            cost = 500;
        } else if(tempCost < 10000)
        {
            int remainder = roundedCost % 1000;
            cost = roundedCost + (1000 - remainder);
        }
        else
        {
            int remainder = roundedCost % 10000;
            cost = roundedCost + (10000 - remainder);
        }

    }

    //generates stats based on the player type and skill level
    int SetStat(bool specialty)
    {
        switch (playerType.rankingLevel)
        {
            case SCR_PlayerTypes.PlayerLevels.BRONZE:
                if (specialty)
                {
                    return Random.Range(61, 91);
                }
                else
                {
                    return Random.Range(30, 81);
                }
            case SCR_PlayerTypes.PlayerLevels.SILVER:
                if (specialty)
                {
                    return Random.Range(76, 96);
                }
                else
                {
                    return Random.Range(50, 86);
                }
            case SCR_PlayerTypes.PlayerLevels.GOLD:
                if (specialty)
                {
                    return Random.Range(81, 101);
                }
                else
                {
                    return Random.Range(70, 91);
                }
            case SCR_PlayerTypes.PlayerLevels.PLATINUM:
                if(specialty)
                {
                    return Random.Range(91, 101);
                }
                else
                {
                    return Random.Range(80, 101);
                }
            default:
                return Random.Range(1, 101);
        }
    }

    //sets growth rate based on their growth potential
    void SetGrowthRate()
    {
        switch (playerType.growthPotential)
        {
            case SCR_PlayerTypes.GrowthLevels.LOW:
                growthRate = Random.Range(0f, 0.8f);
                break;
            case SCR_PlayerTypes.GrowthLevels.MEDIUM:
                growthRate = Random.Range(0.8f, 1.5f);
                break;
            case SCR_PlayerTypes.GrowthLevels.HIGH:
                growthRate = Random.Range(1.5f, 2.2f);
                break;
            default:
                growthRate = 1f;
                break;
        }
    }

    public void IncreasePower()
    {
        powerRating++;
    }

    public void IncreaseAccuracy()
    {
        accuracyRating++;
    }

    public void IncreaseControl()
    {
        controlRating++;
    }

    public void IncreaseExperience()
    {
        experienceRating++;
    }

    //Increases player XP
    public void IncreaseXP(int additionalXP)
    {
        XP += additionalXP;

        //if XP is <3000, reset to 0 and award a stat boost
        if(XP > 3000)
        {
            while (XP > 3000)
            {
                statBoosts++;
                if (growthRate > 0f) //decrease growth rate, unless it reaches 0
                {
                    growthRate -= 0.01f;
                }
                XP -= 3000;
            }
        }
    }
}
