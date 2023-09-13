using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

//handles pause menu functions
public class SCR_PauseMenu : MonoBehaviour
{
    //references to the core pause menu components
    [SerializeField] private GameObject pauseMenu;

    [SerializeField] private GameObject optionsMenu;

    [SerializeField] private GameObject exitMenu;

    [SerializeField] private Slider masterVolumeSlider;

    [SerializeField] private Slider musicVolumeSlider;

    [SerializeField] private Slider sfxVolumeSlider;

    //reference to the audio mixer set up
    [SerializeField] private AudioMixer mainMixer;

    private bool volumeSet = false; //used to prevent audio bugs

    // Start is called before the first frame update
    void Start()
    {
        OpenPauseMenu();

        float volume;

        //set the sliders to the current volume. Note that volume ranges from -80 to 20, so add 80 to get a percentage
        mainMixer.GetFloat("masterVol", out volume);
        masterVolumeSlider.value = (volume + 80f) / 100f;

        mainMixer.GetFloat("musicVol", out volume);
        musicVolumeSlider.value = (volume + 80f) / 100f;

        mainMixer.GetFloat("sfxVol", out volume);
        sfxVolumeSlider.value = (volume + 80f) / 100f;

        volumeSet = true;
    }

    //called by options button to open options
    public void OpenOptionsMenu()
    {
        optionsMenu.SetActive(true);
        pauseMenu.SetActive(false);
    }

    //called by pause button (from game manager) to display pause menu
    public void OpenPauseMenu()
    {
        pauseMenu.SetActive(true);
        optionsMenu.SetActive(false);
        exitMenu.SetActive(false);
    }

    //opens the exit confirmation window
    public void OpenExitMenu()
    {
        exitMenu.SetActive(true);
        pauseMenu.SetActive(false);
    }

    //called by resume button to resume game
    public void ResumeGame()
    {
        GameManager.gameManager.UnPauseGame();
    }

    //called by "yes" in exit confirmation window to load main menu
    public void ExitActiveLevel()
    {
        GameManager.gameManager.ExitCurrentLevel();
    }

    //called by volume slider OnValueChanged
    public void SetMasterVolume()
    {
        if (volumeSet)
        {
            //set volume based on current slider value
            float volume = (masterVolumeSlider.value * 100f) - 80f;
            mainMixer.SetFloat("masterVol", volume);
        }
    }

    public void SetMusicVolume()
    {
        if (volumeSet)
        {
            float volume = (musicVolumeSlider.value * 100f) - 80f;
            mainMixer.SetFloat("musicVol", volume);
        }
    }

    public void SetSFXVolume()
    {
        if (volumeSet)
        {
            float volume = (sfxVolumeSlider.value * 100f) - 80f;
            mainMixer.SetFloat("sfxVol", volume);
        }
    }
}
