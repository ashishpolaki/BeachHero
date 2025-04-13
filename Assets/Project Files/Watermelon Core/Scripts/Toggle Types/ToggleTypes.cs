using UnityEngine;
using UnityEngine.Serialization;

namespace Watermelon
{
    [System.Serializable]
    public abstract class ToggleType<T>
    {
        [SerializeField] bool enabled;
        public bool Enabled => enabled;

        [FormerlySerializedAs("newValue")]
        [SerializeField] T value;
        public T Value => value;

        public T Handle(T value)
        {
            if (enabled)
            {
                return this.value;
            }
            else
            {
                return value;
            }
        }
    }

    [System.Serializable]
    public class BoolToggle : ToggleType<bool> { }

    [System.Serializable]
    public class FloatToggle : ToggleType<float> { }

    [System.Serializable]
    public class IntToggle : ToggleType<int> { }

    [System.Serializable]
    public class LongToggle : ToggleType<long> { }

    [System.Serializable]
    public class StringToggle : ToggleType<string> { }

    [System.Serializable]
    public class DoubleToggle : ToggleType<double> { }

    [System.Serializable]
    public class ObjectToggle : ToggleType<GameObject> { }

    [System.Serializable]
    public class AudioClipToggle : ToggleType<AudioClip> { }
}