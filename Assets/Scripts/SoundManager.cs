
using UnityEngine;

public class SoundManager : Singleton<SoundManager>
{

    [SerializeField]
    private AudioClip arrow; // tiếng bắn tên
    [SerializeField]
    private AudioClip death; // tiếng chết
    [SerializeField]
    private AudioClip fireball; // tiếng bắn lửa
    [SerializeField]
    private AudioClip gameover; // tiếng gameover
    [SerializeField]
    private AudioClip hit; // tiếng đánh
    [SerializeField]
    private AudioClip level; // tiếng level up
    [SerializeField]
    private AudioClip newGame; // tiếng bắt đầu game
    [SerializeField]
    private AudioClip rock; // tiếng đá
    [SerializeField]
    private AudioClip towerBuilt; // tiếng xây tháp

    public AudioClip Arrow
    {
        get { return arrow; }
    }
    public AudioClip Death
    {
        get { return death; }
    }
    public AudioClip Fireball
    {
        get { return fireball; }
    }
    public AudioClip Gameover
    {
        get { return gameover; }
    }
    public AudioClip Hit
    {
        get { return hit; }
    }
    public AudioClip Level
    {
        get { return level; }
    }
    public AudioClip NewGame
    {
        get { return newGame; }
    }
    public AudioClip Rock
    {
        get { return rock; }
    }
    public AudioClip TowerBuilt
    {
        get { return towerBuilt; }
    }
}
