using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectedSongPanel : MonoBehaviour
{
    [SerializeField]protected TextMeshProUGUI title;
    [SerializeField]protected TextMeshProUGUI Artist;
    [SerializeField]protected TextMeshProUGUI Time;
    [SerializeField] protected Image TitleImg;
    [SerializeField] protected AudioSource audio;
    [SerializeField] protected Button btn;
    [SerializeField] protected Animator animator;
    [SerializeField] protected GameObject songPanel;
    [SerializeField] protected Transform ava;
    // Start is called before the first frame update
    void Start()
    {
        btn.onClick.AddListener(PlaySong);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DisplaySong(SongAsset song)
    {
        audio.clip = song.audioClip;
        audio.Play();
        title.text = song.songTitle;
        Artist.text = song.artist;
        Time.text = (int)song.audioClip.length/60 + ":" + (((int)song.audioClip.length % 60 < 10) ? ("0" + (int)song.audioClip.length % 60) : (int)song.audioClip.length % 60);
        TitleImg.sprite = song.coverImage;
    }

    void PlaySong()
    {
        String song = title.text.ToLower();
        audio.Play();
        animator.Play(song);
        Quaternion temp = ava.rotation;
        Vector3 tempLoc = ava.localPosition;
        if (song.Equals("super shy"))
        {
            ava.Rotate(0,-80,0);
        }else if (song.Equals("likejennie"))
        {
            ava.Rotate(0,-50,0);
        }
        else if (song.Equals("haidilao"))
        {
            ava.Rotate(0, -10, 0);
        }

        GameManager.gameManager.restart(audio.clip.length, temp, tempLoc);
        songPanel.SetActive(false);
        // 게임매니저 스타트
    }

    IEnumerator WaitForSongToEnd(float sec, Quaternion originRotation, Vector3 originLoc)
    {
        Debug.Log("restart");
        yield return new WaitForSeconds(sec);
        Debug.Log("노래끝");
        ava.rotation = originRotation;
        ava.localPosition = originLoc;
        songPanel.SetActive(true);
    }
}
