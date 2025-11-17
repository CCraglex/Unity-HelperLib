using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Repos.Crag_AudioHandler
{
    public class AudioHandler : MonoBehaviour
    {
        [SerializeField] private AudioClipContainer strClips;
        
        private const int DefaultChannels = 6;
        private static AudioHandler _instance;

        private Queue<AudioSource> _free;
        private HashSet<AudioPlayData> _busy;

        private Dictionary<string,float> volumeValues;

        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            volumeValues = new Dictionary<string, float>();
            volumeValues.Add("Default",1f);

            _free = new Queue<AudioSource>();
            _busy = new HashSet<AudioPlayData>();

            var parent = new GameObject("AudioSources").transform;
            parent.SetParent(transform, false);

            for (int i = 0; i < DefaultChannels; i++)
                CreateSource(parent, i);
        }

        private void CreateSource(Transform parent, int index)
        {
            var src = new GameObject("Source_" + index).AddComponent<AudioSource>();
            src.transform.SetParent(parent);
            _free.Enqueue(src);
        }

        public static void Play(AudioClip clip,string volumeChannel,float pitch = 1,float pitchDiff = 0,bool loop = false)
        {
            if (!_instance._free.TryDequeue(out var src))
            {
                Debug.LogWarning("No free audio channels for: " + clip.name);
                return;
            }
            Coroutine coroutine = _instance.StartCoroutine(_instance.ReturnWhenDone(src));
            AudioPlayData clipData = new(volumeChannel, src, clip, coroutine);
            
            src.volume = _instance.volumeValues[volumeChannel];
            src.clip = clip;
            src.Play();
            src.pitch = Random.Range(pitch - pitchDiff, pitch + pitchDiff);
            src.loop = loop;
            
            _instance._busy.Add(clipData);
        }

        public static void Play(string clipName,string volumeChannel,float pitch = 1,float pitchDiff = 0,bool loop = false)
            => Play(_instance.strClips.GetClip(clipName), volumeChannel,pitch,pitchDiff, loop);
        
        public static void Stop(AudioSource src)
        {
            var apd = _instance._busy.FirstOrDefault(x => src);
            if (apd == null)
            {
                Debug.LogWarning("Source not tracked by AudioHandler: " + src.name);
                src.Stop();
                return;
            }

            src.Stop();
            _instance.StopCoroutine(apd.PlaybackCoroutine);

            _instance._busy.Remove(apd);
            _instance._free.Enqueue(src);
        }

        private IEnumerator ReturnWhenDone(AudioSource src)
        {
            var apd = _instance._busy.First(x => src);
            
            while (src.isPlaying)
                yield return null;

            _busy.Remove(apd);
            _free.Enqueue(src);
        }

        public static void AddChannels(int amount)
        {
            var ins = _instance;
            var parent = ins.transform.Find("AudioSources");
            int start = ins._free.Count + ins._busy.Count;

            for (int i = 0; i < amount; i++)
                ins.CreateSource(parent, start + i);
        }

        public static void RemoveChannels(int amount)
        {
            var ins = _instance;

            if (ins._free.Count < amount)
            {
                Debug.LogError("Cannot remove channels: some are in use.");
                return;
            }

            for (int i = 0; i < amount; i++)
                Destroy(ins._free.Dequeue().gameObject);
        }

        public static void SetVolume(string channel,float v)
        {
            IEnumerable<AudioPlayData> clips = _instance._busy.Where(x => x.VolumeValue == channel);
            
            v = Mathf.Clamp01(v);

            foreach (var s in clips)
            {
                s.VolumeValue = channel;
                s.Source.volume = _instance.volumeValues[channel];
            }
        }

        public static void SetVolume(string channel,int v)
            => SetVolume(channel,Mathf.Clamp(v, 0, 100) / 100f);
    }

    public class AudioPlayData
    {
        public AudioPlayData(string volumeChannel, AudioSource source,AudioClip clip, Coroutine coroutine)
        {
            VolumeValue = volumeChannel;
            Source = source;
            PlaybackCoroutine =  coroutine;
            
            source.clip = clip;
        }
        
        public string VolumeValue;
        public readonly AudioSource Source;
        public readonly Coroutine PlaybackCoroutine;
    }
}
