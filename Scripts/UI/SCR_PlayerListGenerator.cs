using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;

//code adapted from Beever (2023)
//inaccurate name used, actually handles list generation for all UI lists
public class SCR_PlayerListGenerator : MonoBehaviour
{
    //references to UI elements to be spawned
    [SerializeField] private GameObject playerButton;

    [SerializeField] private GameObject storeButton;

    [SerializeField] private GameObject levelButton;

    [SerializeField] private GameObject managementPlayerPanel;

    [SerializeField] private GameObject leaderboardEntry;

    //reference to game courses
    [SerializeField] private List<Courses> gameCourses;

    //references to positioning of UI elements
    [SerializeField] private Transform storeContentPanel;

    [SerializeField] private Transform teamSelectPanel;

    [SerializeField] private Transform levelSelectPanel;

    [SerializeField] private Transform teamManagementPanel;

    [SerializeField] private Transform leaderboardPanel;

    //stores the spawned buttons, so the can be destroyed once the page is exited, preventing duplicate buttons
    private List<GameObject> spawnedButtons = new List<GameObject>();

    private void Update()
    {
        /*gameCourses = new List<Courses>();*/
        /*if (spawnedButtons == null)
        {
            spawnedButtons = new List<GameObject>();
        }*/
    }

    //generates a player list for the store
    public void GeneratePlayerList(List<SCR_Player> players)
    {
        //access every player in the game
        foreach(var item in players)
        {
            //spawn a button for each player
            GameObject spawnedButton = Instantiate(storeButton) as GameObject;
            spawnedButtons.Add(spawnedButton);
            SCR_PlayerButton tempButton = spawnedButton.GetComponent<SCR_PlayerButton>();

            //set up text elements
            tempButton.playerNameText.text = item.playerForename + " " + item.playerSurname;
            tempButton.powerStatsText.text = "Power: " + item.powerRating.ToString();
            tempButton.accuracyStatsText.text = "Accuracy: " + item.accuracyRating.ToString();
            tempButton.controlStatsText.text = "Control: " + item.controlRating.ToString();
            tempButton.experienceStatsText.text = "Experience: " + item.experienceRating.ToString();
            tempButton.playerCostText.text = "Price: $" + item.cost.ToString();
            tempButton.growthPotentialText.text = "Growth Potential: " + item.playerType.growthPotential.ToString();

            //set player so it can be accessed by game manager on purchase
            tempButton.player = item;
            //set up button functionality
            tempButton.button.onClick = tempButton.onClickEvents.addPlayer;

            //disable if player doesn't have enough money
            if(item.cost > GameManager.gameManager.Money)
            {
                tempButton.button.interactable = false;
            }

            //set position and size of button
            spawnedButton.transform.SetParent(storeContentPanel);
            spawnedButton.transform.localScale = new Vector3(1f, 1f, 1f);
        }
    }

    //generates the team list for team selection screen (after clicking "Play")
    public void GenerateTeamList(List<SCR_Player> teamPlayers)
    {
        //access each team member
        foreach (var item in teamPlayers)
        {
            //spawn the button
            GameObject spawnedButton = Instantiate(playerButton) as GameObject;
            spawnedButtons.Add(spawnedButton);
            SCR_PlayerButton tempButton = spawnedButton.GetComponent<SCR_PlayerButton>();

            //set up text elements
            tempButton.playerNameText.text = item.playerForename + " " + item.playerSurname;
            tempButton.powerStatsText.text = "Power: " + item.powerRating.ToString();
            tempButton.accuracyStatsText.text = "Accuracy: " + item.accuracyRating.ToString();
            tempButton.controlStatsText.text = "Control: " + item.controlRating.ToString();
            tempButton.experienceStatsText.text = "Experience: " + item.experienceRating.ToString();
            //set player so it can be accessed by the game manager
            tempButton.player = item;
            //set up button functionality
            tempButton.button.onClick = tempButton.onClickEvents.setTeam;

            //set position and size of button
            spawnedButton.transform.SetParent(teamSelectPanel);
            spawnedButton.transform.localScale = new Vector3(1f, 1f, 1f);
        }
    }

    //generates level list for the level select screen
    public void GenerateLevelList()
    {
        //access each game level set by user
        foreach(var item in gameCourses)
        {
            //spawn the button
            GameObject spawnedButton = Instantiate(levelButton) as GameObject;
            spawnedButtons.Add(spawnedButton);
            SCR_LevelButton tempButton = spawnedButton.GetComponent<SCR_LevelButton>();

            //set up UI elements and button functionality
            tempButton.levelNameText.text = item.courseName;
            tempButton.levelImage.sprite = item.courseIcon;
            tempButton.levelButton.onClick = item.openCourse;
            if(GameManager.gameManager.ManagerWins < item.winsRequiredToUnlock)
            {
                tempButton.levelButton.interactable = false;
            }

            //set position and scale of button
            spawnedButton.transform.SetParent(levelSelectPanel);
            spawnedButton.transform.localScale = new Vector3(1f, 1f, 1f);
        }
    }

    //generates a player panel with their details for team management screen
    public void GenerateTeamManagementPanel(List<SCR_Player> usersTeam)
    {
        int index = 0;
        //access each team member
        //a for i loop could have been used for index access, but the foreach was already set up before it was realised that index was required
        foreach(var item in usersTeam)
        {
            //spawn the button
            GameObject spawnedPanel = Instantiate(managementPlayerPanel) as GameObject;
            spawnedButtons.Add(spawnedPanel);
            SCR_ManagementPanel tempPanel = spawnedPanel.GetComponent<SCR_ManagementPanel>();

            //set up text elements
            tempPanel.playerNameText.text = item.playerForename + " " + item.playerSurname;
            tempPanel.powerStatsText.text = "Power: " + item.powerRating.ToString();
            tempPanel.accuracyStatsText.text = "Accuracy: " + item.accuracyRating.ToString();
            tempPanel.controlStatsText.text = "Control: " + item.controlRating.ToString();
            tempPanel.experienceStatsText.text = "Experience: " + item.experienceRating.ToString();
            tempPanel.growthPotentialText.text = "Growth Potential: x" + item.growthRate.ToString("0.00");
            tempPanel.XPText.text = "XP: " + item.XP.ToString();
            tempPanel.statBoostsText.text = "Stat Boosts: " + item.statBoosts.ToString();
            tempPanel.removedText.SetActive(false);
            tempPanel.buttonIndex = index; //set index for game manager access

            //set player for game manager access
            tempPanel.player = item;
            //set up button functionality for each button in the panel
            tempPanel.increasePowerButton.onClick = tempPanel.powerOnClick.setTeam;
            tempPanel.increaseAccuracyButton.onClick = tempPanel.accuracyOnClick.setTeam;
            tempPanel.increaseControlStatsButton.onClick = tempPanel.controlOnClick.setTeam;
            tempPanel.increaseExperienceButton.onClick = tempPanel.experienceOnClick.setTeam;
            tempPanel.removePlayerButton.onClick = tempPanel.removeButtonOnClick.setTeam;

            //disable stat boosts button if they don't have any stat boosts
            if(tempPanel.player.statBoosts <= 0)
            {
                tempPanel.DisableButtons();
            }

            //set position of panel
            spawnedPanel.transform.SetParent(teamManagementPanel);
            spawnedPanel.transform.localScale = Vector3.one;
            index++;
        }
    }
    //end of adapted code

    //prevents duplicate spawned buttons
    public void DestroySpawnedButtons()
    {
        //remove buttons if there are any
        if(spawnedButtons == null)
        {
            return;
        }
        foreach(var button in spawnedButtons)
        {
            Destroy(button);
        }
        //clear the spawned buttons list as they no longer exist
        spawnedButtons.Clear();
    }

    //set up unity leaderboard. Leaderboard ID is usually leaderboard name but with _ instead of spaces
    //can display any leaderboard providing leaderboard ID is correct
    public async void DisplayLeaderboard(string leaderboardID)
    {
        //get leaderboard data. Await used as leaderboard is an online service
        var leaderboardData = await LeaderboardsService.Instance.GetPlayerRangeAsync(leaderboardID);

        //get each leaderboard entry
        foreach(var entry in leaderboardData.Results)
        {
            //spawn a leaderboard panel
            GameObject spawnedEntry = Instantiate(leaderboardEntry) as GameObject;

            spawnedButtons.Add(spawnedEntry);
            SCR_LeaderboardEntry tempEntry = spawnedEntry.GetComponent<SCR_LeaderboardEntry>();

            //set up text elements
            tempEntry.rankingText.text = entry.Rank.ToString();
            tempEntry.playerNameText.text = entry.PlayerName;
            tempEntry.scoreText.text = entry.Score.ToString("0.##");

            //set position and scale of panel
            spawnedEntry.transform.SetParent(leaderboardPanel);
            spawnedEntry.transform.localScale = Vector3.one;
        }
    }
}
