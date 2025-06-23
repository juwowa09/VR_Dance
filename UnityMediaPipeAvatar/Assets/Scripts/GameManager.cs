using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager gameManager;
    [SerializeField] protected Transform ava;
    [Tooltip("Song panel.")]
    [SerializeField] protected GameObject songPanel;
    [Tooltip("result panel.")]
    [SerializeField] protected GameObject resultPanel;

    [Tooltip("Play Button")] [SerializeField]
    protected Button m_PlayButton;
    // Start is called before the first frame update
    void Awake()
    {
        if (gameManager != null && gameManager != this)
        {
            Destroy(gameObject);
            return;
        }
        gameManager = this;
        DontDestroyOnLoad(gameManager);
    }

    public void restart(float sec, Quaternion quat, Vector3 loc)
    {
        StartCoroutine(WaitForSong(sec, quat, loc));
    }

    IEnumerator WaitForSong(float sec, Quaternion quat, Vector3 loc)
    {
        Debug.Log("restart");
        yield return new WaitForSeconds(sec);
        Debug.Log("노래끝");
        ava.rotation = quat;
        ava.localPosition = loc;
        songPanel.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
