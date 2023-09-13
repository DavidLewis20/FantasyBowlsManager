using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

//controls the sequence of each in game round
public class SCR_RoundSequence : MonoBehaviour
{
    //ui elements
    [SerializeField] private TextMeshProUGUI turnText;

    [SerializeField] private GameObject tacticsPanel;

    [SerializeField] private float outOfBoundsHeight = -10.0f;

    public TextMeshProUGUI roundText;

    [SerializeField] private TextMeshProUGUI playerNameText;

    [SerializeField] private TextMeshProUGUI playerPowerText;

    [SerializeField] private TextMeshProUGUI playerAccuracyText;

    [SerializeField] private TextMeshProUGUI playerControlText;

    [SerializeField] private TextMeshProUGUI playerExperienceText;

    [SerializeField] private GameObject resultText;

    [SerializeField] private string nextSceneName = "SN_MainMenu";

    //reference to the jack to spawn
    [SerializeField] private GameObject jack;

    [Header("Jack spawn point range")]
    [SerializeField] private float minimumXPoint = 1f; //refers to where jack can spawn

    [SerializeField] private float maximumXPoint = 1f;

    [SerializeField] private float minimumZPoint = 1f;

    [SerializeField] private float maxZPoint = 1f;

    [SerializeField] private float basePayout = 100f;

    //tutorial variables
    [SerializeField] private VoiceoverClips[] tutorialVoiceoverClips;

    [SerializeField] private GameObject subtitlesPanel;

    [SerializeField] private TextMeshProUGUI subtitlesTextArea;

    private bool tutorialRunning = false; //if true, will run the tutorial

    private float closestDistance = 1000000f; //used to calculate the closest ball to the jack

    //camera set up
    private Camera activeCamera;

    private Camera mainCamera;

    //team details
    private int currentTeam;

    private int winningTeam = 1;

    private GameObject spawnedJack;

    [SerializeField] private SCR_PlayerControl controlScript;

    private int turnCount = 1;

    //sets the round winner and the results text
    public int Winner
    {
        get
        {
            return winningTeam;
        }
        set
        {
            winningTeam = value;
            if (winningTeam == 1 || winningTeam == 2)
            {
                resultText.GetComponent<TextMeshProUGUI>().text = "The winner is: Team " + winningTeam.ToString();
            }
            else
            {
                resultText.GetComponent<TextMeshProUGUI>().text = "It's a draw!";
            }
        }
    }

    //sets the active team and the team text
    public int ActiveTeam
    {
        get
        {
            return currentTeam;
        }
        set
        {
            if (value== 1 || value == 2)
            {
                currentTeam = value;
                turnText.text = "Current Team: " + currentTeam.ToString();
            }
        }
    }

    //sets the active camera
    public Camera ActiveCamera
    {
        private get
        {
            return activeCamera;
        }
        set
        {
            activeCamera = value;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //variable set up
        mainCamera = Camera.main;

        resultText.SetActive(false);

        ActiveTeam = Random.Range(1, 3);
        //spawns the jack at a random point in a range set by the player
        spawnedJack = Instantiate(jack, new Vector3(Random.Range(minimumXPoint, maximumXPoint), transform.position.y, Random.Range(minimumZPoint, maxZPoint)), transform.rotation);

        StartCoroutine(Gameplay()); //runs the game

        GameManager.gameManager.Round++;

        tacticsPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        //if jack falls out of bounds, respawn the jack
        if(spawnedJack.transform.position.y < -10.0f)
        {
            Destroy(spawnedJack);
            spawnedJack = Instantiate(jack, new Vector3(Random.Range(minimumXPoint, maximumXPoint), transform.position.y, Random.Range(minimumZPoint, maxZPoint)), transform.rotation);
        }
    }

    //controls the gameplay sequence
    IEnumerator Gameplay()
    {
        yield return null;

        while (turnCount <= 4)
        {
            //if team 1, set up control panel
            if (ActiveTeam == 1)
            {
                controlScript.throwButton.interactable = false;

                if (GameManager.gameManager.TutorialEnabled) //if tutorial is enabled, display tutorial
                {
                    tutorialRunning = true;
                    StartCoroutine(RunTutorial());
                }

                while (tutorialRunning) //don't open tactics panel until tutorial is finished
                {
                    yield return null;
                }

                //open tactics panel
                controlScript.throwButton.interactable = true;

                tacticsPanel.SetActive(true);

                //wait until player has clicked "throw ball"
                while (controlScript.throwButton.interactable)
                {
                    yield return null;
                }

                tacticsPanel.SetActive(false);

                yield return new WaitForSeconds(1f); //wait 1 second to allow the ball to be spawned
                Vector3 vel = controlScript.spawnedBall.GetComponent<Rigidbody>().velocity;
                float speedTimer = 0f;
                while ((vel.magnitude > 0f && speedTimer < 3f) && controlScript.spawnedBall.transform.position.y > outOfBoundsHeight)
                {
                    //Debug.Log(vel.magnitude);
                    //if velocity drops below 0.1 for longer than 3 seconds, it is assumed that the ball has reached its destination, so end turn
                    if(vel.magnitude < 0.1f)
                    {
                        speedTimer += Time.deltaTime;
                    }
                    else
                    {
                        speedTimer = 0f;
                    }
                    vel = controlScript.spawnedBall.GetComponent<Rigidbody>().velocity;
                    yield return null;
                }

                FinishTurn();

                ActiveTeam = 2; //change active team
            }
            else //if team 2, throw the opponents ball
            {
                controlScript.Throw(true);
                yield return new WaitForSeconds(1f); //wait 1 second to allow ball to be spawned
                Vector3 vel = controlScript.spawnedBall.GetComponent<Rigidbody>().velocity;
                float speedTimer = 0f;
                while ((vel.magnitude > 0f && speedTimer < 3f) && controlScript.spawnedBall.transform.position.y > outOfBoundsHeight)
                {
                    //Debug.Log(vel.magnitude > 0f && controlScript.spawnedBall.transform.position.y > -10f);
                    //as above, stop ball and end turn if velocity drops below 0.1 for 3 seconds
                    if (vel.magnitude < 0.1f)
                    {
                        speedTimer += Time.deltaTime;
                    }
                    else
                    {
                        speedTimer = 0f;
                    }
                    vel = controlScript.spawnedBall.GetComponent<Rigidbody>().velocity;
                    yield return null;
                }

                FinishTurn();

                ActiveTeam = 1; //change active team
            }

            turnCount++; //update turn count
        }
        //once game is finished, calculate the winner
        CalculateScore();

        yield return new WaitForSeconds(2f);
        //update the game scores
        GameManager.gameManager.UpdateScore(Winner, basePayout);
        //if end of round 3, decide the overall winner
        if (GameManager.gameManager.Round >= 3)
        {
            //display winner on screen
            resultText.GetComponent<TextMeshProUGUI>().text = "Team " + GameManager.gameManager.gameWinner + " wins the game!";
            yield return new WaitForSeconds(2f);

            //set money and XP text based on amounts calculated in Game Manager
            string textToDisplay = "Money Earnt: " + GameManager.gameManager.MoneyEarnt;

            if (GameManager.gameManager.P1XPGained > 0)
            {
                textToDisplay += "\n\n" + GameManager.gameManager.roundPlayers[0].playerForename + " " + GameManager.gameManager.roundPlayers[0].playerSurname + " XP Gained: " + GameManager.gameManager.P1XPGained;
                textToDisplay += "\n\n" + GameManager.gameManager.roundPlayers[1].playerForename + " " + GameManager.gameManager.roundPlayers[1].playerSurname + " XP Gained: " + GameManager.gameManager.P2XPGained;
            }
            else
            {
                textToDisplay += "\n\nPlayer XP is only earnt in the first 10 games each day.";
            }

            //set text
            resultText.GetComponent<TextMeshProUGUI>().text = textToDisplay;
            yield return new WaitForSeconds(2f);
            SceneManager.LoadScene(nextSceneName); //load main menu after 2 seconds
        }
        else
        {
            //otherwise, reload the scene and start next round
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    //runs the tutorial
    IEnumerator RunTutorial()
    {
        for(int i = 0; i < tutorialVoiceoverClips.Length; i++)
        {
            //if subtitles are enabled, then display them alongside the voiceover
            if (GameManager.gameManager.SubtitlesEnabled)
            {
                subtitlesPanel.SetActive(true);
                subtitlesTextArea.text = tutorialVoiceoverClips[i].subtitles;
            }

            //if there are voiceover clips, run the voiceover and wait until clip is finished
            if (tutorialVoiceoverClips[i].voiceover)
            {
                tutorialVoiceoverClips[i].voiceover.Play();
                yield return new WaitForSeconds(tutorialVoiceoverClips[i].voiceover.clip.length);
            }
            else
            {
                yield return new WaitForSeconds(2f); //otherwise, wait for 2 seconds
            }
        }
        //hide subtitles and disable tutorial as it is finished
        subtitlesPanel.SetActive(false);

        GameManager.gameManager.TutorialEnabled = false;

        tutorialRunning = false;
    }
    //pauses game
    public void PauseGame()
    {
        GameManager.gameManager.PauseGame();
    }

    //finishes the turn and resets the cameras
    private void FinishTurn()
    {
        controlScript.spawnedBall.GetComponent<Rigidbody>().velocity = Vector3.zero;
        mainCamera.enabled = true;
        ActiveCamera.enabled = false;
    }

    //calculates the round winner
    void CalculateScore()
    {
        closestDistance = 100000000f;
        int ballsOutOfBounds = 0;
        //find all the thrown balls in the scene
        GameObject[] thrownBalls = GameObject.FindGameObjectsWithTag("Ball");
        foreach(GameObject thrownBall in thrownBalls)
        {
            //if out of bounds, don't count ball
            if(thrownBall.transform.position.y < -10f)
            {
                ballsOutOfBounds++;
                continue;
            }

            //calculate distance of ball to the jack
            float dist = Vector3.Distance(thrownBall.transform.position, jack.transform.position);

            //set as current winner if closer than the original closest distance
            if(dist < closestDistance)
            {
                closestDistance = dist;
                Winner = thrownBall.GetComponent<SCR_BallMovement>().teamNumber;
            }
        }

        //if every ball was out of bounds, don't set a winner
        if(ballsOutOfBounds > 3)
        {
            Winner = -1;
        }
        Debug.Log("The winner is: Team " + Winner);
        resultText.SetActive(true);
    }

    //sets the text on the player details panel based on the active player
    public void UpdateDetailsPanel(SCR_Player currentPlayer)
    {
        playerNameText.text = currentPlayer.playerForename + " " + currentPlayer.playerSurname;
        playerPowerText.text = "Power: " + currentPlayer.powerRating.ToString();
        playerAccuracyText.text = "Accuracy: " + currentPlayer.accuracyRating.ToString();
        playerControlText.text = "Control: " + currentPlayer.controlRating.ToString();
        playerExperienceText.text = "Experience: " + currentPlayer.experienceRating.ToString();
    }
}
