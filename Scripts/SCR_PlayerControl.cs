using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

//handles the AI behaviour in game
public class SCR_PlayerControl : MonoBehaviour
{
    public Button throwButton;

    //references to each team's stats
    public SCR_Player[] playerTeamStats;

    public SCR_Player[] aiTeamStats;

    //references to the camera components
    [SerializeField] private GameObject followCamera;

    [SerializeField] private Vector3 followCamOffset;

    [SerializeField] private Vector3 followCamRotation;

    //references to the ball that will be thrown
    [SerializeField] private GameObject ball;

    [SerializeField] private Transform ballSpawnPoint;

    /*[Header("Used to prevent collision bugs")]
    [SerializeField] private float shortCutPowerCap = 1f;*/

    [Header("Optional Variables")]
    [SerializeField] private Transform idealStartingDirection;

    [SerializeField] private GameObject secretShortcutDirection;

    //current player that is throwing
    private SCR_Player currentPlayer;

    private SCR_RoundSequence roundScript;

    //reference to the player data
    private float power;

    private GameObject jack;

    private float distanceToTarget;

    [HideInInspector] public GameObject spawnedBall;

    //references to the throw data
    private Vector3 direction;

    private bool powerShot = false;

    private bool accuracyShot = false;

    //reference to the current player throwing in each team
    private int team1Index = 0;

    private int team2Index = 0;

    private Vector3 targetDirection;

    private float errorChance;

    private float powerLimit;

    private float vel;

    private GameObject target;

    //reference to the player tactics
    private bool blockOpponent = false;

    private bool powerSet = false;

    private bool targetSet = false;

    //sets a power or accuracy shot
    public bool IsPowerShot
    {
        set
        {
            powerShot = value;
            accuracyShot = !value;
            powerSet = true;
        }
    }

    //sets the tactic chosen
    public int TacticNumber
    {
        set
        {
            targetSet = true;
            value = Mathf.Clamp(value, 1, 3);
            if(value < 3) //tactics for blocking or aiming directly at jack
            {
                target = jack;

                if(value == 2) //tactics for blocking
                {
                    blockOpponent = true;
                    return;
                }
                //tactics for aiming at jack
                blockOpponent = false;
            }
            else //tactics for hitting opponents ball
            {
                GameObject[] thrownBalls = GameObject.FindGameObjectsWithTag("Ball");
                if(thrownBalls.Length > 0)
                {
                    foreach(GameObject thrownBall in thrownBalls)
                    {
                        if(thrownBall.GetComponent<SCR_BallMovement>().teamNumber != roundScript.ActiveTeam)
                        {
                            target= thrownBall;
                            return;
                        }
                    }
                }
                //if no balls have been thrown, targets the jack rather than an opponent ball
                target = jack;
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //jack = GameObject.FindGameObjectWithTag("Jack");
        //variables set up
        roundScript = GameObject.Find("Main Camera").GetComponent<SCR_RoundSequence>();

        //distanceToJack = Vector3.Distance(gameObject.transform.position, jack.transform.position);

        throwButton.interactable = false;

        IsPowerShot = false;

        team1Index = 0;
        team2Index = 0;

        followCamera.GetComponent<Camera>().enabled = false;

        //initialise follow camera
        followCamera.transform.rotation = Quaternion.Euler(followCamRotation);
    }

    // Update is called once per frame
    void Update()
    {
        //spawns jack
        if (!jack)
        {
            jack = GameObject.FindGameObjectWithTag("Jack");
        } else if (jack.IsDestroyed()) //if jack was destroyed because it fell out of bounds, spawn a new jack
        {
            jack = GameObject.FindGameObjectWithTag("Jack");
        }

        //if a ball is spawned, set the follow camera position based on the position of the spawned ball
        if (spawnedBall)
        {
            followCamera.transform.position = spawnedBall.transform.position + followCamOffset;
        }
    }

    //called to set the players shot
    void SetPowerAndDirection()
    {
        //caps the players power based on their power stats
        powerLimit = ((float)currentPlayer.powerRating * 15.0f) + 500.0f;
        /*if(target == secretShortcutDirection)
        {
            powerLimit = shortCutPowerCap;
        }*/

        //sets player's error chance based on their control rating
        errorChance = ((101.0f - (float)currentPlayer.controlRating) / 200.0f);
        //get distance to the jack/opponents ball
        distanceToTarget = Vector3.Distance(gameObject.transform.position, target.transform.position);
        Debug.Log("Distance to target: " + distanceToTarget);
        //set up shot. If power, throw with more power but less accuracy and vice versa
        if (powerShot)
        {
            SetupShot(1f, 1.2f, 1f);
        }
        else
        {
            SetupShot(0.9f, 1.1f, 0.5f);
        }
        
        Debug.Log(power);
    }
    //completes the players throw. Low and high multiplier refers to the power range and accuracry multiplier refers to the players accuracy
    private void SetupShot(float lowMultiplier, float highMultiplier, float accuracyMultiplier)
    {
        //estimate the power required to reach the jack (doesn't take ramps into account)
        Rigidbody rb = ball.GetComponent<Rigidbody>();
        float timeToReachJack = Mathf.Sqrt(distanceToTarget / (rb.drag / Time.fixedDeltaTime));
        vel = timeToReachJack * distanceToTarget;
        //code adapted from brenosilver (2015)
        float idealPower = rb.mass * vel;
        idealPower = ((rb.drag / Time.fixedDeltaTime) * idealPower) / (1 - 0.02f * (rb.drag/ Time.fixedDeltaTime));
        //end of adapted code

        //if the tactic is to block the opponent, throw the ball short by reducing the power
        if (blockOpponent)
        {
            idealPower *= 0.8f;
        }
        //decide if player has made a power error
        bool powerError = HasMadeError();
        if (!powerError)
        {
            //if not, then set power based on power range above, as well as accuracy rating (higher = closer to ideal power)
            power = Random.Range(idealPower * lowMultiplier * (0.5f + (0.005f * (float)currentPlayer.accuracyRating)), idealPower * highMultiplier * (1.5f - (0.005f * (float)currentPlayer.accuracyRating)));
        }
        else
        {
            //otherwise, set power randomly, potentially resulting in too much or too little
            power = Random.Range(1f, 2000f);
            Debug.Log("Power Error!");
        }
        //cap power if over limit
        if (power > powerLimit)
        {
            power = powerLimit;
        }
        //refers to how close to the ideal direction the player will be, based on accuracy rating
        float errorMargin = ((101.0f - (float)currentPlayer.accuracyRating) / 100.0f) * accuracyMultiplier;
        //decide if player has made an accuracy error
        bool accuracyError = HasMadeError();
        //prevents errors from becoming too large
        if (errorMargin > 1f)
        {
            errorMargin = 1f;
        }
        //decide if the player made an accuracy error
        if (!accuracyError)
        {
            //if not, set direction randomly based on the error margin
            direction = new Vector3(Random.Range(targetDirection.x - errorMargin, targetDirection.x + errorMargin), 0f, 1f);
        }
        else
        {
            //otherwise, set direction randomly, resulting in potentially throws well wide of the target
            direction = new Vector3(Random.Range(-1f, 1f), 0f, 1f);
            Debug.Log("Accuracy Error");
        }
    }

    //randomly decides if the player has made an error based on their error likihood
    bool HasMadeError()
    {
        float randNum = Random.Range(0f, 1f);
        return errorChance > randNum;
    }

    //throws the ball. Called by "throw" button. If an AI player, sets their tactics
    public void Throw(bool aiPlayer)
    {
        //if tactics have not been set, do not throw the ball
        if((!powerSet || !targetSet) && !aiPlayer)
        {
            return;
        }
        //resets direction for shortcuts
        if (secretShortcutDirection)
        {
            if (target == secretShortcutDirection)
            {
                target = jack;
            }
        }
        //ai player tactic set up
        if (aiPlayer)
        {
            currentPlayer = aiTeamStats[team2Index];
            team2Index++;
            int randNum = Random.Range(1, 3); //randomly chooses power or accuracy shot
            if (randNum == 1)
            {
                IsPowerShot = true;
            }
            else
            {
                IsPowerShot = false;
            }

            randNum = Random.Range(1, 4); //randomly sets the tactic
            TacticNumber = randNum;
        }
        else
        {
            currentPlayer = playerTeamStats[team1Index];
            team1Index++;
        }
        roundScript.UpdateDetailsPanel(currentPlayer); //update the player details text
        //changes target direction if jack is blocked by obstacles
        if (idealStartingDirection)
        {
            targetDirection = transform.TransformDirection(idealStartingDirection.position - transform.position);
            targetDirection.Normalize();
        }
        else
        {
            targetDirection = transform.TransformDirection(target.transform.position - transform.position);
            targetDirection.Normalize();
        }

        //if there is a secret shortcut
        if (secretShortcutDirection)
        {
            //player must be aiming for jack
            if (target == jack && !blockOpponent)
            {
                Debug.Log("Aim for shortcut");
                //randomly decides if shortcut was spotted based on the experience rating
                float chanceOfSpottingShortcut = (float)currentPlayer.experienceRating / 100.0f;
                float randomNumber = Random.Range(0f, 1f);
                if (randomNumber <= chanceOfSpottingShortcut)
                {
                    //if it was spotted, aim for the shortcut
                    target = secretShortcutDirection;
                    targetDirection = transform.TransformDirection(secretShortcutDirection.transform.position - transform.position);
                    targetDirection.Normalize();
                }

            }
        }
        //disable throw button and set up the shot
        throwButton.interactable = false;
        SetPowerAndDirection();
        //spawn the ball to be thrown
        spawnedBall = Instantiate(ball, ballSpawnPoint.position, ballSpawnPoint.rotation);
        //roll the ball based on power and direction set in SetPowerAndDirection()
        spawnedBall.GetComponent<SCR_BallMovement>().RollBall(power, direction);
        //set team number of ball
        spawnedBall.GetComponent<SCR_BallMovement>().teamNumber = roundScript.ActiveTeam;

        //disable main camera and enable ball camera
        Camera.main.enabled = false;
        Camera ballCamera = followCamera.GetComponent<Camera>();
        ballCamera.enabled= true;
        roundScript.ActiveCamera = ballCamera;
    }
}
