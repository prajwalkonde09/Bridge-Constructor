using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Eduzo.Games.BridgeConstruction.Data;
using UnityEngine.UI;
using Eduzo.Games.BridgeConstruction.UI;

namespace Eduzo.Games.BridgeConstruction
{
    public class BridgeConstructionGameManager : MonoBehaviour
    {
        [Header("UI")]
        public TMP_Text QuestionText;
        public Button HomeButton;
        public Button RestartButton;

        [Header("Tiles")]
        public BridgeConstructionOptionTile[] OptionTiles;

        [Header("Tile Reset Effect")]
        public ParticleSystem TileResetEffect1;
        public ParticleSystem TileResetEffect2;
        public ParticleSystem TileResetEffect3;        

        [Header("Bridge Variants")]
        public GameObject GapObject;
        public GameObject FlatBridge;
        public GameObject StoneArchBridge;
        public GameObject WoodenBridge;
        public GameObject OptionsParent;

        [Header("Bridge Fall Settings")]
        public Transform DisolvePoint;
        public float FallSpeed = 2f;
        public float FadeSpeed = 2f;

        [Header("Arc Points")]
        public Transform ArcStartPoint;
        public Transform ArcPeakPoint;
        public Transform ArcEndPoint;
        public float tweakspeed = 1.2f; // tweak this for faster/slower arc movement

        [Header("Cars")]
        public Transform HoldPoint;
        public Transform EndPoint;
        public float CarSpeed = 200f;
        private int carsFinished;

        [Header("Individual Cars")]
        public Transform[] Cars; // assign 3 cars manually

        private Vector3[] carStartPositions;
        private Quaternion[] carStartRotations;
        
        [Header("Path Rotations")]
        public float[] PathZRotations = new float[5] { 0f, 20f, 0f, -20f, 0f };

        [Header("Panels")]
        public GameObject WinPanel;

        [Header("Overlay")]
        public CanvasGroup BlurScreen; // assign blur panel with CanvasGroup
        public TextMeshProUGUI AnswerResultText;

        [Header("Path Movement")]
        public List<Transform> PathPoints;

        [Header("Test Mode UI")]
        public GameObject LivesParent;
        
        [Header("Lives (Hearts)")]
        public Image[] LifeImages; // assign 3 hearts in order

        [Header("Win Panel")]
        public BridgeConstructionWinPanelController WinPanelController;

        public GameObject TimerObject;
        public TMP_Text TimerText;

        [Header("Score")]
        public TMP_Text ScoreText;

        private int currentIndex;
        private BridgeConstructionQuestion currentQuestion;

        private bool isTestMode;
        private int lives = 3;
        private float timer = 60f;

        private int correct;
        private int wrong;
        private bool isGameOver;

        private bool canHonk = true;
        private bool isAnswered;

        public void StartGame(bool testMode)
        {
            BridgeConstructionDatabaseManager.Load();

            isTestMode = testMode;
            currentIndex = 0;
            correct = 0;
            wrong = 0;
            AnswerResultText.text = "";
            isGameOver = false;

            lives = 3;
            timer = 60f;

            if (isTestMode)
            {
                LivesParent.SetActive(true);
                TimerObject.SetActive(true);

                UpdateLivesUI();
                TimerText.text = Mathf.Ceil(timer).ToString();
            }
            else
            {
                LivesParent.SetActive(false);
                TimerObject.SetActive(false);
            }

            carStartPositions = new Vector3[Cars.Length];
            carStartRotations = new Quaternion[Cars.Length];

            for (int i = 0; i < Cars.Length; i++)
            {
                carStartPositions[i] = Cars[i].localPosition;
                carStartRotations[i] = Cars[i].localRotation;
            }

            LoadQuestion();
        }

        private void Update()
        {
            if (!isTestMode || isGameOver)
                return;

            timer -= Time.deltaTime;
            TimerText.text = Mathf.Ceil(timer).ToString();

            if (timer <= 0)
            {
                EndGame();
            }
        }

        private void UpdateLivesUI()
        {
            for (int i = 0; i < LifeImages.Length; i++)
            {
                if (LifeImages[i] != null)
                    LifeImages[i].gameObject.SetActive(i < lives);
            }
        }

        private void LoadQuestion()
        {
            StopAllCoroutines();
            for(int i = 0; i < OptionTiles.Length; i++)
            {
                OptionTiles[i].gameObject.SetActive(true);
            }

            ResetBridgeVisuals();

            if (currentIndex >= BridgeConstructionDatabaseManager.Database.Questions.Count)
            {
                EndGame();
                return;
            }

            currentQuestion = BridgeConstructionDatabaseManager.Database.Questions[currentIndex];

            QuestionText.text = currentQuestion.Question;

            SetupOptions();

            FlatBridge.SetActive(false);
            StoneArchBridge.SetActive(false);
            WoodenBridge.SetActive(false);

            GapObject.SetActive(true);

            isAnswered = false;

            AnswerResultText.text = currentQuestion.CorrectAnswer;

            StartCoroutine(HonkRoutine());
        }

        private void SetupOptions()
        {
            List<string> texts = new List<string>
            {
                currentQuestion.CorrectAnswer,
                currentQuestion.WrongAnswer1,
                currentQuestion.WrongAnswer2
            };

            // Shuffle ONLY text
            for (int i = 0; i < texts.Count; i++)
            {
                int rand = Random.Range(0, texts.Count);
                (texts[i], texts[rand]) = (texts[rand], texts[i]);
            }

            for (int i = 0; i < OptionTiles.Length; i++)
            {
                OptionTiles[i].Setup(texts[i]);
                // DO NOT TOUCH OptionId
            }
        }

        public void OnTileDropped(BridgeConstructionOptionTile tile)
        {
            if (isAnswered)
                return;

            if (isTestMode)
            {
                isAnswered = true; // prevent multiple clicks while processing
            }

            if (tile.Value == currentQuestion.CorrectAnswer)
            {
                GameAudioManager.Instance.PlayCorrect();

                GapObject.SetActive(false);
                ActivateBridge(tile.OptionId);

                isAnswered = true;
                correct++;

                StartCoroutine(CorrectFlow(tile));
            }
            else
            {
                StartCoroutine(WrongAnswerRoutine(tile));
                if (isTestMode)
                {
                    lives--;
                    UpdateLivesUI();

                    if (lives <= 0)
                    {
                        EndGame();
                    }
                    StartCoroutine(WrongThenNext(tile));
                }
                wrong++;
            }
        }

        private IEnumerator WrongThenNext(BridgeConstructionOptionTile tile)
        {
            GapObject.SetActive(false);
            ResetAllTiles();
            ActivateBridge(tile.OptionId);

            GameAudioManager.Instance.Honk();

            GameObject selectedBridge = GetBridgeById(tile.OptionId);

            yield return new WaitForSeconds(1f);

            yield return StartCoroutine(BridgeFallRoutine(selectedBridge));

            yield return new WaitForSeconds(0.3f);

            currentIndex++;

            if (currentIndex >= BridgeConstructionDatabaseManager.Database.Questions.Count)
            {
                EndGame();
                yield break;
            }

            LoadQuestion();
        }

        private void ResetAllTiles()
        {
            foreach (var tile in OptionTiles)
            {
                tile.ResetPosition();
            }
        }

        private IEnumerator CorrectFlow(BridgeConstructionOptionTile tile)
        {
            tile.gameObject.SetActive(false);
            ResetAllTiles();
            
            if (tile.OptionId == 2)
                yield return StartCoroutine(CarFollowPath());
            else
                yield return StartCoroutine(CarMoveStraight());

            yield return StartCoroutine(BlurRoutine());

            currentIndex++;

            if (currentIndex >= BridgeConstructionDatabaseManager.Database.Questions.Count)
            {
                EndGame();
                yield break;
            }

            LoadQuestion();
        }

        private IEnumerator WrongAnswerRoutine(BridgeConstructionOptionTile tile)
        {
            // Show selected bridge
            GapObject.SetActive(false);
            ResetAllTiles();
            ActivateBridge(tile.OptionId);
            if(tile.OptionId == 1)
            {
                TileResetEffect1.Play();
            }
            else if(tile.OptionId == 2)
            {
                TileResetEffect2.Play();
            }
            else if(tile.OptionId == 3)
            {
                TileResetEffect3.Play();
            }

            // Honk once
            GameAudioManager.Instance.Honk();
            GameObject selectedBridge = GetBridgeById(tile.OptionId);

            yield return new WaitForSeconds(1f);

            yield return StartCoroutine(BridgeFallRoutine(selectedBridge));

            // Hide all bridges
            FlatBridge.SetActive(false);
            StoneArchBridge.SetActive(false);
            WoodenBridge.SetActive(false);

            // Show gap again
            GapObject.SetActive(true);

            // Reset tiles AFTER visual feedback
            ResetAllTiles();
        }

        private void ResetBridgeVisuals()
        {
            GameObject[] bridges = { FlatBridge, StoneArchBridge, WoodenBridge };

            foreach (var bridge in bridges)
            {
                if (bridge == null) continue;

                CanvasGroup cg = bridge.GetComponent<CanvasGroup>();
                if (cg != null)
                {
                    cg.alpha = 1f;
                }
            }
        }

        private void ActivateBridge(int id)
        {
            FlatBridge.SetActive(false);
            StoneArchBridge.SetActive(false);
            WoodenBridge.SetActive(false);

            if (id == 1)
                FlatBridge.SetActive(true);
            else if (id == 2)
                StoneArchBridge.SetActive(true);
            else if (id == 3)
                WoodenBridge.SetActive(true);
        }

        private IEnumerator CarFollowPath()
        {
            carsFinished = 0;

            for (int i = 0; i < Cars.Length; i++)
            {
                StartCoroutine(FollowPathSingleCar(Cars[i], i * 0.2f));
            }

            // WAIT until all cars finished
            while (carsFinished < Cars.Length)
                yield return null;
        }

        private IEnumerator CarMoveStraight()
        {
            carsFinished = 0;

            for (int i = 0; i < Cars.Length; i++)
            {
                StartCoroutine(MoveStraightSingleCar(Cars[i], i * 0.2f));
            }

            while (carsFinished < Cars.Length)
                yield return null;
        }

        private IEnumerator MoveStraightSingleCar(Transform car, float delay)
        {
            yield return new WaitForSeconds(delay);

            Quaternion targetRot = Quaternion.Euler(0, 0, 0);

            while (Vector3.Distance(car.position, EndPoint.position) > 0.05f)
            {
                car.position = Vector3.MoveTowards(
                    car.position,
                    EndPoint.position,
                    CarSpeed * Time.deltaTime
                );

                car.rotation = Quaternion.Lerp(
                    car.rotation,
                    targetRot,
                    10f * Time.deltaTime
                );

                yield return null;
            }
            car.position = EndPoint.position;
            carsFinished++;
        }

        private IEnumerator FollowPathSingleCar(Transform car, float delay)
        {
            yield return new WaitForSeconds(delay);

            Vector3 start = ArcStartPoint.position;
            Vector3 peak = ArcPeakPoint.position;
            Vector3 end = ArcEndPoint.position;

            // STEP 1: MOVE TO ARC START
            while (Vector3.Distance(car.position, start) > 0.1f)
            {
                car.position = Vector3.MoveTowards(
                    car.position,
                    start,
                    CarSpeed * Time.deltaTime
                );
                yield return null;
            }

            float t = 0f;
            Vector3 prevPos = car.position;

            while (t < 1f)
            {
                // Bezier position
                Vector3 targetPos =
                    Mathf.Pow(1 - t, 2) * start +
                    2 * (1 - t) * t * peak +
                    Mathf.Pow(t, 2) * end;

                // MOVE WITH CONSTANT SPEED
                car.position = Vector3.MoveTowards(
                    car.position,
                    targetPos,
                    CarSpeed * Time.deltaTime
                );

                // ROTATION
                Vector3 moveDir = (car.position - prevPos);

                if (moveDir.sqrMagnitude > 0.0001f)
                {
                    float angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;

                    // LIMIT EXTREME ROTATION (important for UI)
                    angle = Mathf.Clamp(angle, -25f, 25f);

                    Quaternion targetRot = Quaternion.Euler(0, 0, angle);

                    car.rotation = Quaternion.Lerp(
                        car.rotation,
                        targetRot,
                        8f * Time.deltaTime
                    );
                }

                // store for next frame
                prevPos = car.position;

                // ✅ PROGRESS ONLY WHEN REACHED CURRENT TARGET
                if (Vector3.Distance(car.position, targetPos) < 0.2f)
                {
                    t += 0.05f; // 🔥 small step = smooth + accurate
                }

                yield return null;
            }

            // FINAL SMOOTH RESET TO FLAT
            // while (Mathf.Abs(car.eulerAngles.z) > 0.5f)
            // {
            //     car.rotation = Quaternion.Lerp(
            //         car.rotation,
            //         Quaternion.identity,
            //         10f * Time.deltaTime
            //     );
            //     yield return null;
            // }

            // STEP 3: MOVE TO FINAL END
            // while (Vector3.Distance(car.position, EndPoint.position) > 0.1f)
            // {
            //     car.position = Vector3.MoveTowards(
            //         car.position,
            //         EndPoint.position,
            //         CarSpeed * Time.deltaTime
            //     );
            //     yield return null;
            // }

            // SMOOTH RESET WHILE MOVING TO END
            Quaternion targetReset = carStartRotations[System.Array.IndexOf(Cars, car)];

            while (Vector3.Distance(car.position, EndPoint.position) > 0.1f)
            {
                car.position = Vector3.MoveTowards(
                    car.position,
                    EndPoint.position,
                    CarSpeed * Time.deltaTime
                );

                car.rotation = Quaternion.Lerp(
                    car.rotation,
                    targetReset,
                    8f * Time.deltaTime
                );

                yield return null;
            }

            car.rotation = targetReset;

            carsFinished++;
        }

        private IEnumerator BridgeFallRoutine(GameObject bridge)
        {
            if (bridge == null) yield break;

            CanvasGroup cg = bridge.GetComponent<CanvasGroup>();

            Vector3 startPos = bridge.transform.position;

            float t = 0f;

            while (bridge.transform.position.y > DisolvePoint.position.y)
            {
                // MOVE DOWN
                bridge.transform.position = Vector3.MoveTowards(
                    bridge.transform.position,
                    new Vector3(startPos.x, DisolvePoint.position.y, startPos.z),
                    FallSpeed * Time.deltaTime
                );

                // FADE OUT
                if (cg != null)
                {
                    cg.alpha -= FadeSpeed * Time.deltaTime;
                }

                yield return null;
            }

            bridge.SetActive(false);

            // RESET for next use
            bridge.transform.position = startPos;

            if (cg != null)
            {
                cg.alpha = 1f;
            }
        }

        private IEnumerator BlurRoutine()
        {
            // SHOW BLUR
            BlurScreen.gameObject.SetActive(true);
            BlurScreen.alpha = 1f;
            OptionsParent.SetActive(false);

            for (int i = 0; i < Cars.Length; i++)
            {
                Cars[i].localPosition = carStartPositions[i];
                Cars[i].localRotation = carStartRotations[i];
            }

            ResetAllTiles();

            FlatBridge.SetActive(false);
            StoneArchBridge.SetActive(false);
            WoodenBridge.SetActive(false);

            GapObject.SetActive(true);

            yield return new WaitForSeconds(2f);

            // FADE OUT
            float t = 0f;
            float duration = 0.5f;

            while (t < duration)
            {
                t += Time.deltaTime;
                BlurScreen.alpha = 1f - (t / duration);
                yield return null;
            }
            OptionsParent.SetActive(true);
            // RESET BLUR
            BlurScreen.alpha = 1f;
            BlurScreen.gameObject.SetActive(false);
        }

        private IEnumerator HonkRoutine()
        {
            while (!isAnswered)
            {
                if (canHonk)
                {
                    // HonkAudio.Play();
                    GameAudioManager.Instance.Honk();
                    canHonk = false;
                    yield return new WaitForSeconds(15f);
                    canHonk = true;
                }
                yield return null;
            }
        }

        private GameObject GetBridgeById(int id)
        {
            if (id == 1) return FlatBridge;
            if (id == 2) return StoneArchBridge;
            if (id == 3) return WoodenBridge;
            return null;
        }

        private void EndGame()
        {
            if (isGameOver) return; 
                isGameOver = true;
            int total = correct + wrong;
            float scorePercentage = total == 0 ? 0 : ((float)correct / total) * 100f;

            bool isWin;

            if (isTestMode)
            {
                isWin = timer > 0 && lives > 0;
            }
            else
            {
                isWin = currentIndex >= BridgeConstructionDatabaseManager.Database.Questions.Count;
            }

            int starsEarned = 0;

            if (scorePercentage > 66)
                starsEarned = 3;
            else if (scorePercentage > 33)
                starsEarned = 2;
            else if (scorePercentage > 0)
                starsEarned = 1;

            WinPanel.SetActive(true);

            if (ScoreText != null)
                ScoreText.text = Mathf.RoundToInt(scorePercentage) + "%";

            if (WinPanelController != null)
            {
                WinPanelController.ShowResults(scorePercentage, isWin, starsEarned);
            }
        }
    }
}