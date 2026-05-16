[System.Serializable]
public class AchievementProgress
{
    public string id;
    public int currentValue;
    public bool isUnlocked;

    public AchievementProgress(string id)
    {
        this.id = id;
        currentValue = 0;
        isUnlocked = false;
    }
}