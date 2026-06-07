using UnityEngine;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Starts playing a set of images in order after some delay.
/// 
/// Slides advance automatically based on their individual time, 
/// or can be manually advanced by triggering the skip action.
/// After the last slide is done, it will transition to the next scene.
/// </summary>
public class SlideShowUIScript : MonoBehaviour
{
    // How long before the cutscenes start playing
    [SerializeField] private float startDelay = 0.5f;

    [SerializeField] private Image mainImage;
    [SerializeField] private RectTransform imageTransform;

    public List<CutsceneData> scenes;

    [SerializeField] private bool endingCutscene = false;
    public InputActionReference skipAction;

    private int currentSlideIndex = 0;
    private Coroutine slideTimerRoutine;

    void Start()
    {
        StartCoroutine(StartWithDelay());
    }

    private IEnumerator StartWithDelay()
    {
        yield return new WaitForSeconds(startDelay);
        PlayCurrentSlide();
    }

    private void PlayCurrentSlide()
    {
        if (currentSlideIndex >= scenes.Count)
        {
            FinishCutscene();
            return;
        }

        imageTransform.DOKill();

        if (slideTimerRoutine != null)
        {
            StopCoroutine(slideTimerRoutine);
        }

        var data = scenes[currentSlideIndex];
        mainImage.sprite = data.sprite;
        imageTransform.anchoredPosition = data.startOffset;
        imageTransform.localScale = data.startScale;

        imageTransform.DOAnchorPos(data.endOffset, data.time);
        imageTransform.DOScale(data.endScale, data.time);

        slideTimerRoutine = StartCoroutine(WaitAndNextSlide(data.time));
    }

    private IEnumerator WaitAndNextSlide(float time)
    {
        yield return new WaitForSeconds(time);
        NextSlide();
    }

    private void NextSlide()
    {
        currentSlideIndex++;
        PlayCurrentSlide();
    }

    void OnEnable()
    {
        skipAction.action.performed += SkipAction;
        skipAction.action.Enable();
    }

    void OnDisable()
    {
        skipAction.action.performed -= SkipAction;
        skipAction.action.Disable();
    }

    private void SkipAction(InputAction.CallbackContext ctx)
    {
        NextSlide();
    }

    private void FinishCutscene()
    {
        imageTransform.DOKill();
        if (slideTimerRoutine != null) StopCoroutine(slideTimerRoutine);

        if (!endingCutscene)
            GameManager.Instance.StartGame();
        else
            GameManager.Instance.TransitionScene("MainMenuScene");
    }

    [Serializable]
    public class CutsceneData
    {
        public Sprite sprite;
        public Vector2 startOffset;
        public Vector2 endOffset;
        public float time;
        public Vector3 startScale = Vector3.one;
        public Vector3 endScale = Vector3.one;
    }
}