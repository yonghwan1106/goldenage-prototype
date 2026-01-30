using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GoldenAge.Dialogue
{
    /// <summary>
    /// 대화 선택지 버튼
    /// </summary>
    public class ChoiceButton : MonoBehaviour
    {
        [SerializeField] private TMP_Text choiceText;
        [SerializeField] private Button button;

        private int choiceIndex;
        private DialogueUI dialogueUI;

        private void Awake()
        {
            if (button == null)
            {
                button = GetComponent<Button>();
            }

            dialogueUI = GetComponentInParent<DialogueUI>();

            button.onClick.AddListener(OnClick);
        }

        /// <summary>
        /// 선택지 설정
        /// </summary>
        public void Setup(int index, string text)
        {
            choiceIndex = index;
            choiceText.text = $"{index + 1}. {text}";
        }

        private void OnClick()
        {
            if (dialogueUI != null)
            {
                dialogueUI.OnChoiceButtonClicked(choiceIndex);
            }
            else
            {
                DialogueManager.Instance?.SelectChoice(choiceIndex);
            }
        }

        private void OnDestroy()
        {
            button.onClick.RemoveListener(OnClick);
        }
    }
}
