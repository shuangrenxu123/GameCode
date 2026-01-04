using LitMotion;
using LitMotion.Extensions;
using TMPro;
using UnityEngine;

namespace Utilities.TextAnimation
{
    public static class TextAnimationHelper
    {

        public static void GarbledCode(this TMP_Text tMP_Text, string defaultStr, float duration, string targetText = null)
        {
            if (string.IsNullOrEmpty(targetText))
            {
                targetText = tMP_Text.text;
            }
            LMotion.String.Create128Bytes("", targetText, duration)
                .WithRichText()
                .WithScrambleChars(defaultStr)
                .BindToText(tMP_Text);
        }

        public static void TypeWriter(this TMP_Text tMP_Text, string targetText, float duration)
        {
            LMotion.String.Create128Bytes("", targetText, duration)
                .WithRichText()
                .BindToText(tMP_Text);
        }

        public static void Fade(this TMP_Text tMP_Text,
            float fadeDuration = 0.5f,
            float characterDuration = 0.05f,
            Ease ease = Ease.OutSine)
        {
            tMP_Text.alpha = 0;
            tMP_Text.ForceMeshUpdate(true);
            for (int i = 0; i < tMP_Text.textInfo.characterCount; i++)
            {
                LMotion.Create(0f, 1f, fadeDuration)
                                   .WithDelay(i * characterDuration)
                                   .BindToTMPCharColorA(tMP_Text, i);
            }
        }

        public static void FlyEnter(this TMP_Text tMP_Text,
            float flyDuration = 0.5f,
            float characterDuration = 0.05f,
            float offsetY = -10f,
            Ease ease = Ease.OutSine)
        {
            tMP_Text.ForceMeshUpdate(true);

            for (int i = 0; i < tMP_Text.textInfo.characterCount; i++)
            {
                LMotion.Create(offsetY, 0, flyDuration)
                    .WithEase(ease)
                    .WithDelay(i * characterDuration, skipValuesDuringDelay: false)
                    .BindToTMPCharPositionY(tMP_Text, i);

                LMotion.Create(0f, 1f, flyDuration)
                    .WithEase(ease)
                    .WithDelay(i * characterDuration, skipValuesDuringDelay: false)
                    .BindToTMPCharScaleY(tMP_Text, i);
            }
        }

        public static void Punch(this TMP_Text tMP_Text, float duration, Vector3 strength, bool charRandom = false)
        {
            tMP_Text.ForceMeshUpdate(true);

            for (int i = 0; i < tMP_Text.textInfo.characterCount; i++)
            {
                var strengthValue = strength;
                if (charRandom)
                {
                    strength *= Random.Range(-1, 1);
                }
                LMotion.Punch.Create(Vector3.zero, strengthValue, duration)
                .WithLoops(-1, LoopType.Flip)
                .BindToTMPCharPosition(tMP_Text, i);
            }
        }
        public static void RandomMove(this TMP_Text tMP_Text, float duration, Vector3 strength)
        {
            tMP_Text.ForceMeshUpdate(true);

            for (int i = 0; i < tMP_Text.textInfo.characterCount; i++)
            {
                var strengthValue = new Vector3(
                    Random.Range(-strength.x, strength.x),
                    Random.Range(-strength.y, strength.y),
                    Random.Range(-strength.z, strength.z)
                    );

                var info = tMP_Text.textInfo.characterInfo[i];

            }
        }
    }
}
