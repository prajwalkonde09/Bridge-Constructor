using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

namespace Eduzo.Games.BridgeConstruction
{
    public class BridgeConstructionOptionTile : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public int OptionId; // 1 = Flat, 2 = Arch, 3 = Wooden
        public string Value;

        private Vector2 originalPos;
        private bool isInitialized;

        private RectTransform rect;
        private Vector2 startPos;
        private Canvas canvas;
        public TextMeshProUGUI Text;
        private CanvasGroup canvasGroup;

        private void Awake()
        {
            rect = GetComponent<RectTransform>();
            canvas = GetComponentInParent<Canvas>();
            canvasGroup = GetComponent<CanvasGroup>();
        }

        public void Setup(string value)
        {
            Value = value;
            Text.text = value;
            gameObject.SetActive(true);

            if (!isInitialized)
            {
                originalPos = rect.anchoredPosition;
                isInitialized = true;
            }

            rect.anchoredPosition = originalPos;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (canvasGroup != null)
                canvasGroup.blocksRaycasts = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            rect.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (canvasGroup != null)
                canvasGroup.blocksRaycasts = true;

            Debug.Log("Dropped on: " + eventData.pointerEnter);

            var slot = eventData.pointerEnter?.GetComponentInParent<BridgeConstructionDropSlot>();

            if (slot != null)
            {
                Debug.Log("Slot detected");
                slot.OnDropTile(this);
            }
            else
            {
                Debug.Log("No slot");
                ResetPosition();
            }
        }

        public void ResetPosition()
        {
            rect.anchoredPosition = originalPos;
        }
    }
}