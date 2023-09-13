using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//No longer in use. Replaced by SCR_PlayerTypes
[CreateAssetMenu(fileName = "Assignment2/Player", menuName = "Player")]
public class SCR_PlayerStats : ScriptableObject
{
    public int powerRating = 1; //Players with higher rating can make power shots travel further

    public int accuracyRating = 1; //Players with higher rating can make accuracy shots get closer to target

    public int controlRating = 1; //Players with higher rating make less errors, such as throwing the ball out of bounds

    public int experienceRating = 1; //Players with higher rating earn more money for team after winning

    public int Power
    {
        get
        {
            return Mathf.Clamp(powerRating, 0, 100);
        }
    }

    public int Accuracy
    {
        get
        {
            return Mathf.Clamp(accuracyRating, 0, 100);
        }
    }

    public int Contol
    {
        get
        {
            return Mathf.Clamp(controlRating, 0, 100);
        }
    }

    public int Experience
    {
        get
        {
            return Mathf.Clamp(experienceRating, 0, 100);
        }
    }
}
