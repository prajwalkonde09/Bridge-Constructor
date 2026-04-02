using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using Eduzo.Games.BridgeConstruction.Data;

namespace Eduzo.Games.BridgeConstruction.UI
{
    public class BridgeConstructionMenuController : MonoBehaviour
    {
        [Header("Panels")]
        public GameObject HomePanel;
        public GameObject FormPanel;

        [Header("Buttons")]
        public Button PracticeButton;
        public Button TestButton;
        public Button SaveButton;

        [Header("Form")]
        public TMP_InputField QuestionInput;
        public TMP_InputField CorrectInput;
        public TMP_InputField Wrong1Input;
        public TMP_InputField Wrong2Input;

        [Header("Question Navigation")]
        public Button PlusButton;
        public Button MinusButton;
        public Button[] QuestionNumberButtons; // size 10

        private int currentEditingIndex = -1;
        private const int MaxQuestions = 10;
        private BridgeConstructionGameManager currentGM;

        private void Start()
        {
            BridgeConstructionDatabaseManager.Load();
            FormPanel.SetActive(false);
            for (int i = 0; i < QuestionNumberButtons.Length; i++)
            {
                int index = i;
                QuestionNumberButtons[i].onClick.AddListener(() => LoadQuestionIntoForm(index));
                QuestionNumberButtons[i].interactable = false;
                QuestionNumberButtons[i].gameObject.SetActive(false);
            }

            RefreshQuestionButtons();
            ValidateForm();
            UpdateButtons();
        }

        public void OpenForm()
        {
            HomePanel.SetActive(false);
            FormPanel.SetActive(true);
        }

        public void OnInputChanged()
        {
            ValidateForm();
        }

        private void LoadQuestionIntoForm(int index)
        {
            var db = BridgeConstructionDatabaseManager.Database;

            if (index < 0 || index >= db.Questions.Count)
                return;

            currentEditingIndex = index;

            var q = db.Questions[index];

            QuestionInput.text = q.Question;
            CorrectInput.text = q.CorrectAnswer;
            Wrong1Input.text = q.WrongAnswer1;
            Wrong2Input.text = q.WrongAnswer2;

            ValidateForm();
        }

        private void ValidateForm()
        {
            bool valid =
                !string.IsNullOrWhiteSpace(QuestionInput.text) &&
                !string.IsNullOrWhiteSpace(CorrectInput.text) &&
                !string.IsNullOrWhiteSpace(Wrong1Input.text) &&
                !string.IsNullOrWhiteSpace(Wrong2Input.text);

            SaveButton.interactable = valid;
        }

        private void ClearForm()
        {
            currentEditingIndex = -1;

            QuestionInput.text = "";
            CorrectInput.text = "";
            Wrong1Input.text = "";
            Wrong2Input.text = "";

            ValidateForm();
        }

        public void SaveQuestion()
        {
            if (!SaveButton.interactable)
                return;

            BridgeConstructionQuestion q;

            if (currentEditingIndex >= 0 &&
                currentEditingIndex < BridgeConstructionDatabaseManager.Database.Questions.Count)
            {
                q = BridgeConstructionDatabaseManager.Database.Questions[currentEditingIndex];
            }
            else
            {
                q = new BridgeConstructionQuestion();
                BridgeConstructionDatabaseManager.Database.Questions.Add(q);
                currentEditingIndex = BridgeConstructionDatabaseManager.Database.Questions.Count - 1;
            }

            q.Question = QuestionInput.text;
            q.CorrectAnswer = CorrectInput.text;
            q.WrongAnswer1 = Wrong1Input.text;
            q.WrongAnswer2 = Wrong2Input.text;

            BridgeConstructionDatabaseManager.Save();

            RefreshQuestionButtons();
            UpdateButtons();
            ClearForm();
        }

        public void OnPlusClicked()
        {
            if (BridgeConstructionDatabaseManager.Database.Questions.Count >= MaxQuestions)
                return;

            ClearForm();
        }

        public void OnMinusClicked()
        {
            if (currentEditingIndex < 0)
                return;

            BridgeConstructionDatabaseManager.Database.Questions.RemoveAt(currentEditingIndex);

            BridgeConstructionDatabaseManager.Save();

            currentEditingIndex = -1;

            RefreshQuestionButtons();

            if (BridgeConstructionDatabaseManager.Database.Questions.Count > 0)
                LoadQuestionIntoForm(0);
            else
                ClearForm();

            UpdateButtons();
        }

        public void ClearAll()
        {
            BridgeConstructionDatabaseManager.Clear();

            currentEditingIndex = -1;

            ClearForm();
            RefreshQuestionButtons();
            UpdateButtons();
        }

        private void UpdateButtons()
        {
            bool hasQuestions = BridgeConstructionDatabaseManager.Database.Questions.Count > 0;

            PracticeButton.interactable = hasQuestions;
            TestButton.interactable = hasQuestions;
        }

        public void OnBack()
        {
            FormPanel.SetActive(false);
            HomePanel.SetActive(true);
        }

        private void RefreshQuestionButtons()
        {
            int count = BridgeConstructionDatabaseManager.Database.Questions.Count;

            for (int i = 0; i < QuestionNumberButtons.Length; i++)
            {
                if (i < count)
                {
                    QuestionNumberButtons[i].gameObject.SetActive(true);
                    QuestionNumberButtons[i].interactable = true;
                }
                else
                {
                    QuestionNumberButtons[i].gameObject.SetActive(false);
                }
            }

            PlusButton.interactable = count < MaxQuestions;
            MinusButton.interactable = count > 0;
        }

        public void StartPractice()
        {
            StartCoroutine(LoadGame(false));
        }

        public void StartTest()
        {
            StartCoroutine(LoadGame(true));
        }

        private IEnumerator LoadGame(bool isTest)
        {
            AsyncOperation load = SceneManager.LoadSceneAsync("Gameplay", LoadSceneMode.Additive);

            while (!load.isDone)
                yield return null;

            currentGM = FindObjectOfType<Eduzo.Games.BridgeConstruction.BridgeConstructionGameManager>();

            currentGM.StartGame(isTest);

            currentGM.HomeButton.onClick.RemoveAllListeners();
            currentGM.RestartButton.onClick.RemoveAllListeners();

            currentGM.HomeButton.onClick.AddListener(() =>
            {
                StartCoroutine(UnloadGameplay());
            });

            currentGM.RestartButton.onClick.AddListener(() =>
            {
                StartCoroutine(ReloadGameplay(isTest));
            });
        }

        private IEnumerator ReloadGameplay(bool isTest)
        {
            yield return SceneManager.UnloadSceneAsync("Gameplay");
            currentGM = null;
            AsyncOperation load = SceneManager.LoadSceneAsync("Gameplay", LoadSceneMode.Additive);

            while (!load.isDone)
                yield return null;

            currentGM = FindObjectOfType<Eduzo.Games.BridgeConstruction.BridgeConstructionGameManager>();
            currentGM.StartGame(isTest);

            currentGM.HomeButton.onClick.RemoveAllListeners();
            currentGM.RestartButton.onClick.RemoveAllListeners();

            currentGM.HomeButton.onClick.AddListener(() =>
            {
                StartCoroutine(UnloadGameplay());
            });

            currentGM.RestartButton.onClick.AddListener(() =>
            {
                StartCoroutine(ReloadGameplay(isTest));
            });
        }

        private IEnumerator UnloadGameplay()
        {
            yield return SceneManager.UnloadSceneAsync("Gameplay");
            currentGM = null;
        }
    }
}