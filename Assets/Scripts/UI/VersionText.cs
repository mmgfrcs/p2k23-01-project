using TMPro;
using UnityEngine;

namespace UI
{
    public class VersionText : MonoBehaviour
    {
        
        private void Start()
        {
            var text = GetComponent<TextMeshProUGUI>();
            text.text = $"{Application.platform}\nVersion {Application.version}";
        }
    }
}