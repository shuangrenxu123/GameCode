using UnityEngine;

namespace CharacterController.Camera
{
    /// <summary>
    /// 用于标注字段的中文含义，在Inspector中显示
    /// </summary>
    public class LabText : PropertyAttribute
    {
        public string labelText;

        /// <summary>
        /// 标注字段的中文含义
        /// </summary>
        /// <param name="text">中文含义</param>
        public LabText(string text)
        {
            labelText = text;
        }
    }
}