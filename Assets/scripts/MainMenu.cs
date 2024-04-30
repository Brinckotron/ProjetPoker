using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Slider = UnityEngine.UI.Slider;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Slider gameVolumeSlider;
    
    public void ChangeVolume()
    {
        GameManager.Instance.gameVolume = gameVolumeSlider.value;
        Camera.main.gameObject.GetComponent<AudioSource>().volume = GameManager.Instance.gameVolume;
    }

    public void NewGame()
    {
        SceneManager.LoadScene("Shop");
    }
}
