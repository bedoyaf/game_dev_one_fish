using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

/// <summary>
/// Starts playing a set of images in order after some delay.
/// 
/// After it is done playing, will start the game (hard coded)
/// Also has a skip button feature, that will trigger the action immediately.
/// 
/// </summary>
public class CutsceneUIScript : MonoBehaviour
{

    // How long before the cutscenes start playing
    [SerializeField] private float startDelay = 0.5f;

    [SerializeField] private Image mainImage;

    [SerializeField] private RectTransform imageTransform;

    public List<CutsceneData> scenes;

    private Coroutine thisRoutine;

    [SerializeField] private bool endingCutscene = false;

    void Start()
    {
        thisRoutine = StartCoroutine(PlayCutscene());
    }

    public IEnumerator PlayCutscene()
    {
        yield return new WaitForSeconds(startDelay);

        foreach(var data in scenes)
        {
            mainImage.sprite = data.sprite;
            imageTransform.anchoredPosition = data.startOffset;
            imageTransform.localScale = data.startScale;

            // Animate transitions (myself, cause scale + move is broken)
            imageTransform.DOAnchorPos(data.endOffset, data.time);
            imageTransform.DOScale(data.endScale, data.time);


            yield return new WaitForSeconds(data.time);
        }

        SkipCutscene();
    }


    public InputActionReference skipAction;

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
        Debug.Log("skipping cutscene");

        SkipCutscene();
    }

    private void SkipCutscene()
    {
        StopCoroutine(thisRoutine);

        if(!endingCutscene)  GameManager.Instance.StartGame();
        else GameManager.Instance.TransitionScene("MainMenuScene");
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
