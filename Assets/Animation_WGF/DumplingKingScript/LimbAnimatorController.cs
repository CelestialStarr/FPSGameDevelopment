using System.Collections.Generic;
using UnityEngine;

public class LimbAnimatorController : MonoBehaviour
{
    [System.Serializable]
    public class AnimationData
    {
        public string stateName;
        public List<Transform> frames;
        public float frameRate = 10f;
    }

    public List<AnimationData> animations;
    private int currentFrame = 0;
    private float timer = 0f;
    private AnimationData currentAnim;

    void Update()
    {
        if (currentAnim == null || currentAnim.frames.Count == 0) return;

        timer += Time.deltaTime;
        if (timer > 1f / currentAnim.frameRate)
        {
            currentFrame = (currentFrame + 1) % currentAnim.frames.Count;
            ApplyFrame(currentAnim.frames[currentFrame]);
            timer = 0f;
        }
    }

    void ApplyFrame(Transform frame)
    {
        transform.localPosition = frame.localPosition;
        transform.localRotation = frame.localRotation;
        transform.localScale = frame.localScale;
    }

    public void Play(string name)
    {
        currentAnim = animations.Find(a => a.stateName == name);
        currentFrame = 0;
        timer = 0f;
    }
}
