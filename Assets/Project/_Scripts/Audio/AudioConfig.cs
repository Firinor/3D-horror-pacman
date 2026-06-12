using UnityEngine;

[CreateAssetMenu(menuName = "Configs/Audio")]
public class AudioConfig : ScriptableObject
{
    [Header("Buttons")] 
    public ClipSettings ButtonClick;
    [Header("Player")] 
    public ClipSettings Steps;
    public ClipSettings Hartbeat;
    public ClipSettings HartbeatPanic;
    [Header("Collect")] 
    public ClipSettings BatteryCollect;
    public ClipSettings KeyCollect;
    public ClipSettings Telleport;
    [Header("Flashlight")] 
    public ClipSettings FlashlightAttack;
    [Header("Enemy")] 
    public ClipSettings Idle;
    public ClipSettings Warning;
    public ClipSettings Run;
    public ClipSettings PlayerCatch;
    [Header("Win")] 
    public ClipSettings Win;
    public ClipSettings Lose;
}