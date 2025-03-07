using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Range(0.0f, 1.0f)]
    public float time;
    public float fullDayLength;
    public float startTime = 0.4f;
    private float timeRate;
    public Vector3 noon;

    [Header("Sun")]
    public Light sun;
    public Gradient sunColor;
    public AnimationCurve sunIntensity;

    [Header("Moon")]
    public Light moon;
    public Gradient moonColor;
    public AnimationCurve moonIntensity;

    [Header("Other Lighting")]
    public AnimationCurve lightingIntensityMultiplyer;
    public AnimationCurve reflectionIntensityMultiplyer;

    public static System.Action<int> eventAtTime;

    private int _curHour = 0;

    public static void OnTimeChanged(int time)
    {
        eventAtTime?.Invoke(time);
    }

    private void Awake()
    {
        GameEvents.SaveInitiated += Save;
        //.LoadInitiated += Load;
        Load();
    }

    private void Start()
    {
        timeRate = 1.0f / fullDayLength;
        time = startTime;
    }

    void Save()
    {
        SaveSystem.Save(time, "DayNightCycle");
    }

    void Load()
    {
        if (SaveSystem.SaveExists("DayNightCycle"))
            startTime = SaveSystem.Load<float>("DayNightCycle");
    }

    private void Update()
    {
        time += timeRate * Time.deltaTime;

        if (time >= 1.0f)
        {
            GameEvents.OnNewDay();
            time = startTime;
        }
        else if (time > timeRate * (_curHour * 60))
        {
            _curHour++;
            OnTimeChanged(_curHour);
        }

        sun.transform.eulerAngles = (time - 0.25f) * noon * 4.0f;
        moon.transform.eulerAngles = (time - 0.75f) * noon * 4.0f;

        sun.intensity = sunIntensity.Evaluate(time);
        moon.intensity = moonIntensity.Evaluate(time);

        sun.color = sunColor.Evaluate(time);
        moon.color = moonColor.Evaluate(time);

        if (sun.intensity <= 0 && sun.gameObject.activeInHierarchy)
            sun.gameObject.SetActive(false);
        else if (sun.intensity > 0 && !sun.gameObject.activeInHierarchy)
            sun.gameObject.SetActive(true);

        if (moon.intensity == 0 && moon.gameObject.activeInHierarchy)
            moon.gameObject.SetActive(false);
        else if (moon.intensity > 0 && !moon.gameObject.activeInHierarchy)
            moon.gameObject.SetActive(true);

        RenderSettings.ambientIntensity = lightingIntensityMultiplyer.Evaluate(time);
        RenderSettings.reflectionIntensity = reflectionIntensityMultiplyer.Evaluate(time);

    }
}