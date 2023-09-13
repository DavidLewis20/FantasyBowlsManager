using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using TMPro;
//using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

public class GameManager : MonoBehaviour
{
    //game manager singleton
    public static GameManager gameManager;

    //contains the different player types that can spawn (e.g., accurate players, legendary players, etc.)
    [SerializeField] private SCR_PlayerTypes[] playerTypes;

    //possible forenames that are randomly generated
    [SerializeField] private string[] possiblePlayerForenames;

    //possible surnames that are randomly generated
    [SerializeField] private string[] possiblePlayerSurnames;

    //pause menu game object
    [SerializeField] private GameObject pauseMenu;

    //canvas position
    private GameObject canvas;

    //tracks when the game was last opened. Uses a string as DateTime is not Serializable
    private string lastOpenedDate = "";

    //generates lists on request
    public SCR_PlayerListGenerator playerListGenerator;

    //contains all the player names
    private List<string> playerNames;

    //contains the entire player pool spawned when game is first loaded
    public List<SCR_Player> playerPool;

    //tracks if players are affordable
    private bool enoughMoney = true;

    private SCR_Player[] tempPlayerPool; //allows data to be carried over across scenes

    private SCR_Player[] tempUsersTeam; //as above

    public SCR_Player[] roundPlayers = new SCR_Player[4]; //contains the players used in the current round

    //contains the users team
    public List<SCR_Player> usersTeam;

    //control script
    private SCR_PlayerControl controlScript;

    private SCR_UIHandler uIHandler;

    //paused menu game object which was spawned
    private GameObject spawnedPauseMenu;

    private int roundNo;

    private int team1Wins = 0;

    private int team2Wins = 0;

    //contains the current score in round
    private TextMeshProUGUI currentScoreText;

    private SCR_RoundSequence roundScript;

    //used to track players when selecting team. Cannot be higher than 1
    [HideInInspector] public int playerIndex;

    private int gamesPlayed = 0;

    private int managerWins = 0;

    private int managerLosses = 0;

    private int money = 10000;

    private float winPercentage = 0f;

    private string managerName = "";

    //tracks how much money will be left over if the player makes a purchase. Used to prevent player from being "stuck" and unable to buy players
    private int moneyAfterPurchase;

    private bool teamSelected = false;

    //tracks if a save file was found.
    private bool dataLoaded = false;

    private bool tutorialEnabled = false;

    private bool subtitlesEnabled = false;

    private int gamesPlayedToday = 0;
    
    //prize money after round
    private int moneyEarnt = 0;

    private int p1XPGained = 0;

    private int p2XPGained = 0;

    [HideInInspector] public int gameWinner;
    
    public int MoneyEarnt { get; private set; }

    public int P1XPGained { get; private set; }

    public int P2XPGained { get; private set; }

    public bool TutorialEnabled
    {
        get
        {
            return tutorialEnabled;
        }
        set
        {
            tutorialEnabled = value;
        }
    }

    public bool SubtitlesEnabled
    {
        get
        {
            return subtitlesEnabled;
        }
        set
        {
            subtitlesEnabled = value;
        }
    }
    public int Round
    {
        get
        {
            return roundNo;
        }
        set
        {
            if (roundNo <= 3)
            {
                roundNo = value;

                if (roundScript)
                {
                    roundScript.roundText.text = "Round: " + roundNo;
                }
            }
        }
    }

    public int ManagerWins
    {
        get
        {
            return managerWins;
        }
        set
        {
            managerWins = value;
            GamesPlayed++;
            gameWinner = 1;
        }
    }

    public bool TeamSelected
    {
        get
        {
            return teamSelected;
        }
        set
        {
            teamSelected = value;
        }
    }

    public int ManagerLosses
    {
        get
        {
            return managerLosses;
        }
        set
        {
            managerLosses = value;
            GamesPlayed++;
            gameWinner = 2;
        }
    }

    public int GamesPlayed
    {
        get
        {
            return gamesPlayed;
        }
        private set
        {
            gamesPlayed = value;
            WinPercentage = WinPercentage;
        }
    }

    public float WinPercentage
    {
        get
        {
            return winPercentage;
        } 
        private set
        {
            if (GamesPlayed > 0)
            {
                winPercentage = ((float)ManagerWins / (float)GamesPlayed) * 100.0f;
            } else
            {
                winPercentage = 0f;
            }
        }
    }

    public string ManagerName
    {
        get
        {
            return managerName;
        }
        set
        {
            if(managerName == "")
            {
                managerName = value;
            }
        }
    }

    public int Money
    {
        get
        {
            return money;
        }
        set
        {
            money = value;

            if (SceneManager.GetActiveScene().name == "SN_MainMenu")
            {
                uIHandler.UpdateMoney();
            }
        }
    }

    //if false, stops player from buying player and displays an error message
    public bool SuitableMoneyRemaining
    {
        get
        {
            return enoughMoney = moneyAfterPurchase >= (3 - usersTeam.Count) * 500;
        }
    }

    //if false, stops player from removing player if they don't have enough money for a new one
    public bool CanDeletePlayer
    {
        get
        {
            return enoughMoney = Money >= (4 - (usersTeam.Count - 1)) * 500;
        }
    }

    private void Awake()
    {
        //singleton setup
        if(gameManager == null)
        {
            DontDestroyOnLoad(gameObject);
            gameManager = this;
        } 
        else if(gameManager != this)
        {
            Destroy(gameObject);
        }

        if (gameManager == this)
        {
            playerNames = new List<string>();
            playerPool = new List<SCR_Player>();
            usersTeam = new List<SCR_Player>();

            uIHandler = GameObject.Find("Main Camera").GetComponent<SCR_UIHandler>();

            LoadGameData();

            //tracks when game was last opened. If opened on a different day, will reset the gamesPlayedToday counter
            if(lastOpenedDate != DateTime.Today.ToString())
            {
                gamesPlayedToday = 0;

                lastOpenedDate = DateTime.Today.ToString();
            }

            int index = 0;

            //if a save file wasn't located
            if (!dataLoaded)
            {
                //create a randomly generated player pool of 100 players
                for (int i = 0; i < 100; i++)
                {
                    playerPool.Add(new SCR_Player());
                    playerPool[i].playerType = playerTypes[index];
                    playerPool[i].playerForename = possiblePlayerForenames[UnityEngine.Random.Range(0, possiblePlayerForenames.Length)];
                    playerPool[i].playerSurname = possiblePlayerSurnames[UnityEngine.Random.Range(0, possiblePlayerSurnames.Length)];
                    string playerFullName = playerPool[i].playerForename + playerPool[i].playerSurname;

                    //if a duplicate name is found then re-create the player
                    if (playerNames.Contains(playerFullName))
                    {
                        playerPool.Remove(playerPool[i]);
                        i--;
                        continue;
                    }
                    playerNames.Add(playerFullName);
                    playerPool[i].GeneratePlayer();
                    if (index + 1 < playerTypes.Length)
                    {
                        index++;
                    }
                    else
                    {
                        index = 0;
                    }

                    /*Debug.Log("Player Name: " + playerPool[i].playerForename + " " + playerPool[i].playerSurname);

                    Debug.Log("Current player's strength = " + playerPool[i].powerRating);*/
                }
            }

            //used so player pool still exists once a new scene is opened
            tempPlayerPool = playerPool.ToArray();

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //get player pool and users team
        playerPool = tempPlayerPool.ToList();
        usersTeam = tempUsersTeam.ToList();
        Debug.Log("First player: " + playerPool[0].playerForename + " " + playerPool[0].playerSurname); 

        //if main menu is loaded, save game and reset scores and round number
        if(scene.name == "SN_MainMenu")
        {
            Round = 0;
            team1Wins = 0;
            team2Wins = 0;
            playerListGenerator = GameObject.Find("PlayerManager").GetComponent<SCR_PlayerListGenerator>();
            uIHandler = GameObject.Find("Main Camera").GetComponent<SCR_UIHandler>();

            SaveGameData();
            return;
        }

        //otherwise, set up UI for game scene
        canvas = GameObject.Find("Canvas");

        roundScript = GameObject.Find("Main Camera").GetComponent<SCR_RoundSequence>();

        currentScoreText = GameObject.Find("ScoreText").GetComponent<TextMeshProUGUI>();
        currentScoreText.text = "Team 1: " + team1Wins + "\nTeam 2: " + team2Wins;

/*        for(int i = 2; i < roundPlayers.Length; i++)
        {
            roundPlayers[i] = playerPool[Random.Range(0, playerPool.Count)];
        }*/

        controlScript = GameObject.Find("Main Camera").GetComponent<SCR_PlayerControl>();

        //add respective team players to their respective teams in the control script
        for(int i = 0; i < roundPlayers.Length; i++)
        {
            if (i < 2)
            {
                controlScript.playerTeamStats[i] = roundPlayers[i];
                continue;
            }

            controlScript.aiTeamStats[i - 2] = roundPlayers[i];
        }
    }

    public void UpdateScore(int winningTeam, float basePayout)
    {
        //set the winner
        if (winningTeam == 1)
        {
            team1Wins++;
        }
        else if(winningTeam == 2)
        {
            team2Wins++;
        }

        //if round 3, display results
        if(Round >= 3)
        {
            gamesPlayedToday++;
            P1XPGained = 0;
            P2XPGained = 0;
            MoneyEarnt = 0;

            //adjusts prize money based on team player's experience rating
            float teamPlayer1Multiplier = roundPlayers[0].experienceRating / 50.0f;
            float teamPlayer2Multiplier = roundPlayers[1].experienceRating / 50.0f;
            int XPBoost;

            //award player XP based on difficulty of opponent
            if (roundPlayers[2].overallRating > 90.0f || roundPlayers[3].overallRating > 90.0f)
            {
                XPBoost = 1000;
            } 
            else if (roundPlayers[2].overallRating > 85.0f || roundPlayers[3].overallRating > 85.0f)
            {
                XPBoost = 750;
            } 
            else if (roundPlayers[2].overallRating > 80.0f || roundPlayers[3].overallRating > 80.0f)
            {
                XPBoost = 500;
            } 
            else 
            {
                XPBoost = 250;
            }

            //if player wins
            if (team1Wins > team2Wins)
            {
                ManagerWins++;

                //set money earnt using algorithm below (reduces based on games played)
                MoneyEarnt = ((int)((basePayout * 5f) * teamPlayer1Multiplier) + (int)((basePayout * 5f) * teamPlayer2Multiplier)) / gamesPlayedToday;
                Money += MoneyEarnt;

                //if 10 or less games have been played today, award X4 player XP and adjust via growth rate
                if (gamesPlayedToday <= 10)
                {
                    P1XPGained = (int)(XPBoost * 4 * roundPlayers[0].growthRate);
                    P2XPGained = (int)(XPBoost * 4 * roundPlayers[1].growthRate);
                    roundPlayers[0].IncreaseXP(P1XPGained);
                    roundPlayers[1].IncreaseXP(P2XPGained);
                }
            }
            else
            {
                //if player loses or draws, do same as before, but increase losses count (if lost) and award standard money and XP rewards
                if (team1Wins < team2Wins)
                {
                    ManagerLosses++;
                } else
                {
                    GamesPlayed++;
                }

                MoneyEarnt = ((int)(basePayout * teamPlayer1Multiplier) + (int)(basePayout * teamPlayer2Multiplier)) / gamesPlayedToday;
                Money += MoneyEarnt;

                if (gamesPlayedToday <= 10)
                {
                    P1XPGained = (int)(XPBoost * roundPlayers[0].growthRate);
                    P2XPGained = (int)(XPBoost * roundPlayers[1].growthRate);
                    roundPlayers[0].IncreaseXP(P1XPGained);
                    roundPlayers[1].IncreaseXP(P2XPGained);
                }
            }
        }
    }

    //sets up player list from list generator
    public void DisplayPlayerList()
    {
        if (gameManager == this)
        {
            playerListGenerator.GeneratePlayerList(playerPool);
        }
    }

    //sets up team select from list generator
    public void DisplayTeamForTeamSelect()
    {
        if (usersTeam.Count > 0)
        {
            playerListGenerator.GenerateTeamList(usersTeam);

            roundPlayers[0] = null;
            roundPlayers[1] = null;
        }
    }

    //sets up team list for team management screen
    public void DisplayTeamForManagement()
    {
        if(usersTeam.Count > 0)
        {
            playerListGenerator.GenerateTeamManagementPanel(usersTeam);
        }
    }

    //clears list to prevent duplicate spawned buttons
    public void ClearList()
    {
        playerListGenerator.DestroySpawnedButtons();
    }

    //display level list from list generator and set up opponent team
    public void DisplayLevels()
    {
        roundPlayers[2] = playerPool[UnityEngine.Random.Range(0, playerPool.Count)];
        Debug.Log("Opposition Player: " + roundPlayers[2].playerForename + " " + roundPlayers[2].playerSurname);
        roundPlayers[3] = playerPool[UnityEngine.Random.Range(0, playerPool.Count)];
        Debug.Log("Opposition Player: " + roundPlayers[3].playerForename + " " + roundPlayers[3].playerSurname);
        playerListGenerator.GenerateLevelList();
    }

    //display a leaderboard based on the ID
    public void DisplayChosenLeaderboard(string leaderboardID)
    {
        playerListGenerator.DisplayLeaderboard(leaderboardID);
    }

    //set up round team
    public void SetRoundTeam(SCR_Player chosenPlayer)
    {
        uIHandler.PlayButtonPressSFX();
        if (roundPlayers.Contains(chosenPlayer))
        {
            return;
        }

        roundPlayers[playerIndex] = chosenPlayer;
        Debug.Log("Added " + chosenPlayer.playerForename + " " + chosenPlayer.playerSurname + " to the team");
        playerIndex++;

        if (playerIndex > 1)
        {
            TeamSelected = true;
            playerIndex = 0;
        }
    }

    public void AddNewPlayer(SCR_Player newPlayer)
    {
        //if less than 4 players in team
        if(usersTeam.Count < 4)
        {
            //check how much money is remaining post-purchase
            moneyAfterPurchase = Money - newPlayer.cost;

            //if with the remaining money, player is at risk of not affording a full team, block the purchase and display error message
            if (!SuitableMoneyRemaining)
            {
                uIHandler.DisplayErrorMessage();
                return;
            }

            //otherwise, add the player to the team and remove from player pool to avoid duplicates
            uIHandler.PlayButtonPressSFX();
            usersTeam.Add(newPlayer);
            tempUsersTeam = usersTeam.ToArray();
            playerPool.Remove(newPlayer);
            tempPlayerPool = playerPool.ToArray();
            Money = moneyAfterPurchase;
            Debug.Log("Added " + newPlayer.playerForename + " " + newPlayer.playerSurname + " to team");
        }
    }

    //remove a player from the team
    public void RemovePlayer(int playerIndex)
    {
        //block removal if player can't afford a new player
        if (!CanDeletePlayer)
        {
            uIHandler.DisplayErrorMessage();
            return;
        }

        uIHandler.PlayButtonPressSFX();

        SCR_Player tempPlayer = usersTeam[playerIndex];
        usersTeam.RemoveAt(playerIndex);
        playerPool.Add(tempPlayer);
    }

    //pause game and display pause menu
    public void PauseGame()
    {
        Time.timeScale = 0f;
        canvas.SetActive(false);
        spawnedPauseMenu = Instantiate(pauseMenu) as GameObject;
        spawnedPauseMenu.transform.localScale = Vector3.one;
    }

    //unpause game and hide pause menu
    public void UnPauseGame()
    {
        Time.timeScale = 1f;
        Destroy(spawnedPauseMenu);
        canvas.SetActive(true);
    }

    //exit level from pause menu
    public void ExitCurrentLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("SN_MainMenu");
    }

    //code adapted from Beever (2023)
    //save game to Json file
    public void SaveGameData()
    {
        SCR_SaveData data = new SCR_SaveData();

        data.playerPool = playerPool;
        data.usersTeam = usersTeam;
        data.managerWins = ManagerWins;
        data.managerLosses = ManagerLosses;
        data.gamesPlayed = GamesPlayed;
        data.money = Money;
        data.managerName = ManagerName;
        data.lastSavedDate = DateTime.Today.ToString(); //convert to string as DateTime is not serializable
        data.gamesPlayedToday = gamesPlayedToday;

        var fullPath = Path.Combine(Application.persistentDataPath, "savedata.dat");

        try
        {
            File.WriteAllText(fullPath, System.Convert.ToBase64String(Encoding.UTF8.GetBytes(data.ToJson())));
        } catch (System.Exception e)
        {
            Debug.LogError($"Failed to write to {fullPath} with exception {e}");
        }
    }

    //load game data from Json
    public void LoadGameData()
    {
        string savedData;

        var fullPath = Path.Combine(Application.persistentDataPath, "savedata.dat");

        SCR_SaveData data = new SCR_SaveData();

        try
        {
            savedData = Encoding.UTF8.GetString(System.Convert.FromBase64String(File.ReadAllText(fullPath)));

            data.LoadFromJson(savedData);

            playerPool = data.playerPool;
            usersTeam = data.usersTeam;
            tempUsersTeam = usersTeam.ToArray();
            ManagerWins = data.managerWins;
            ManagerLosses = data.managerLosses;
            GamesPlayed = data.gamesPlayed;
            Money = data.money;
            ManagerName = data.managerName;
            lastOpenedDate = data.lastSavedDate;
            gamesPlayedToday = data.gamesPlayedToday;

            dataLoaded = true;
        } catch(System.Exception e)
        {
            Debug.LogError($"Failed to read from {fullPath} with exception {e}");

            savedData = "";

            dataLoaded = false;
        }
    }
    //end of adapted code
}
