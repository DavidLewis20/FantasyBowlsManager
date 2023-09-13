using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Courses //references an entire course element
{
    public string courseName;

    public int winsRequiredToUnlock;

    public Sprite courseIcon;

    public Button.ButtonClickedEvent openCourse;
}

public class SCR_LevelButton : MonoBehaviour
{
    //reference to UI elements in the button
    public TextMeshProUGUI levelNameText;

    public Image levelImage;

    public Button levelButton;

    private int totalTouches = 0; //used for a hidden cheat code

    private float timer = 1f;

    private void Update()
    {
        timer += Time.deltaTime;
        //cheat code for demo purposes
        if(Input.touchCount == 1 && timer > 1f)
        {
            timer = 0f;
            totalTouches++;

            //if screen is tapped 10 times in 10 different seconds, will enable all level buttons
            if (totalTouches >= 10)
            {
                levelButton.interactable = true;
            }
        }
    }
}
