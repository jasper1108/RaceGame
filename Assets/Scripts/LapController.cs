using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

/// Tracks race timers, checkpoint progress, lap completion, and the player finish flow.
public class LapController : MonoBehaviour
{
    public CarController carController;
    public CarController[] racers = new CarController[0];
    public GameObject UI, StartPnl, FinishedPnl, SpeedPnl, TimerLapPnl;
    public GameObject[] AiCars;
    public UI uiController;
    public TMP_Text startTMP, timerTMP;
    public float timer = 5f;
    public bool startTimer = true, gameTimer, Finished;
    public int expectedRacers = 6, playerPosition;
    public List<float> lapTimes = new List<float>();
    public int totalLaps = 1, lapsDone;
    public TMP_Text lapTMP, lapTimeTMP, PositionTMP, TotalTimeTMP;
    RacerProgress[] racerProgress = new RacerProgress[0];
    CarController firstFinisher;
    bool playerFinished, finishMenuLinked;

    // Tracks per-racer state such as lap start time, checkpoint status, and finish state.
    class RacerProgress
    {
        public int lapsDone;
        public bool lapStarted, checkpointPassed, finished, isPlayer;
        public float lapStartTime;
    }

    // Initializes the manager and prepares the race state for all racers.
    void Awake()
    {
        if (uiController == null)
        {
            uiController = FindAnyObjectByType<UI>();
        }

        CheckAI();
        if (racers != null)
        {
            if (expectedRacers <= 0 || expectedRacers < racers.Length)
            {
                expectedRacers = racers.Length;
            }

            racerProgress = new RacerProgress[racers.Length];
            for (int i = 0; i < racers.Length; i++)
            {
                if (racers[i] != null)
                {
                    racerProgress[i] = new RacerProgress();
                }
            }
        }

        if (uiController != null)
        {
            uiController.GameUI = UI;
        }
    }

    // Updates the countdown, timer, lap text, and finish HUD for the current race state.
    void Update()
    {

        LinkMenuButton(uiController);
        if (startTimer && !Finished)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                startTimer = false;
                gameTimer = true;
            }
        }
        if (gameTimer && !Finished)
        {
            timer += Time.deltaTime;
        }
        if (!playerFinished && lapTimeTMP != null)
        {
            lapTimeTMP.gameObject.SetActive(false);
        }
        if (!playerFinished && PositionTMP != null)
        {
            PositionTMP.gameObject.SetActive(false);
        }
        if (!startTimer && lapTMP != null)
        {
            lapTMP.gameObject.SetActive(true);
            lapTMP.text = "Lap: " + lapsDone + "/" + totalLaps;
        }
        if (firstFinisher != null)
        {
            Finished = true;
            gameTimer = false;
        }
        UImanager();
    }
    // Hides AI cars when they are disabled in the menu settings.
    void CheckAI()
    {
        if (uiController == null)
        {
            return;
        }

        if (!uiController.AIEnabled)
        {
            if (AiCars != null)
            {
                for (int i = 0; i < AiCars.Length; i++)
                {
                    if (AiCars[i] != null)
                    {
                        AiCars[i].SetActive(false);
                    }
                }
            }
        }
    }

    // Switches the active HUD panels for countdown, race play, and finish states.
    void UImanager()
    {
        if (playerFinished)
        {
            if (UI != null) UI.SetActive(true);
            if (FinishedPnl != null) FinishedPnl.SetActive(true);
            if (SpeedPnl != null) SpeedPnl.SetActive(false);
            if (TimerLapPnl != null) TimerLapPnl.SetActive(false);
            if (TotalTimeTMP != null) TotalTimeTMP.text = "Total Time: " + timer.ToString("F2");
        }
        else if (startTimer)
        {
            if (UI != null) UI.SetActive(true);
            if (StartPnl != null) StartPnl.SetActive(true);
            if (SpeedPnl != null) SpeedPnl.SetActive(false);
            if (TimerLapPnl != null) TimerLapPnl.SetActive(false);
            if (startTMP != null) startTMP.text = Mathf.Ceil(timer).ToString();
        }
        else
        {
            if (StartPnl != null) StartPnl.SetActive(false);
            if (SpeedPnl != null) SpeedPnl.SetActive(true);
            if (TimerLapPnl != null) TimerLapPnl.SetActive(true);
            if (timerTMP != null) timerTMP.text = timer.ToString("F2");
        }
    }

    // Detects a lap crossing or checkpoint pass when another collider enters the finish trigger.
    void OnTriggerEnter(Collider other)
    {
        if (startTimer || Finished)
        {
            return;
        }

        CarController racer = other.GetComponentInParent<CarController>();
        if (racer == null)
        {
            return;
        }

        RacerProgress progress = GetProgress(racer);
        progress.isPlayer |= other.CompareTag("Player") || racer.CompareTag("Player");
        if (progress.finished)
        {
                return;
        }

        if (!progress.lapStarted)
        {
            progress.lapStarted = true;
            progress.lapStartTime = timer;
            return;
        }

        if (!progress.checkpointPassed)
        {
            return;
        }

        progress.checkpointPassed = false;
        float completedLapTime = timer - progress.lapStartTime;
        progress.lapStartTime = timer;
        progress.lapsDone++;

        if (progress.isPlayer)
        {
            lapsDone = progress.lapsDone;
            lapTimes.Add(completedLapTime);
        }

        if (progress.lapsDone >= totalLaps)
        {
            RegisterFinish(racer, progress);
        }
    }

    // Marks the provided racer as having passed the active checkpoint.
    public void RegisterCheckpoint(CarController racer)
    {
        if (racer == null || startTimer)
        {
            return;
        }

        RacerProgress progress = GetProgress(racer);
        if (!progress.finished)
        {
            progress.checkpointPassed = true;
        }
    }

    // Returns true when a racer has already completed the race.
    public bool IsRacerFinished(CarController racer)
    {
        int index = GetRacerIndex(racer);
        if (index < 0 || index >= racerProgress.Length || racerProgress[index] == null)
        {
            return false;
        }

        return racerProgress[index].finished;
    }

    // Returns the finishing placement for the provided racer if it is the winner.
    public int GetFinishPosition(CarController racer)
    {
        return racer != null && racer == firstFinisher ? 1 : 0;
    }

    // Finds the tracking slot assigned to a specific racer.
    int GetRacerIndex(CarController racer)
    {
        if (racer == null)
        {
            return -1;
        }

        for (int i = 0; i < racers.Length; i++)
        {
            if (racers[i] == racer)
            {
                return i;
            }
        }

        return -1;
    }

    // Retrieves the stored progress data for a racer and creates it if it does not exist yet.
    RacerProgress GetProgress(CarController racer)
    {
        int index = GetRacerIndex(racer);
        if (index < 0)
        {
            CarController[] newRacers = new CarController[racers.Length + 1];
            System.Array.Copy(racers, newRacers, racers.Length);
            newRacers[racers.Length] = racer;
            racers = newRacers;

            RacerProgress[] newProgress = new RacerProgress[racerProgress.Length + 1];
            System.Array.Copy(racerProgress, newProgress, racerProgress.Length);
            racerProgress = newProgress;

            index = racers.Length - 1;
        }

        if (racerProgress[index] == null)
        {
            racerProgress[index] = new RacerProgress();
        }

        return racerProgress[index];
    }

    // Finalizes the racer as finished and updates the player-specific finish UI.
    void RegisterFinish(CarController racer, RacerProgress progress)
    {
        if (racer == null)
        {
            return;
        }

        if (firstFinisher == null)
        {
            firstFinisher = racer;
        }

        Collider[] carColliders = racer.GetComponentsInChildren<Collider>(true);
        Rigidbody[] rigidBodies = racer.GetComponentsInChildren<Rigidbody>(true);
        for (int i = 0; i < carColliders.Length; i++)
        {
            if (carColliders[i] != null)
            {
                carColliders[i].enabled = false;
            }
        }
        for (int i = 0; i < rigidBodies.Length; i++)
        {
            if (rigidBodies[i] != null)
            {
                rigidBodies[i].useGravity = false;
            }
        }

        progress.finished = true;
        playerPosition = racer == firstFinisher ? 1 : 0;
        Debug.Log(racer.name + (racer == firstFinisher ? " crossed the finish line first" : " finished"));

        if (progress.isPlayer)
        {
            playerFinished = true;
            if (PositionTMP != null)
            {
                PositionTMP.gameObject.SetActive(true);
                PositionTMP.text = racer == firstFinisher ? "Position: 1st" : "Position: Finished";
            }

            if (lapTimeTMP != null)
            {
                lapTimeTMP.gameObject.SetActive(true);
                string fastestLap = lapTimes.Count > 0
                    ? "Fastest Lap: " + Mathf.Min(lapTimes.ToArray()).ToString("F2")
                    : "Fastest Lap: --";
                lapTimeTMP.text = fastestLap;
            }
        }
    }

    // Connects the finish screen menu button to the menu navigation callback once available.
    void LinkMenuButton(UI uiController)
    {
        if (UI == null || uiController == null || finishMenuLinked)
        {
            if (uiController == null)
            {
                this.uiController = FindAnyObjectByType<UI>();
            }
            if (this.uiController == null || UI == null || finishMenuLinked)
            {
                return;
            }
            uiController = this.uiController;
        }

        Transform finishRoot = UI.transform.Find("Finish");
        Button menuButton = null;

        if (finishRoot != null)
        {
            menuButton = finishRoot.GetComponentInChildren<Button>(true);
        }

        if (menuButton == null)
        {
            Transform menuTransform = UI.transform.Find("Finish/Menu");
            if (menuTransform != null)
            {
                menuButton = menuTransform.GetComponent<Button>();
            }
        }

        if (menuButton == null)
        {
            Debug.LogWarning("Missing Finish/Menu button on " + UI.name + ". Searched path: Finish/Menu");
            return;
        }

        Debug.Log("Found Finish/Menu button on " + UI.name + " at path: Finish/Menu");
        menuButton.onClick.AddListener(uiController.ToMenu);
        finishMenuLinked = true;
    }
}
