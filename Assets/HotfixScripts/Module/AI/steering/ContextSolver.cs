using UnityEngine;

public class ContextSolver
{
    AiData aiData;
    Steering steering;
    Detector detector;
    Vector2 resultDirection = Vector2.zero;
    public ContextSolver(AiData data)
    {
        aiData = data;
        steering = new Steering(aiData);
        detector = new Detector(aiData);

    }
    public Vector2 GetDirectionToMove()
    {
        //Ö´ÐÐËÑË÷Æ÷
        detector.Find();
        float[] danger = new float[8];
        float[] interest = new float[8];

        steering.GetSteering(ref interest, ref danger);
        for (int i = 0; i < 8; i++)
        {
            interest[i] = Mathf.Clamp01(interest[i] - danger[i]);
        }
        Vector2 outputDirection = Vector2.zero;
        for (int i = 0; i < 8; i++)
        {
            outputDirection += Directions.eightDirections[i] * interest[i];
        }

        outputDirection.Normalize();

        resultDirection = outputDirection;
        return resultDirection;
    }
}
