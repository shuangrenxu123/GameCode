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
                .BindToText(tMP_Text)
                .AddTo(tMP_Text);
        }

        public static void TypeWriter(this TMP_Text tMP_Text, string targetText, float duration)
        {
            LMotion.String.Create128Bytes("", targetText, duration)
                .WithRichText()
                .BindToText(tMP_Text)
                .AddTo(tMP_Text);
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
                                   .BindToTMPCharColorA(tMP_Text, i)
                                   .AddTo(tMP_Text);
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
                    .BindToTMPCharPositionY(tMP_Text, i)
                    .AddTo(tMP_Text);

                LMotion.Create(0f, 1f, flyDuration)
                    .WithEase(ease)
                    .WithDelay(i * characterDuration, skipValuesDuringDelay: false)
                    .BindToTMPCharScaleY(tMP_Text, i)
                    .AddTo(tMP_Text);
            }
        }

        public static void Shake(this TMP_Text tMP_Text, float duration, float strength, float frequency = 25f)
        {
            tMP_Text.ForceMeshUpdate(true);

            bool isInfinite = duration < 0f;
            float actualDuration = isInfinite ? float.MaxValue : duration;

            var textInfo = tMP_Text.textInfo;
            var cachedMeshInfo = textInfo.CopyMeshInfoVertexData();

            // Per-character stable noise seeds (so it looks like each char has its own jitter).
            var seedX = new float[textInfo.characterCount];
            var seedY = new float[textInfo.characterCount];
            int instanceSeed = tMP_Text.GetInstanceID();
            for (int i = 0; i < textInfo.characterCount; i++)
            {
                unchecked
                {
                    int hash = (instanceSeed * 397) ^ (i + 1) * 92821;
                    // Spread into [0, 1000) range for PerlinNoise.
                    seedX[i] = ((hash & 0xFFFF) / 65535f) * 1000f;
                    seedY[i] = (((hash >> 16) & 0xFFFF) / 65535f) * 1000f;
                }
            }

            var motion = LMotion.Create(0f, 1f, actualDuration);

            if (!isInfinite)
            {
                motion = motion.WithOnComplete(() =>
                {
                    for (int i = 0; i < textInfo.characterCount; i++)
                    {
                        var charInfo = textInfo.characterInfo[i];
                        if (!charInfo.isVisible) continue;

                        int materialIndex = charInfo.materialReferenceIndex;
                        int vertexIndex = charInfo.vertexIndex;
                        var sourceVertices = cachedMeshInfo[materialIndex].vertices;
                        var destinationVertices = textInfo.meshInfo[materialIndex].vertices;

                        destinationVertices[vertexIndex + 0] = sourceVertices[vertexIndex + 0];
                        destinationVertices[vertexIndex + 1] = sourceVertices[vertexIndex + 1];
                        destinationVertices[vertexIndex + 2] = sourceVertices[vertexIndex + 2];
                        destinationVertices[vertexIndex + 3] = sourceVertices[vertexIndex + 3];
                    }

                    for (int m = 0; m < textInfo.meshInfo.Length; m++)
                    {
                        var meshInfo = textInfo.meshInfo[m];
                        meshInfo.mesh.vertices = meshInfo.vertices;
                        tMP_Text.UpdateGeometry(meshInfo.mesh, m);
                    }
                });
            }

            motion.Bind(_ =>
            {
                float time = Time.unscaledTime * frequency;

                for (int i = 0; i < textInfo.characterCount; i++)
                {
                    var charInfo = textInfo.characterInfo[i];
                    if (!charInfo.isVisible) continue;

                    int materialIndex = charInfo.materialReferenceIndex;
                    int vertexIndex = charInfo.vertexIndex;

                    var sourceVertices = cachedMeshInfo[materialIndex].vertices;
                    var destinationVertices = textInfo.meshInfo[materialIndex].vertices;

                    float nx = Mathf.PerlinNoise(seedX[i], time) * 2f - 1f;
                    float ny = Mathf.PerlinNoise(seedY[i], time) * 2f - 1f;
                    var offset = new Vector3(nx * strength, ny * strength, 0f);

                    destinationVertices[vertexIndex + 0] = sourceVertices[vertexIndex + 0] + offset;
                    destinationVertices[vertexIndex + 1] = sourceVertices[vertexIndex + 1] + offset;
                    destinationVertices[vertexIndex + 2] = sourceVertices[vertexIndex + 2] + offset;
                    destinationVertices[vertexIndex + 3] = sourceVertices[vertexIndex + 3] + offset;
                }

                for (int m = 0; m < textInfo.meshInfo.Length; m++)
                {
                    var meshInfo = textInfo.meshInfo[m];
                    meshInfo.mesh.vertices = meshInfo.vertices;
                    tMP_Text.UpdateGeometry(meshInfo.mesh, m);
                }
            })
            .AddTo(tMP_Text.gameObject);
        }

        public static void SinLoop(this TMP_Text tMP_Text, float duration, float charDaley = 0.2f, float hight = 2)
        {
            tMP_Text.ForceMeshUpdate(true);

            for (int i = 0; i < tMP_Text.textInfo.characterCount; i++)
            {
                LMotion.Create(0f, hight, duration)
                    .WithDelay(charDaley * i)
                    .WithLoops(-1, LoopType.Yoyo)
                    .BindToTMPCharPositionY(tMP_Text, i)
                    .AddTo(tMP_Text);
            }
        }

    }
}
