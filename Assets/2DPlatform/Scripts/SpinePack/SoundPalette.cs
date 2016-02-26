/*****************************************************************************
 * Spine Asset Pack License
 * Version 1.0
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to use the Asset Pack and derivative works only as
 * incorporated and embedded components of your software applications and to
 * distribute such software applications. Any source code contained in the Asset
 * Pack may not be distributed in source form. You may otherwise not reproduce,
 * distribute, sublicense, rent, lease or lend the Asset Pack. It is emphasized
 * that you are not entitled to distribute or transfer the Asset Pack in any way
 * other way than as integrated components of your software applications.
 * 
 * THIS ASSET PACK IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS ASSET PACK, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundPalette : MonoBehaviour {
	
	public static Hashtable soundTable;
	public static Hashtable categoryTable;
	
	public static SoundPalette instance;
	
	public static AudioSource PlaySound(string str){
		return PlaySound(str, 1.0f, 1.0f);
	}
	
	public static AudioSource PlaySound(string str, float volume){
		return PlaySound(str, volume, 1.0f);
	}

	public static AudioSource PlaySound(string str, float volume, float pitch){
		return PlaySound(str, volume, pitch, Vector3.zero);
	}

	public static AudioSource PlaySound(string str, float volume, float pitch, Vector3 position)
	{
		return PlaySound(str, volume, pitch, position, 1);
	}
	public static AudioSource PlaySound(string str, float volume, float pitch, Vector3 position, float minDistance){
		if(str == "") return null;
		
		if(str.Contains("/")){
			string[] chunks = str.Split('/');
			if(categoryTable.ContainsKey(chunks[0])){
				if(chunks[1] == "Random"){
					SoundCategory c = (SoundCategory)(categoryTable[chunks[0]]);
					return PlaySound(c.sounds[(int)Random.Range(0, c.sounds.Count)], volume, pitch, position, minDistance);
				}
			}
		}

			if(soundTable.ContainsKey(str)){
				return PlaySound((AudioClip)soundTable[str], volume, pitch, position, minDistance);
			}
		
		return null;
	}

	public static AudioSource PlaySound(AudioClip clip, float volume, float pitch, Vector3 position, float minDistance){
		foreach(AudioSource src in instance.channels){
			if(!src.isPlaying){
				src.transform.position = position;
				src.pitch = Mathf.Lerp(0.4f, 1f, Mathf.InverseLerp(0.5f, 2f, Time.timeScale));
				src.clip = clip;
				src.volume = volume * instance.volumeMultiplier;
				src.minDistance = minDistance;
				src.Play();
				return src;
			}
		}
		
		return null;
	}
	
	public int maxChannels = 5;
	public List<AudioClip> sounds;
	public AudioSource[] channels;
	public SoundCategory[] categories;
	public float volumeMultiplier;
	public bool persistent = false;

	void Update () {
		foreach (AudioSource s in channels) {
			s.pitch = Mathf.Lerp(0.4f, 1f, Mathf.InverseLerp(0.5f, 2f, Time.timeScale));
		}
	}

	void Awake(){
		if(persistent){
			if(instance != null && instance != this){
				Destroy(gameObject);
				return;
			}
			else
				DontDestroyOnLoad(gameObject);
		}
			
		instance = this;
		
		soundTable = new Hashtable();
		categoryTable = new Hashtable();
		
		if(channels.Length == 0){
			channels = new AudioSource[maxChannels];
			for(int i = 0; i < maxChannels; i++){
				GameObject go = new GameObject("_SoundPalette_Channel_" + i);
				go.hideFlags = HideFlags.HideAndDontSave;
				go.transform.parent = transform;
				channels[i] = (AudioSource)go.AddComponent<AudioSource>();
				channels[i].dopplerLevel = 0;
                channels[i].minDistance = 15;
			}
		}
		else{
			maxChannels = channels.Length;
		}
		
		foreach(AudioClip clip in sounds){
			soundTable.Add(clip.name, clip);
		}
		
		foreach(SoundCategory c in categories){
			categoryTable.Add(c.name, c);
			foreach(AudioClip clip in c.sounds){
				string nm = clip.name;
				if(soundTable.ContainsKey(c.name + "/" + nm)){
					int i = 0;
					while(soundTable.ContainsKey(c.name + "/" + nm)){
						i++;
						nm = clip.name + i;
					}
				}
				soundTable.Add(c.name + "/" + nm, clip);
			}
			
		}
	}
	
	void OnEnable(){

	}

	void HandleSoundEnabled ()
	{

	}
	
	void OnDisable(){

	}

	[System.Serializable]
	public class SoundCategory{
		public string name;
		public List<AudioClip> sounds;
	}
	
}