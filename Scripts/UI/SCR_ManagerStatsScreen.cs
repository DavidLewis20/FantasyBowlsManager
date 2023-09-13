using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SCR_ManagerStatsScreen : MonoBehaviour
{
    //text that displays each stat shown on manager stats page
    [SerializeField] private TextMeshProUGUI managerNameText;

    [SerializeField] private TextMeshProUGUI managerWinsText;

    [SerializeField] private TextMeshProUGUI managerLossesText;

    [SerializeField] private TextMeshProUGUI totalGamesPlayedText;

    [SerializeField] private TextMeshProUGUI winPercentageText;

    //called by UI handler to set up text
    public void DisplayManagerStats()
    {
        managerNameText.text = GameManager.gameManager.ManagerName + "'s Stats";
        managerWinsText.text = "Wins: " + GameManager.gameManager.ManagerWins;
        managerLossesText.text = "Losses: " + GameManager.gameManager.ManagerLosses;
        totalGamesPlayedText.text = "Games Played: " + GameManager.gameManager.GamesPlayed;
        winPercentageText.text = "Win Percentage: " + GameManager.gameManager.WinPercentage.ToString("F2") + "%"; //formatted to 2 decimal places
    }
}
