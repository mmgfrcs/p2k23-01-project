using UnityEngine;

public class Singleton<T> : MonoBehaviour where T: Component
{
    public static T Instance { get; private set; }

    protected virtual void Awake()
    {
        AssignSingleton();
    }
    
    protected virtual void OnDisable()
    {
        DeassignSingleton();
    }

    protected virtual void OnEnable()
    {
        AssignSingleton();
    }
    
    protected virtual void AssignSingleton()
    {
        if (Instance == null)
        {
            Instance = FindObjectOfType<T>();
            if (Instance == null) Instance = new GameObject(typeof(T).Name).AddComponent<T>();
        }
        else
        {
            var obj = FindObjectOfType<T>();
            if (Instance != obj) Destroy(obj);
        }
    }
    
    protected virtual void DeassignSingleton()
    {
        Instance = null;
    }
}
