using BackendControl;
using CustomEnumControl;
using ManagerControl;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIControl
{
    public class UserNameController : MonoBehaviour
    {
        private string _curNickName;
        private bool _isOpenUserName;
        private Button _userButton;
        private TMP_Text _userNameText;
        [SerializeField] private Button userNameButton;
        [SerializeField] private TMP_InputField nameInputField;

        private void Awake()
        {
            _userNameText = userNameButton.GetComponentInChildren<TMP_Text>();
            nameInputField.gameObject.SetActive(false);

            _userButton = GetComponent<Button>();
            _userButton.onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                OnOffUserName();
            });
            _userNameText.text = BackendLogin.instance.GetUserNickName();
            userNameButton.onClick.AddListener(() =>
            {
                nameInputField.gameObject.SetActive(true);
            });
            nameInputField.onSelect.AddListener(delegate
            {
                Debug.Log("onselect");
                GetCurNickName();
            });
            nameInputField.onSubmit.AddListener(delegate
            {
                Debug.Log("onsubmit");
                CompleteNickName();
            });
            nameInputField.onDeselect.AddListener(delegate
            {
                Debug.Log("ondeselect");
                _userNameText.text = _curNickName;
            });
        }

        private void GetCurNickName()
        {
            Debug.Log("select");
            _curNickName = _userNameText.text;
        }

        private void CompleteNickName()
        {
            if (nameInputField.text.Length > 0)
            {
                BackendLogin.instance.UpdateNickname(nameInputField.text);
            }
            else
            {
                _userNameText.text = _curNickName;
            }
        }

        private void OnOffUserName()
        {
            if (_isOpenUserName)
            {
                _isOpenUserName = false;
                userNameButton.gameObject.SetActive(false);
            }
            else
            {
                _isOpenUserName = true;
                userNameButton.gameObject.SetActive(true);
            }
        }
    }
}