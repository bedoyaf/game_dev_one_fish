using UnityEngine;

public class MenuMusicSwitcher : MonoBehaviour
{
    [SerializeField] private SoundData menuMusic;
    void Start()
    {
        AudioManager.Instance.PlayMusic(menuMusic);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
