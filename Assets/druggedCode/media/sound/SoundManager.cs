using UnityEngine;
using System.Collections;

namespace druggedcode
{
    /// <summary>
    /// 사운드 재생을 관리
    /// </summary>
    public class SoundManager : Singleton<SoundManager>
    {   
        //[Header("Particle Effects")]
        [Header("BGM")]
        ///BGM 활성화
        public bool MusicOn = true;

        ///BGM 볼륨
        [Range(0,1)]
        public float MusicVolume=0.3f;

        [Header("SFX")]
        ///SFX 볼륨
        [Range(0,1)]
        public float SfxVolume=1f;

        ///SFX 활성화 
        public bool SfxOn = true;

        AudioSource _backgroundMusic;   
        Transform _tr;

        override protected void Awake()
        {
            base.Awake();
            
            _tr = transform;
        }

        /// <summary>
        /// BGM 을 재생한다. BGM 은 한번에 하나만 플레이 된다.
        /// </summary>
        public void PlayBackgroundMusic( AudioSource Music )
        {
            if (MusicOn == false ) return;

            //이전 bgm 스톱
            if (_backgroundMusic != null )
            {
                _backgroundMusic.Stop();
            }

            //신규 bgm 재생 
            _backgroundMusic = Music;
            _backgroundMusic.volume = MusicVolume;
            _backgroundMusic.loop = true;
            _backgroundMusic.Play();        
        }   

        /// <summary>
        /// 효과음을 재생한다. bgm 은 AudioSource 를 전달 받지만 SFX 는 AudioClip 을 전달 받는다. 이부분 통일 필요.
        /// </summary>
        public AudioSource PlaySound( AudioClip Sfx, Vector3 Location )
        {
            if( SfxOn == false ) return null;
            if (Sfx == null)
                return null;

            // 오디오 소스 생성
            GameObject temporaryAudioHost = new GameObject("TempAudio");
            temporaryAudioHost.transform.position = Location;
            temporaryAudioHost.transform.SetParent( _tr );
            AudioSource audioSource = temporaryAudioHost.AddComponent<AudioSource>() as AudioSource; 

            audioSource.clip = Sfx; 
            audioSource.volume = SfxVolume;
            audioSource.Play(); 

            //사운드 재생이 끝나면 관련 객체 제거
            Destroy(temporaryAudioHost, Sfx.length);
            return audioSource;
        }
    }
}


