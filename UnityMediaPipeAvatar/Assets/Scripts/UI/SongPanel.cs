using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SongPanel : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] protected GameObject m_SongButton;
    [SerializeField] protected ScrollRect m_ScrollRect;
    [SerializeField] protected RectTransform m_ScrollViewContent;
    [SerializeField] protected SelectedSongPanel m_SelectedSongPanel;
    private SongAsset[] allSongs;
    private Sprite[] SongImg;
    void Start()
    {
        allSongs = Resources.LoadAll<SongAsset>("Music/Audio");
        
        foreach(var song in allSongs)
        {
            GameObject songBt = Instantiate(m_SongButton, m_ScrollViewContent);
            TextMeshProUGUI btText = songBt.GetComponentInChildren<TextMeshProUGUI>();
            
            btText.text = song.songTitle;
            Debug.Log(btText.text);
            Button btn = songBt.GetComponent<Button>();
            
            btn.onClick.AddListener(() =>
            {
                m_SelectedSongPanel.DisplaySong(song);
            });
        }
        m_SelectedSongPanel.DisplaySong(allSongs[0]);
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
