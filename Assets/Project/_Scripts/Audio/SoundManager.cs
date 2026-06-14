using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class SoundManager : MonoBehaviour
{
   public static SoundManager Instance;
   [SerializeField]
   private AudioConfig config;
   [SerializeField]
   private List<AudioSource> audioPool;
   [SerializeField]
   private List<AudioSource> playerAudioPool;
   private int soundIndex;

   private const float buttonClickDelay = .2f;
   private DateTime lastButtonClick;

   private AudioSource scoresSource;
   
   private void Awake()
   {
      Instance = this;
      lastButtonClick = DateTime.Now;
   }
   
   public void PlayButtonClick(Vector3 position = default)
   {
      if((DateTime.Now - lastButtonClick).TotalSeconds < buttonClickDelay)
         return;
      
      lastButtonClick = DateTime.Now;
      
      Play(position, config.ButtonClick, isPriority: true);
   }
   public void PlayBatteryCollect(Vector3 position = default)
   {
      Play(position, config.BatteryCollect, isPriority: true);
   }
   public void PlayKeyCollect(Vector3 position = default)
   {
      Play(position, config.KeyCollect, isPriority: true);
   }
   public void PlayTelleport(Vector3 position = default)
   {
      Play(position, config.Telleport, isPriority: true);
   }
   public void PlayFlashlightAttack()
   {
      PlayerPlay(config.FlashlightAttack);
   }
   
   public void PlayWin(Vector3 position = default)
   {
      Play(position, config.Win, isPriority: true);
   }
   public void PlayLose(Vector3 position = default)
   {
      Play(position, config.Lose, isPriority: true);
   }
   
   //Enemy
   public void PlayIdle(Vector3 position = default)
   {
      Play(position, config.Idle, isPriority: true);
   }
   public void PlayWarning(Vector3 position = default)
   {
      Play(position, config.Warning, isPriority: true);
   }
   public void PlayRun(Vector3 position = default)
   {
      Play(position, config.Run, isPriority: true);
   }
   public void PlayPlayerCatch(Vector3 position = default)
   {
      Play(position, config.PlayerCatch, isPriority: true);
   }
   
   /*public void PlayScores()
   {
      scoresSource = audioPool[soundIndex];

      soundIndex++;
      soundIndex %= audioPool.Count;
   
      ClipSettings clipData = config.Scores;
      
      scoresSource.gameObject.SetActive(true);
      scoresSource.transform.position = default;
      scoresSource.pitch = 1 + Random.Range(-0.05f, 0.05f);
      scoresSource.clip = clipData.Clip;
      scoresSource.volume = clipData.Volume;
      scoresSource.loop = true;
      scoresSource.Play();
   }
   public void StopScores()
   {
      scoresSource.Stop();
      scoresSource.gameObject.SetActive(false);
      scoresSource = null;
   }*/
   
   public void Play(Vector3 position, ClipSettings clipData, bool isPriority = false)
   {
      AudioSource source = audioPool.FirstOrDefault(a => !a.gameObject.activeSelf);

      if (source is null)
      {
         source = audioPool[soundIndex];
      }

      soundIndex++;
      soundIndex %= audioPool.Count;

      source.gameObject.SetActive(true);
      source.transform.position = position;
      source.pitch = 1 + Random.Range(-0.05f, 0.05f);
      source.volume = clipData.Volume;
      
      source.PlayOneShot(clipData.Clip);

      StartCoroutine(DisableAudioSource(source));
   }
   private void PlayerPlay(ClipSettings clipData, bool isPriority = false)
   {
      AudioSource source = playerAudioPool.FirstOrDefault(a => !a.gameObject.activeSelf);

      source!.gameObject.SetActive(true);
      source.pitch = 1 + Random.Range(-0.05f, 0.05f);
      source.volume = clipData.Volume;
      
      source.PlayOneShot(clipData.Clip);

      StartCoroutine(DisableAudioSource(source));
   }
   private IEnumerator DisableAudioSource(AudioSource source)
   {
      while (source.isPlaying)
      {
         yield return null;
      }
      
      source.gameObject.SetActive(false);
   }
}
