using System.Linq;
using UnityEngine;

namespace Repos.Crag_AudioHandler
{
    [CreateAssetMenu(fileName = "Audio Clips Container", menuName = "Craglex/Audio/Audio Clips Container", order = 0)]
    public class AudioClipContainer : ScriptableObject
    {
        public ClipElement[] clips;

        public AudioClip GetClip(string clipName)
        {
            var clip = clips.FirstOrDefault(c => c.name == clipName);
            return clip?.clip;
        }
    }

    [System.Serializable]
    public class ClipElement
    {
        public string name;
        public AudioClip clip;
    }
}