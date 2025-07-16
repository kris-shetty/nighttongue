using UnityEngine;

public class ThrowableObject : MonoBehaviour
{
    [SerializeField] public ThrowablePropertiesSO _properties;

    public float MaxVerticalHeight => _properties.MaxVerticalHeight;
    public float MaxHoldTime => _properties.MaxHoldTime;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
