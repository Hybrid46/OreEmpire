using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public MapGen mapGen;
    public ResourceManager resourceManager;

    private float elapsedTimeAfterLastTick = 0.0f;
    public GameSettings gameSettings;

    void Start()
    {
        Application.targetFrameRate = 60;
    }

    void Update()
    {
        elapsedTimeAfterLastTick += Time.deltaTime;

        if (elapsedTimeAfterLastTick >= gameSettings.tickPeriod)
        {
            elapsedTimeAfterLastTick -= gameSettings.tickPeriod;

            //TODO -> Tick!
        }
    }
}
