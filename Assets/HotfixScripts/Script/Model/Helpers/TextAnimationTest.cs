using UnityEngine;
using Utilities.TextAnimation;

public class TextAnimationTest : MonoBehaviour
{
    [SerializeField]
    TMPro.TMP_Text tmpText;
    [SerializeField]
    float arg1;
    [SerializeField]
    float arg2;
    void Start()
    {
        // tmpText.FlyEnter(arg1);
        tmpText.GarbledCode("测试文本",arg1 * 2);
    }
}
