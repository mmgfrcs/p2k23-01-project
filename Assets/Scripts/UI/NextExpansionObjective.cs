using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class NextExpansionObjective : MonoBehaviour
{
    private TextMeshProUGUI _text;
    private Map _map;

    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        _map = FindObjectOfType<Map>();
    }

    private void Update()
    {
        if (GameManager.Instance.Wave == 0) _text.text = "";
        else if (_map.GetNextExpansionWave(GameManager.Instance.Wave) - GameManager.Instance.Wave == 1) _text.text = $"Map Expansion imminent!";
        else _text.text = $"Next Map Expansion in {_map.GetNextExpansionWave(GameManager.Instance.Wave) - GameManager.Instance.Wave} Waves"; 
    }
}
