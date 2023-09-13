using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ButtonControl //reference to the on clicked events. Separate class used as these events must be Serializable
{
    //controls the add player functionality. Note that this can also used for other button functionality
    public Button.ButtonClickedEvent addPlayer;

    //controls the set team functionality
    public Button.ButtonClickedEvent setTeam;
}
public class SCR_PlayerButton : MonoBehaviour
{
    public Button button;

    public bool bShopButton = true; //allows same script to be used for shop and team select

    //references to text elements
    public TextMeshProUGUI playerNameText;

    public TextMeshProUGUI powerStatsText;

    public TextMeshProUGUI accuracyStatsText;

    public TextMeshProUGUI controlStatsText;

    public TextMeshProUGUI experienceStatsText;

    public TextMeshProUGUI growthPotentialText;

    public TextMeshProUGUI playerCostText;

    //controls the on clicked events
    public ButtonControl onClickEvents;

    //reference to the player that this button is attached to
    [HideInInspector] public SCR_Player player;

    //called to add a new player to the team from the store
    public void AddNewPlayer()
    {
        GameManager.gameManager.AddNewPlayer(player);
        button.interactable = false; //disables button to prevent duplicates
    }

    //called to add a player to the in game team from the team select
    public void SetTeamMember()
    {
        GameManager.gameManager.SetRoundTeam(player);
    }

    private void Update()
    {
        //if a shop button
        if (bShopButton)
        {
            //disable button if user cannot afford this player
            if (button.interactable && (GameManager.gameManager.Money < player.cost || GameManager.gameManager.usersTeam.Count >= 4))
            {
                button.interactable = false;
            }
        }
    }
}
