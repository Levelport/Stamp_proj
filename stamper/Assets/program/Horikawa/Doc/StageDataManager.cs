using UnityEngine;

public static class StageDataManager
{
    private static int currentStageNumber = 1;

    public static void SetStageNumber(int num)
    {
        currentStageNumber = num;
    }

    public static int GetStageNumber()
    {
        return currentStageNumber;
    }
}
