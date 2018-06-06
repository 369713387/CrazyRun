using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
/// <summary>
/// 该脚本是用来将一个对象分配给主混音器
/// 如果我们在编辑器中分配该对象，则将会把该对象的拷贝添加到bundle中，而不是添加到主混音器中
/// </summary>
public class AssignOutputChannel : MonoBehaviour {

    public string mixerGroup;

    private void Awake()
    {
        AudioSource source = GetComponent<AudioSource>();

        if(source == null)
        {
            Debug.LogError("That object don't have any audio source, can't change it's output", gameObject);
            Destroy(this);
            return;
        }

        AudioMixerGroup[] groups = MusicPlayer.instance.mixer.FindMatchingGroups(mixerGroup);

        if(groups.Length == 0)
        {
            Debug.LogErrorFormat(gameObject, "Could not find any group called {0}", mixerGroup);
        }

        for(int i = 0;i < groups.Length; ++i)
        {
            if(groups[i].name == mixerGroup)
            {
                source.outputAudioMixerGroup = groups[i];
                break;
            }
        }
    }
}
