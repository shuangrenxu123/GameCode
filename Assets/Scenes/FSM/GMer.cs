using UnityEngine;
using UnityEngine.InputSystem;

public class GMer : MonoBehaviour
{
    public GameObject m_Prefab;
    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            var pos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Instantiate(m_Prefab, (Vector2)pos, Quaternion.identity);
        }
    }
}
