using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class CutSceneVideoPlayer : MonoBehaviour
{
    [Header("Video Settings")]
    public VideoPlayer videoPlayer;
    public VideoClip videoClip;

    private bool videoStarted = false;

    void Start()
    {
        // 确保视频播放器设置正确
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();

        if (videoPlayer != null && videoClip != null)
        {
            SetupVideoPlayer();
        }
    }

    void Update()
    {
        // 检查视频是否播放完成
        if (videoStarted && videoPlayer != null && !videoPlayer.isPlaying)
        {
            OnVideoFinished();
        }

        // 按ESC键跳过视频
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SkipVideo();
        }
    }

    private void SetupVideoPlayer()
    {
        videoPlayer.clip = videoClip;
        videoPlayer.renderMode = VideoRenderMode.CameraFarPlane; // 全屏播放
        videoPlayer.isLooping = false;
        videoPlayer.waitForFirstFrame = true;

        // 设置音频
        videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;

        // 播放完成事件
        videoPlayer.loopPointReached += OnVideoFinished;
    }

    /// <summary>
    /// 开始播放视频（在剧情结束后调用）
    /// </summary>
    public void PlayVideo()
    {
        if (videoPlayer != null && videoClip != null)
        {
            videoPlayer.Play();
            videoStarted = true;

            // 隐藏鼠标
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Debug.LogWarning("视频播放器或视频文件未设置，直接跳转到GameOver");
            LoadGameOver();
        }
    }

    /// <summary>
    /// 视频播放完成
    /// </summary>
    private void OnVideoFinished(VideoPlayer vp = null)
    {
        LoadGameOver();
    }

    /// <summary>
    /// 跳过视频
    /// </summary>
    public void SkipVideo()
    {
        if (videoPlayer != null && videoPlayer.isPlaying)
        {
            videoPlayer.Stop();
        }
        LoadGameOver();
    }

    /// <summary>
    /// 加载GameOver场景
    /// </summary>
    private void LoadGameOver()
    {
        SceneManager.LoadScene("GameOver");
    }
}