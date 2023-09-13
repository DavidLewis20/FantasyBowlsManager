using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//controls the spawned panel elements in team management screen
public class SCR_ManagementPanel : MonoBehaviour
{
    //reference to text elements
    public TextMeshProUGUI playerNameText;

    public TextMeshProUGUI powerStatsText;

    public TextMeshProUGUI accuracyStatsText;

    public TextMeshProUGUI controlStatsText;

    public TextMeshProUGUI experienceStatsText;

    public TextMeshProUGUI growthPotentialText;

    public TextMeshProUGUI XPText;

    public TextMeshProUGUI statBoostsText;

    public GameObject removedText;

    //reference to buttons in the panel
    public Button increasePowerButton;

    public Button increaseAccuracyButton;

    public Button increaseExperienceButton;

    public Button increaseControlStatsButton;

    public Button removePlayerButton;

    //reference to the On Click functionality
    public ButtonControl powerOnClick;

    public ButtonControl accuracyOnClick;

    public ButtonControl controlOnClick;

    public ButtonControl experienceOnClick;

    public ButtonControl removeButtonOnClick;

    public AudioSource clickedSFX;

    [HideInInspector] public int buttonIndex; //used to detect which player was deleted if remove button is pressed

    [HideInInspector] public SCR_Player player; //reference to the player instance attached to the button

    private SCR_UIHandler uIHandler;

    private void Start()
    {
        uIHandler = GameObject.Find("Main Camera").GetComponent<SCR_UIHandler>();

        //disabled remove button if tutorial is enabled
        if (uIHandler.tutorialEnabled)
        {
            removePlayerButton.interactable = false;
        }
    }
    //Increases power stats by 1
    public void IncreasePower()
    {
        //if the power rating is less than 100
        if (player.powerRating < 100)
        {
            //increase power, subtract stat boosts by 1 and update power text
            clickedSFX.Play();
            player.IncreasePower();
            DecreaseBoostPoints();
            powerStatsText.text = "Power: " + player.powerRating.ToString();
        }
    }
    //decreases stat boost when one is used
    private void DecreaseBoostPoints()
    {
        //decrease stat boosts and update text
        player.statBoosts--;
        statBoostsText.text = "Stat Boosts: " + player.statBoosts.ToString();
        //if no stat boosts are left, disable all stat increase buttons
        if (player.statBoosts <= 0)
        {
            DisableButtons();
        }
    }
    //disables all stat boost buttons
    public void DisableButtons()
    {
        increasePowerButton.interactable = false;
        increaseAccuracyButton.interactable = false;
        increaseControlStatsButton.interactable = false;
        increaseExperienceButton.interactable = false;
    }

    //removes a player from the team
    public void RemovePlayer()
    {
        GameManager.gameManager.RemovePlayer(buttonIndex); //remove player in game manager
        //if successful
        if (GameManager.gameManager.CanDeletePlayer)
        {
            //disable every button attached to the player and display the removed text
            DisableButtons();
            removePlayerButton.interactable = false;
            removedText.SetActive(true);
        }
    }

    //increases accuracy stats by 1. Works as per IncreasePower()
    public void IncreaseAccuracy()
    {
        if (player.accuracyRating < 100)
        {
            clickedSFX.Play();
            player.IncreaseAccuracy();
            DecreaseBoostPoints();
            accuracyStatsText.text = "Accuracy: " + player.accuracyRating.ToString();
        }
    }

    //increases experience stats by 1. Works as per IncreasePower()
    public void IncreaseExperience()
    {
        if (player.experienceRating < 100)
        {
            clickedSFX.Play();
            player.IncreaseExperience();
            DecreaseBoostPoints();
            experienceStatsText.text = "Experience: " + player.experienceRating.ToString();
        }
    }

    //increases control stats by 1. Works as per IncreasePower()
    public void IncreaseControl()
    {
        if (player.controlRating < 100)
        {
            clickedSFX.Play();
            player.IncreaseControl();
            DecreaseBoostPoints();
            controlStatsText.text = "Control: " + player.controlRating.ToString();
        }
    }
}
