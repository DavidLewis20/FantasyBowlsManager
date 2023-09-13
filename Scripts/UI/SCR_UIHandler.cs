using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.Services.Core;
using Unity.Services.Leaderboards;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards.Models;

[System.Serializable]
public class VoiceoverClips //used for each voiceover clip added to scene
{
    [TextArea]
    public string subtitles;

    public AudioSource voiceover;
}

public class SCR_UIHandler : MonoBehaviour
{
    //references to the relevant UI element game objects
    [SerializeField] private GameObject startScreen;

    [SerializeField] private GameObject menuScreen;

    [SerializeField] private GameObject storeScreen;

    [SerializeField] private GameObject teamSelectScreen;

    [SerializeField] private GameObject courseSelectScreen;

    [SerializeField] private GameObject teamManagementScreen;

    [SerializeField] private GameObject specificLeaderboardsScreen;

    [SerializeField] private GameObject leaderboardsScreen;

    [SerializeField] private TMP_InputField managerNameInput; //reference to the text entry field

    [SerializeField] private GameObject managerNameSelect;

    [SerializeField] private GameObject managerStatsScreen;

    [SerializeField] private SCR_ManagerStatsScreen statsHandler; //reference to manager stats script

    [SerializeField] private GameObject moneyText;

    [SerializeField] private GameObject errorMessage;

    [SerializeField] private Button goToLevelSelect;

    [SerializeField] private AudioSource buttonPress; //button press audio

    //references to voiceover set up variables
    [SerializeField] private VoiceoverClips[] voiceoverElement;

    [SerializeField] private GameObject subtitlesPanel;

    [SerializeField] private TextMeshProUGUI subtitlesTextArea;

    //reference to all UI buttons (for tutorial)
    [SerializeField] private Button[] menuButtons;

    //references to relevant UI buttons
    [SerializeField] private Button storeButton;

    [SerializeField] private Button storeBackButton;

    [SerializeField] private Button teamManagementBackButton;

    [SerializeField] private Button tutorialOnButton;

    [SerializeField] private Button tutorialOffButton;

    [SerializeField] private Button subtitlesOnButton;

    [SerializeField] private Button subtitlesOffButton;

    //references to leaderboard data
    [SerializeField] private TextMeshProUGUI leaderboardHeader;

    [SerializeField] private SCR_LeaderboardEntry yourEntry;

    [SerializeField] private string winPercentageLeaderboardID;

    [SerializeField] private string totalWinsLeaderboardID;

    public Button playButton;

    public Button teamManagementButton;

    public bool tutorialEnabled = false;

    private int voiceoverIndex = 0;

    private bool subtitlesEnabled = true;

    //used to determine whether non-relevant UI buttons should be disabled
    public bool TutorialEnabled
    {
        get
        {
            return tutorialEnabled;
        }
        set
        {
            tutorialEnabled = value;

            if (tutorialEnabled)
            {
                tutorialOnButton.interactable = false;
                tutorialOffButton.interactable = true;
            }
            else
            {
                tutorialOnButton.interactable = true;
                tutorialOffButton.interactable = false;
            }
        }
    }

    //if true, displays subtitles in the tutorial
    public bool SubtitlesEnabled
    {
        get
        {
            return subtitlesEnabled;
        }
        private set
        {
            subtitlesEnabled = value;
        }
    }

    public async void Awake()
    {
        //set up online access to leaderboards
        await UnityServices.InitializeAsync();

        //sign in anonymously (used for easy access to leaderboards)
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Signed in anonymously");
        } catch (AuthenticationException e)
        {
            Debug.LogError("Couldn't sign in");
        }

        //add current player scores to the respective leaderboards
        await LeaderboardsService.Instance.AddPlayerScoreAsync(winPercentageLeaderboardID, GameManager.gameManager.WinPercentage);

        await LeaderboardsService.Instance.AddPlayerScoreAsync(totalWinsLeaderboardID, GameManager.gameManager.ManagerWins);
    }

    // Start is called before the first frame update
    void Start()
    {
        //initialise UI panels
        startScreen.SetActive(true);
        menuScreen.SetActive(false);
        storeScreen.SetActive(false);
        teamSelectScreen.SetActive(false);
        courseSelectScreen.SetActive(false);
        teamManagementScreen.SetActive(false);
        managerNameSelect.SetActive(false);
        managerStatsScreen.SetActive(false);
        moneyText.SetActive(false);
        errorMessage.SetActive(false);
        subtitlesPanel.SetActive(false);
        leaderboardsScreen.SetActive(false);
        specificLeaderboardsScreen.SetActive(false);

        TutorialEnabled = tutorialEnabled;
        ToggleSubtitles(SubtitlesEnabled);

        //disabled play and team management if players team is less than 4
        if (GameManager.gameManager.usersTeam.Count < 4)
        {
            playButton.interactable = false;
            teamManagementButton.interactable = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //enable play button if users team is 4 (or more) and the tutorial is not running
        if(!playButton.interactable && GameManager.gameManager.usersTeam.Count >= 4 && !TutorialEnabled)
        {
            playButton.interactable = true;
            teamManagementButton.interactable = true;
        } else if(playButton.interactable && GameManager.gameManager.usersTeam.Count < 4 && !TutorialEnabled) //otherwise, disable if it drops below 4
        {
            playButton.interactable = false;
        }

        //prevents players from going to level select before the team has been set
        if (teamSelectScreen.activeSelf)
        {
            if (GameManager.gameManager.TeamSelected)
            {
                goToLevelSelect.interactable = true;
            }
        }
    }

    //called by "proceed" on manager set up screen
    public void EnterManagerName()
    {
        //prevents players from entering a blank name
        if (managerNameInput.text != "")
        {
            //set manager name and open main menu
            PlayButtonPressSFX();
            GameManager.gameManager.ManagerName = managerNameInput.text;
            OpenMenuScreen();
        }
    }

    //opened by "start" if no save data was found
    void OpenManagerNameSelect()
    {
        startScreen.SetActive(false);
        managerNameSelect.SetActive(true);
    }

    //called by start and "proceed" on manager set up
    public void OpenMenuScreen()
    {
        PlayButtonPressSFX();
        //assumes that no manager name means no save data
        if(GameManager.gameManager.ManagerName == "")
        {
            //therefore, opens manager set up rather than main menu
            OpenManagerNameSelect();
            return;
        }

        //if tutorial is enabled
        if (tutorialEnabled)
        {
            DisableAllMenuButtons();

            //set up tutorial sequence based on the current voice line. (Menu is accessed 3 times, with each case representing a starting voiceline for this page)
            switch (voiceoverIndex)
            {
                case 0:
                    StartCoroutine(RunTutorialVoiceover(2, storeButton, false));
                    break;
                case 8:
                    StartCoroutine(RunTutorialVoiceover(1, teamManagementButton, false));
                    break;
                case 17:
                    StartCoroutine(RunTutorialVoiceover(1, playButton, false));
                    break;
                default:
                    break;

            }
        }

        //hide all other screens except main menu (only includes screens that menu can be accessed from)
        menuScreen.SetActive(true);
        moneyText.SetActive(true);
        startScreen.SetActive(false);
        storeScreen.SetActive(false);
        teamSelectScreen.SetActive(false);
        teamManagementScreen.SetActive(false);
        managerNameSelect.SetActive(false);
        managerStatsScreen.SetActive(false);
        leaderboardsScreen.SetActive(false);

        //set money value and clear any UI lists
        moneyText.GetComponent<TextMeshProUGUI>().text = "Cash Remaining: $" + GameManager.gameManager.Money;
        GameManager.gameManager.ClearList();
    }

    //runs the tutorial voiceover for the current menu page
    IEnumerator RunTutorialVoiceover(int voiceoverLines, Button activeButton, bool buyingPlayers)
    {
        while(voiceoverLines > 0)
        {
            //if voiceover index is out of bounds, exit loop as could trigger a crash
            if(voiceoverIndex >= voiceoverElement.Length)
            {
                break;
            }

            //displays subtitles of voiceover if enabled
            if (SubtitlesEnabled)
            {
                subtitlesPanel.SetActive(true);

                subtitlesTextArea.text = voiceoverElement[voiceoverIndex].subtitles;
            }

            //plays voiceover if clip is included and waits until clip is finished. Otherwise, waits 2 seconds
            if (voiceoverElement[voiceoverIndex].voiceover)
            {
                voiceoverElement[voiceoverIndex].voiceover.Play();

                yield return new WaitForSeconds(voiceoverElement[voiceoverIndex].voiceover.clip.length);

                voiceoverElement[voiceoverIndex].voiceover.Stop();
            }
            else
            {
                yield return new WaitForSeconds(2f);
            }

            //increment voiceover
            voiceoverIndex++;

            voiceoverLines--;
        }

        //hide subtitles panel as it obstructs part of the screen
        if (SubtitlesEnabled)
        {
            subtitlesPanel.SetActive(false);
        }

        //prevents player from exiting store if they have bought less than 4 players
        if (buyingPlayers)
        {
            while(GameManager.gameManager.usersTeam.Count < 4)
            {
                yield return null;
            }
        }
        //activate next button
        activeButton.interactable = true;
    }

    //update the money text. Called by game manager.
    public void UpdateMoney()
    {
        moneyText.GetComponent<TextMeshProUGUI>().text = "Cash Remaining: $" + GameManager.gameManager.Money;
    }

    //open manager stats screen
    public void OpenManagerStats()
    {
        PlayButtonPressSFX();
        menuScreen.SetActive(false);
        managerStatsScreen.SetActive(true);
        statsHandler.DisplayManagerStats(); //sets up the text elements
    }

    //opens store. Called by "store" button
    public void OpenStore()
    {
        PlayButtonPressSFX();
        menuScreen.SetActive(false);
        storeScreen.SetActive(true);
       
        GameManager.gameManager.DisplayPlayerList(); //calls the list generator to generate the store list

        //if tutorial is enabled, set up tutorial sequence
        if (tutorialEnabled)
        {
            DisableAllMenuButtons();

            StartCoroutine(RunTutorialVoiceover(6, storeBackButton, true));
        }
        
    }

    //opens team select for a match. Called by "play" from main menu
    public void OpenTeamSelect()
    {
        PlayButtonPressSFX();
        menuScreen.SetActive(false);
        teamSelectScreen.SetActive(true);
        courseSelectScreen.SetActive(false);
        moneyText.SetActive(false);
        GameManager.gameManager.ClearList(); //removes any buttons if level select was opened
        GameManager.gameManager.DisplayTeamForTeamSelect(); //generates UI list
        GameManager.gameManager.playerIndex = 0;
        GameManager.gameManager.TeamSelected = false;
        goToLevelSelect.interactable = false;

        //if tutorial is enabled, set up sequence
        if (tutorialEnabled)
        {
            DisableAllMenuButtons();

            StartCoroutine(RunTutorialVoiceover(2, playButton, false));
        }
    }

    //opens level select screen. Called by "proceed" on team select screen
    public void OpenCourseSelect()
    {
        PlayButtonPressSFX();
        teamSelectScreen.SetActive(false);
        courseSelectScreen.SetActive(true);
        GameManager.gameManager.ClearList(); //clears the player list buttons
        GameManager.gameManager.DisplayLevels(); //generates the UI level list

        //if tutorial is enabled, run tutorial sequence
        if (tutorialEnabled)
        {
            DisableAllMenuButtons();

            StartCoroutine(RunTutorialVoiceover(3, playButton, false));
        }
    }

    //called by level buttons. Runs the game level.
    public void StartGame(string levelName)
    {
        PlayButtonPressSFX();

        //if level 1 is opened, set tutorial enabled bool for game manager
        //this tells the game whether to play the tutorial in level 1 or not
        if (levelName == "SN_Map1")
        {
            GameManager.gameManager.TutorialEnabled = TutorialEnabled;
            GameManager.gameManager.SubtitlesEnabled = SubtitlesEnabled;
        }
        else
        {
            GameManager.gameManager.TutorialEnabled = false;
            GameManager.gameManager.SubtitlesEnabled = false;
        }

        SceneManager.LoadScene(levelName);
    }

    //opens team management screen. Called by team management button
    public void OpenTeamManagement()
    {
        PlayButtonPressSFX();
        menuScreen.SetActive(false);

        teamManagementScreen.SetActive(true);

        GameManager.gameManager.DisplayTeamForManagement(); //generates the team UI panels

        //if tutorial is enabled, run the sequence
        if (tutorialEnabled)
        {
            DisableAllMenuButtons();

            StartCoroutine(RunTutorialVoiceover(8, teamManagementBackButton, false));
        }
    }

    //called when a purchase or player removal would risk the player being unable to make a team of 4
    public void DisplayErrorMessage()
    {
        errorMessage.SetActive(true);
    }

    //called when OK is pressed on error message
    public void HideErrorMessage()
    {
        PlayButtonPressSFX();
        errorMessage.SetActive(false);
    }

    //plays the button press audio source
    public void PlayButtonPressSFX()
    {
        buttonPress.Play();
    }

    //used in the tutorial to control the path of the user
    public void DisableAllMenuButtons()
    {
        foreach(Button menuButton in menuButtons)
        {
            menuButton.interactable = false;
        }
    }

    //turns subtitles on or off. Called by subtitle buttons
    public void ToggleSubtitles(bool turnOn)
    {
        if (turnOn)
        {
            SubtitlesEnabled = true;

            subtitlesOnButton.interactable = false;
            subtitlesOffButton.interactable = true;
        }
        else
        {
            SubtitlesEnabled = false;

            subtitlesOnButton.interactable = true;
            subtitlesOffButton.interactable = false;
        }
    }

    //called by leaderboards button to open the leaderboard select
    public void OpenLeaderboards()
    {
        PlayButtonPressSFX();

        leaderboardsScreen.SetActive(true);
        menuScreen.SetActive(false);
        specificLeaderboardsScreen.SetActive(false);
        moneyText.SetActive(false);

        GameManager.gameManager.ClearList(); //clears any leaderboard lists
    }

    //opens a specific leaderboard. Called by the respective leaderboard buttons.
    public async void OpenChosenLeaderboards(string ID)
    {
        leaderboardsScreen.SetActive(false);
        specificLeaderboardsScreen.SetActive(true);

        //since ID is the leaderboard name with _ instead of spaces, converts ID to leaderboard name and sets header text
        string ldrbrdName = ID.Replace("_", " ");
        leaderboardHeader.text = ldrbrdName;

        GameManager.gameManager.DisplayChosenLeaderboard(ID); //generates the leaderboard list

        var playerEntry = await LeaderboardsService.Instance.GetPlayerScoreAsync(ID); //gets the players score

        //displays the players score in a separate panel
        yourEntry.rankingText.text = playerEntry.Rank.ToString();
        yourEntry.playerNameText.text = playerEntry.PlayerName.ToString();
        yourEntry.scoreText.text = playerEntry.Score.ToString("0.##");
    }
}
