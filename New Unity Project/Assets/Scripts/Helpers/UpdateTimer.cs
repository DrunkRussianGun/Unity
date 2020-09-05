public class UpdateTimer
{
    private float timer;
    private float interval;

    public UpdateTimer(float intervalInSeconds)
    {
        interval = intervalInSeconds;
    }

    public bool Check(float elapsedSeconds)
    {
        timer += elapsedSeconds;
        if (timer >= interval)
        {
            timer = 0;
            return true;
        }

        return false;
    }
}
