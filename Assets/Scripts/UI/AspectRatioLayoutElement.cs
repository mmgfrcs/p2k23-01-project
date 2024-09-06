using UnityEngine;
using UnityEngine.UI;

namespace AdInfinitum.UI
{
    [RequireComponent(typeof(RectTransform))]
    [ExecuteAlways]
    public class AspectRatioLayoutElement : AspectRatioFitter, ILayoutElement
    {
        // Start is called before the first frame update
        private RectTransform _rect;
    
        private RectTransform Rect { get { if (_rect == null) _rect = GetComponent<RectTransform>(); return _rect; } }
    
        public float preferredWidth { get; private set; }
        public float preferredHeight { get; private set; }
    
        /// <summary>
        /// The minimum width this layout element may be allocated. Always -1 for this component.
        /// </summary>
        public float minWidth => -1;
        /// <summary>
        /// The extra relative width this layout element should be allocated if there is additional available space. Always -1 for this component.
        /// </summary>
        public float flexibleWidth => -1;
        /// <summary>
        /// The minimum height this layout element may be allocated. Always -1 for this component.
        /// </summary>
        public float minHeight => -1;
        /// <summary>
        /// The extra relative height this layout element should be allocated if there is additional available space. Always -1 for this component.
        /// </summary>
        public float flexibleHeight => -1;

        public int layoutPriority => 1;
    
        protected override void Start()
        {
            base.Start();
            _rect = GetComponent<RectTransform>();
        }

        public void CalculateLayoutInputHorizontal() {}

        public void CalculateLayoutInputVertical() {}
    
        protected override void OnEnable()
        {
            base.OnEnable();
            SetLayout();
        }

        protected override void OnTransformParentChanged()
        {
            base.OnTransformParentChanged();
            SetLayout();
        }

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            SetLayout();
        }

        protected override void OnDisable()
        {
            SetLayout();
            base.OnDisable();
        }

        private void SetLayout()
        {        
            if (!IsActive())
                return;
        
            preferredWidth = -1;
            preferredHeight = -1;
        
            switch (aspectMode)
            {
                case AspectMode.HeightControlsWidth:
                    preferredWidth = Rect.rect.width;
                    break;
                case AspectMode.WidthControlsHeight:
                    preferredHeight = Rect.rect.height;
                    break;
                case AspectMode.EnvelopeParent:
                case AspectMode.FitInParent:
                    preferredWidth = Rect.rect.width;
                    preferredHeight = Rect.rect.height;
                    break;
            }
        
            LayoutRebuilder.MarkLayoutForRebuild(transform as RectTransform);
        }
    }
}

