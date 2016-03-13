using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace druggedcode
{
    /// <summary>
    /// 사운드 재생을 관리
    /// </summary>
    public class SoundManager : Singleton<SoundManager>
    {
        //-----------------------------------------------------------------------------------
        // inspector
        //-----------------------------------------------------------------------------------
        [Header("BGM")]
        [SerializeField]
        bool isBgmOn = true;
        [Range(0, 1)]
        public float bgmMultiplier = 0.3f;

        [Header("SFX")]
        [SerializeField]
        bool isSfxOn = true;
        [Range(0, 1)]
        public float sfxMultiplier = 1f;
        public int maxChannels = 5;

        [Space(10)]
        [Header("PALETTE")]
        public List<AudioClip> basicClips;
        public List<SoundCategory> categoryList;

        //-----------------------------------------------------------------------------------
        // member
        //-----------------------------------------------------------------------------------

        Transform mTr;
        AudioSource mBgmChannel;
        List<AudioSource> mSfxChannelList;

        Dictionary<string, AudioClip> mPalette;
        Dictionary<string, SoundCategory> mCategories;

        override protected void Awake()
        {
            base.Awake();

            mTr = transform;

            //create channel
            mBgmChannel = GameObjectUtil.Create<AudioSource>("BGM_Channel", mTr);
            mBgmChannel.playOnAwake = false;
            mBgmChannel.loop = true;

            mSfxChannelList = new List<AudioSource>();
            for (int i = 0; i < maxChannels; ++i)
            {
                AudioSource ch = GameObjectUtil.Create<AudioSource>("SFX_Channel_" + i, mTr);
                ch.playOnAwake = false;
                mSfxChannelList.Add(ch);
            }

            //create palette
            mPalette = new Dictionary<string, AudioClip>();
            mCategories = new Dictionary<string, SoundCategory>();

            foreach (AudioClip clip in basicClips)
            {
                mPalette.Add(clip.name, clip);
            }

            foreach (SoundCategory category in categoryList)
            {
                mCategories.Add(category.name, category);
                foreach (AudioClip clip in category.clips)
                {
                    string clipName = clip.name;

                    if (mPalette.ContainsKey(category.name + "/" + clipName))
                    {
                        int i = 0;
                        while (mPalette.ContainsKey(category.name + "/" + clipName))
                        {
                            i++;
                            clipName = clip.name + i;
                        }
                    }
                    mPalette.Add(category.name + "/" + clipName, clip);
                }
            }
        }

        void Update()
        {
            if (Time.timeScale == 1f) return;

            float pitch = Mathf.Lerp(0.4f, 1f, Mathf.InverseLerp(0.5f, 1f, Time.timeScale));
            AudioSource s;
            for (int i = 0; i < maxChannels; ++i)
            {
                s = mSfxChannelList[i];
                if (s.isPlaying == false) continue;
                s.pitch = pitch;
            }
        }

        //언제나 하나의 bgm만을 재생
        public AudioSource PlayBGM(AudioClip clip, float volume = 1f, float pitch = 1f)
        {
            if (isBgmOn == false) return null;
            if (clip == null) return null;

            if (mBgmChannel.isPlaying) mBgmChannel.Stop();

            mBgmChannel.clip = clip;
            mBgmChannel.pitch = pitch;
            mBgmChannel.volume = volume * bgmMultiplier;
            mBgmChannel.Play();

            return mBgmChannel;
        }

        public AudioSource PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f)
        {
            return PlaySFX(clip, volume, pitch, Vector3.zero);
        }

        public AudioSource PlaySFX(string str, float volume = 1f, float pitch = 1f)
        {
            return PlaySFX(str, volume, pitch, Vector3.zero);
        }

        public AudioSource PlaySFX(string str, float volume, float pitch, Vector3 position)
        {
            if (str == "") return null;

            if (str.Contains("/"))
            {
                string[] chunks = str.Split('/');

                if (mCategories.ContainsKey(chunks[0]))
                {
                    if (chunks[1] == "Random")
                    {
                        SoundCategory c = mCategories[chunks[0]];
                        return PlaySFX(c.clips[(int)Random.Range(0, c.clips.Count)], volume, pitch, position);
                    }
                }
            }

            if (mPalette.ContainsKey(str))
            {
                return PlaySFX(mPalette[str], volume, pitch, position);
            }

            return null;
        }
        
        public AudioSource PlaySFX(AudioClip clip, float volume, float pitch, Vector3 position)
        {
            if (isSfxOn == false) return null;
            if (clip == null) return null;

            AudioSource ch = null;
            bool canPlay = false;

            for (int i = 0; i < maxChannels; ++i)
            {
                ch = mSfxChannelList[i];
                if (ch.isPlaying == false)
                {
                    canPlay = true;
                    break;
                }
            }

            if (canPlay == false || ch == null) return null;

            ch.transform.position = position;
            ch.pitch = pitch;
            ch.volume = volume * sfxMultiplier;
            ch.clip = clip;
            ch.Play();

            return ch;
        }
        
        public bool bgmMute
        {
            set
            {
                mBgmChannel.mute = value;
            }
        }

        public bool sfxMute
        {
            set
            {
                foreach (AudioSource s in mSfxChannelList)
                {
                    s.mute = value;
                }
            }
        }

        [System.Serializable]
        public class SoundCategory
        {
            public string name;
            public List<AudioClip> clips;
        }
    }
}


