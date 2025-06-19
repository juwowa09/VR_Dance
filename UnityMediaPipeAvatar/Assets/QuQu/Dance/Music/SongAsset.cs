using UnityEngine;

[CreateAssetMenu(fileName = "NewSong", menuName = "Rhythm/Song")]
public class SongAsset : ScriptableObject
{
    public string songTitle;
    public string artist;
    public AudioClip audioClip;
    public Sprite coverImage;
}