using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace AdInfinitum.UI
{
    public class MainMenuTitle : MonoBehaviour
    {
        // Start is called before the first frame update
        public float fadeDuration = 3f;
        public float fadeDelay = 1f;
        private MaskableGraphic _graphic;
        private CanvasGroup _group;

        private static bool _hasFaded = false;
    
        void Start()
        {        
            _group = GetComponent<CanvasGroup>();
            if (_group != null)
            {
                _group.alpha = 0f;
                _group.interactable = false;             
                if (_hasFaded) _group.DOFade(1f, 1f).OnComplete(() => { _group.interactable = true; });
                else _group.DOFade(1f, fadeDuration).SetDelay(fadeDelay)
                    .OnComplete(() => { _group.interactable = true; _hasFaded = true; });
             
                return;
            }
         
            _graphic = GetComponent<MaskableGraphic>();
            if (_graphic != null)
            {
                _graphic.color = new Color(_graphic.color.r, _graphic.color.g, _graphic.color.b, 0f);
                if (_hasFaded) _graphic.DOColor(new Color(_graphic.color.r, _graphic.color.g, _graphic.color.b, 1f), 1f);
                else _graphic.DOColor(new Color(_graphic.color.r, _graphic.color.g, _graphic.color.b, 1f), fadeDuration)
                    .SetDelay(fadeDelay).OnComplete(() => _hasFaded = true);
             
                return;
            }
        
            Debug.LogError("[MainMenuTitle] A Graphic or Group is required", gameObject);
        }
    
    }
}
