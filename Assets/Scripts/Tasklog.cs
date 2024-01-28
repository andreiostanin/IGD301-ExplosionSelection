public class TaskLog 
{
    private int errorCount = 0;

    private float startTimestamp = -1f;
    private float endTimestamp = -1f;

    private string objectName;

    public TaskLog(float startTimestamp, string objectName)
    {
        this.startTimestamp = startTimestamp;
        this.objectName = objectName;
    }

    public int GetErrorCount()
    {
        return this.errorCount;
    }

    public void IncrementErrorCount()
    {
        this.errorCount++;
    }

    public float GetStartTimestamp()
    {
        return this.startTimestamp;
    }

    public void SetStartTimestamp(float startTimestamp)
    {
        this.startTimestamp = startTimestamp;
    }

    public float GetEndTimestamp()
    {
        return this.endTimestamp;
    }

    public void SetEndTimestamp(float endTimestamp)
    {
        this.endTimestamp = endTimestamp;
    }

    public string GetObjectName()
    {
        return this.objectName;
    }

    public void SetObjectName(string objectName)
    {
        this.objectName = objectName;
    }
}
