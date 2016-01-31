using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace druggedcode
{
    /// <summary>
    /// DialogueZone 에 의해 생성되는 대화상자. 직접적으로 게임에 추가하지 말 것.
    /// GameObject dialogueObject = (GameObject)Instantiate(Resources.Load("GUI/DialogueBox"));
    // 별도의 카메라 UI를 가지고 있는 프리팹에 달려 있다.
    // 만약 대화존이 캐릭터에 종속되어있을 경우 캐릭터가 움직이면 대화박스는 같이 움직이지 않는다. HP바와 같이 (주인객체) 를 지속적으로 따라다닐 수 있게 수정하는 것을 고려
    /// </summary>
    public class DialogueBox : MonoBehaviour
    {
        /// 백그라운드
        public Image TextPanel;
        /// 화살표
        public Image TextPanelArrowDown;
        /// 표시될 텍스트
        public Text DialogueText;
        /// 버튼 프롬프트
        public GameObject ButtonA;

        private Color _backgroundColor;
        private Color _textColor;
        private SpriteRenderer _buttonSpriteRenderer;

        //텍스트를 변경
        public void ChangeText(string newText)
        {
            DialogueText.text = newText;
        }

        public void ButtonActive(bool state)
        {
            ButtonA.SetActive(state);
        }

        // 배경과 텍스트의 색을 저장하고 배경과 텍스트를 투명화 시켜둔다.
        public void ChangeColor(Color backgroundColor, Color textColor)
        {
            _backgroundColor = backgroundColor;
            _textColor = textColor;

            Color newBackgroundColor = new Color(_backgroundColor.r, _backgroundColor.g, _backgroundColor.b, 0);
            Color newTextColor = new Color(_textColor.r, _textColor.g, _textColor.b, 0);

            TextPanel.color = newBackgroundColor;
            TextPanelArrowDown.color = newBackgroundColor;
            DialogueText.color = newTextColor;

            _buttonSpriteRenderer = ButtonA.GetComponent<SpriteRenderer>();
            _buttonSpriteRenderer.material.color = new Color(1f, 1f, 1f, 0f);
        }

        public void FadeIn(float duration)
        {
            StartCoroutine(MotionUI.ChangeColor(TextPanel, duration, _backgroundColor));
            StartCoroutine(MotionUI.ChangeColor(TextPanelArrowDown, duration, _backgroundColor));
            StartCoroutine(MotionUI.FadeAlpha(DialogueText, duration, 1f));
            StartCoroutine(Motion2D.FadeAlpha(_buttonSpriteRenderer, duration, 1f ));
        }

        public void FadeOut(float duration)
        {
            Color newBackgroundColor = new Color(_backgroundColor.r, _backgroundColor.g, _backgroundColor.b, 0);
            Color newTextColor = new Color(_textColor.r, _textColor.g, _textColor.b, 0);

            StartCoroutine(MotionUI.ChangeColor(TextPanel, duration, newBackgroundColor));
            StartCoroutine(MotionUI.ChangeColor(TextPanelArrowDown, duration, newBackgroundColor));
            StartCoroutine(MotionUI.FadeAlpha(DialogueText, duration, 0f ));
            StartCoroutine(Motion2D.FadeAlpha(_buttonSpriteRenderer, duration, 0f ));
        }

        // 화살표를 가린다.
        public void HideArrow()
        {
            TextPanelArrowDown.enabled = false;
        }
    }
}
