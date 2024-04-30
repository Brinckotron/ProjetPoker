using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Slider = UnityEngine.UI.Slider;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider effectsVolumeSlider;
    
    public void ChangeEffectsVolume()
    {
        GameManager.Instance.effectsVolume = effectsVolumeSlider.value;
    }
    public void ChangeMusicVolume()
    {
        GameManager.Instance.musicVolume = musicVolumeSlider.value;
        Camera.main.gameObject.GetComponent<AudioSource>().volume = GameManager.Instance.musicVolume;
    }

    public void NewGame()
    {
        SceneManager.LoadScene("Shop");
    }
}
