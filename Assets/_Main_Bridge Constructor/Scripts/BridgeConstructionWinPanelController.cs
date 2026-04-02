using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Eduzo.Games.BridgeConstruction.UI
{
    public class BridgeConstructionWinPanelController : MonoBehaviour
    {
        [Header("Texts")]
        public TMP_Text ScoreText;
        public TMP_Text ResultText;
        public GameObject TryAaingFlag;
        public GameObject YouWinFlag;

        [Header("Stars")]
        public GameObject[] Stars; // assign 3 stars

        public void ShowResults(float scorePercentage, bool isWin, int starsEarned)
        {
            // Score
            if (ScoreText != null)
                ScoreText.text = Mathf.RoundToInt(scorePercentage) + "%";

            // Ribbin
            if(isWin) {
                TryAaingFlag.SetActive(false);
                YouWinFlag.SetActive(true);
            }
            else
            {
                TryAaingFlag.SetActive(true);
                YouWinFlag.SetActive(false);
            }
                
            // Stars
            for (int i = 0; i < Stars.Length; i++)
            {
                if (Stars[i] != null)
                    Stars[i].SetActive(i < starsEarned);
            }
        }
    }
}