using UnityEngine;
using UnityEngine.Video;

public class StartupVideoController : MonoBehaviour
{
    [Header("Video")]
    public VideoPlayer videoPlayer;

    [Header("UI Groups")]
    public GameObject videoLayer;   // 播放视频的遮罩层（Panel/RawImage的父物体）
    public GameObject mainUILayer;  // 你的正常UI父物体（开始按钮/登录面板等）

    void Awake()
    {
        // 防止循环
        if (videoPlayer != null) videoPlayer.isLooping = false;

        // 初始状态：显示视频层，隐藏主UI
        if (videoLayer != null) videoLayer.SetActive(true);
        if (mainUILayer != null) mainUILayer.SetActive(false);
    }

    void Start()
    {
        if (videoPlayer == null) return;

        // 播完回调
        videoPlayer.loopPointReached += OnVideoFinished;

        // 播放
        videoPlayer.Play();
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        EndVideoAndShowUI();
    }

    // 给“跳过/开始”按钮调用
    public void SkipVideo()
    {
        EndVideoAndShowUI();
    }

    private void EndVideoAndShowUI()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop(); // 停止在最后一帧也行，但Stop更干净
        }

        if (videoLayer != null) videoLayer.SetActive(false);
        if (mainUILayer != null) mainUILayer.SetActive(true);
    }
}
