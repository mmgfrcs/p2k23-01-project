using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuTitle : MonoBehaviour
{
    // Start is called before the first frame update
    public float fadeDuration = 3f;
    public float fadeDelay = 1f;
    private MaskableGraphic _graphic;
    private CanvasGroup _group;
    void Start()
    {        
        _group = GetComponent<CanvasGroup>();
         if (_group != null)
         {
             _group.alpha = 0f;
             _group.interactable = false;
             _group.DOFade(1f, fadeDuration).SetDelay(fadeDelay).onComplete += () =>
             {
                 _group.interactable = true;
             };
             return;
         }
             
        _graphic = GetComponent<MaskableGraphic>();
        if (_graphic != null)
        {
            _graphic.color = new Color(_graphic.color.r, _graphic.color.g, _graphic.color.b, 0f);
            _graphic.DOColor(new Color(_graphic.color.r, _graphic.color.g, _graphic.color.b, 1f), fadeDuration).SetDelay(fadeDelay);
            return;
        }
        
        Debug.LogError("[MainMenuTitle] A Graphic or Group is required", gameObject);
    }
    
}
