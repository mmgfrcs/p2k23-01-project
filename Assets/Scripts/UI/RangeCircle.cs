using UnityEngine;

public class RangeCircle : MonoBehaviour
{
    [SerializeField] private int segments = 360;
    [SerializeField] private float radius = 1f;
    
    private LineRenderer _line;

    private void Awake()
    {
        _line = gameObject.GetComponent<LineRenderer>();
        Gradient whiteGrad = new Gradient();
        whiteGrad.SetKeys(new []{new GradientColorKey(Color.white, 0f)}, new []{new GradientAlphaKey(1f, 0f)});

        if (_line == null)
        {
            _line = gameObject.AddComponent<LineRenderer>();
            _line.material = new Material(Shader.Find("Sprites/Default"));
        }
        
        _line.useWorldSpace = false;
        _line.widthCurve = AnimationCurve.Constant(0f, 1f, 0.04f);
        _line.colorGradient = whiteGrad;
    }
    
    private void CreatePoints()
    {
        float angle = 0f;
       
        for (int i = 0; i <= segments; i++)
        {
            float x = Mathf.Sin (Mathf.Deg2Rad * angle) * radius;
            float y = Mathf.Cos (Mathf.Deg2Rad * angle) * radius;
                   
            _line.SetPosition (i,new Vector3(x,y, 0f) );
                   
            angle += (360f / segments);
        }
    }

    public void SetRadius(float radius)
    {
        this.radius = radius;
        _line.positionCount = Mathf.CeilToInt(segments * radius) + 1;
        CreatePoints();
    }

    public void ShowLine()
    {
        _line.enabled = true;
        CreatePoints();
    }

    public void HideLine()
    {
        _line.enabled = false;
    }
}