using System;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using AppleAuth;
using AppleAuth.Enums;
using AppleAuth.Extensions;
using AppleAuth.Interfaces;
using AppleAuth.Native;
using BackEnd;
using BackendControl;
using CustomEnumControl;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Newtonsoft.Json.Linq;
using TMPro;
using UIControl;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Utilities;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace ManagerControl
{
    public class LoginManager : MonoBehaviour
    {
        private const string UserEmailID = "User Email ID";
        private const string LoginPlatformKey = "LoginPlatform";

        private string _id, _oneTimeCode, _password;
        private string _wwwText;

        private Tween _loginButtonGroupTween;
        private Tween _connectionPanelGroupTween;
        private Tween _federationGroupTween;

        private CancellationTokenSource _cts;

        private LoginPlatform _curSelectPlatform;

#if UNITY_IPHONE
        private IAppleAuthManager _appleAuthManager;
#endif
        [SerializeField] private Button appleLoginButton;
        [SerializeField] private Button emailLoginButton;
        [SerializeField] private Button googleLoginButton;

        [SerializeField] private CanvasGroup loginButtonsGroup;
        [SerializeField] private Image checkLastLoginImage;
        [SerializeField] private GameObject emailLoginPanelGroupObj;
        [SerializeField] private RectTransform content;

        [Header("=======================Email Login=======================")]
        [SerializeField] private Button goBackButton;

        [SerializeField] private CustomTMPInputField idField;
        [SerializeField] private CustomTMPInputField oneTimeCodeField;
        [SerializeField] private Button sendEmailButton;
        [SerializeField] private TMP_Text sendEmailBtnText;
        [SerializeField] private Image timerBackground;
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private GameObject notifySendEmailObj;
        [SerializeField] private Button loginButton;
        [SerializeField] private string myEmail;
        [SerializeField] private string myPassword;
        [SerializeField] private string url;
        [SerializeField] private Image loadingImage;
        [SerializeField] private Transform loadingIcon;

        [SerializeField] private CanvasGroup federationPanelGroup;
        [SerializeField] private Button goBackFederationButton;
        [SerializeField] private Button federationLoginButton;
        [SerializeField] private Image federationImage;
        [SerializeField] private Sprite appleSprite;
        [SerializeField] private Sprite googleSprite;

        [SerializeField] private CanvasGroup connectionPanelGroup;
        [SerializeField] private NoticePanel signUpConfirmPanel;
        [SerializeField] private NoticePanel logOutNoticePanel;

        private void Start()
        {
            LoadLoginPlatform();
            Init();
            InitTween();
            InitInputField();
            InitButtons();
#if UNITY_IPHONE
            AppleInit();
#endif
        }

        private void Update()
        {
#if UNITY_IPHONE
            _appleAuthManager?.Update();
#endif
        }

        private void OnDisable()
        {
            _loginButtonGroupTween?.Kill();
            _connectionPanelGroupTween?.Kill();
        }

#region Init

        private void Init()
        {
            emailLoginPanelGroupObj.SetActive(false);
            notifySendEmailObj.SetActive(false);
            timerBackground.enabled = false;
            timerText.text = "";
            oneTimeCodeField.interactable = false;
            loginButton.interactable = false;

            var lastPlatform = PlayerPrefs.GetInt(LoginPlatformKey);
            checkLastLoginImage.enabled = lastPlatform != 0;

            loadingImage.enabled = false;
            loadingIcon.localScale = Vector3.zero;

            Input.multiTouchEnabled = false;
            Time.timeScale = 1;
            Application.targetFrameRate = 60;
        }

        private void InitTween()
        {
            _loginButtonGroupTween = loginButtonsGroup.DOFade(1, 0.25f).From(0).SetAutoKill(false)
                .Pause();
            _loginButtonGroupTween.OnComplete(() => loginButtonsGroup.blocksRaycasts = true);
            _loginButtonGroupTween.Restart();

            _connectionPanelGroupTween = connectionPanelGroup.DOFade(1, 0.25f).From(0).SetAutoKill(false)
                .Pause();
            connectionPanelGroup.blocksRaycasts = false;

            _federationGroupTween = federationPanelGroup.DOFade(1, 0.25f).From(0).SetAutoKill(false).Pause();
            federationPanelGroup.blocksRaycasts = false;
            _federationGroupTween.OnComplete(() => federationPanelGroup.blocksRaycasts = true);
        }

        private void InitInputField()
        {
            if (idField.text.Contains("@") && idField.text.Contains("."))
            {
                sendEmailButton.interactable = true;
                sendEmailBtnText.color = Color.red;
            }

            idField.OnPointerUpEvent += () =>
            {
                idField.ActivateInputField();
                TouchScreenKeyboard.hideInput = true;
                TouchScreenKeyboard.Open(idField.text, TouchScreenKeyboardType.Default, false, false, false);

                content.DOLocalMoveY(350, 0.5f).SetEase(Ease.OutQuart);
            };
            oneTimeCodeField.OnPointerUpEvent += () =>
            {
                oneTimeCodeField.ActivateInputField();
                TouchScreenKeyboard.hideInput = true;
                TouchScreenKeyboard.Open(oneTimeCodeField.text, TouchScreenKeyboardType.Default, false, false, false);

                content.DOLocalMoveY(350, 0.5f).SetEase(Ease.OutQuart);
            };

            idField.onSubmit.AddListener(_ => content.DOLocalMoveY(0, 0.5f).SetEase(Ease.OutQuart));
            idField.onValueChanged.AddListener(_ => CheckEmailForm());
            // idField.onDeselect.AddListener(_ => content.DOLocalMoveY(0, 0.5f).SetEase(Ease.OutQuart));

            oneTimeCodeField.onSubmit.AddListener(_ => content.DOLocalMoveY(0, 0.5f).SetEase(Ease.OutQuart));
            // oneTimeCodeField.onDeselect.AddListener(_ => content.DOLocalMoveY(0, 0.5f).SetEase(Ease.OutQuart));
        }

        private void InitButtons()
        {
            appleLoginButton.onClick.AddListener(() =>
            {
                loginButtonsGroup.blocksRaycasts = false;
                _loginButtonGroupTween.PlayBackwards();
                _curSelectPlatform = LoginPlatform.Apple;
                federationImage.sprite = appleSprite;
                federationImage.color = Color.black;
                _federationGroupTween.Restart();
            });

            emailLoginButton.onClick.AddListener(() =>
            {
                content.anchoredPosition = Vector2.zero;
                idField.text = PlayerPrefs.GetString(UserEmailID);

                emailLoginPanelGroupObj.SetActive(true);
                loginButtonsGroup.blocksRaycasts = false;
                _loginButtonGroupTween.PlayBackwards();
            });
            googleLoginButton.onClick.AddListener(() =>
            {
                loginButtonsGroup.blocksRaycasts = false;
                _loginButtonGroupTween.PlayBackwards();
                _curSelectPlatform = LoginPlatform.Google;
                federationImage.sprite = googleSprite;
                federationImage.color = Color.white;
                _federationGroupTween.Restart();
            });

            goBackFederationButton.onClick.AddListener(() =>
            {
                _loginButtonGroupTween.Restart();
                federationPanelGroup.blocksRaycasts = false;
                _federationGroupTween.PlayBackwards();
            });

            federationLoginButton.onClick.AddListener(() =>
            {
                switch (_curSelectPlatform)
                {
                    case LoginPlatform.Apple:
                        AppleLogin();
                        break;
                    case LoginPlatform.Google:
                        StartGoogleLogin();
                        break;
                }
            });
            goBackButton.onClick.AddListener(() =>
            {
                CancelTimer();
                emailLoginPanelGroupObj.SetActive(false);
                _loginButtonGroupTween.Restart();
            });

            sendEmailButton.onClick.AddListener(() => SendEmail().Forget());
            loginButton.onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                Login().Forget();
            });

            logOutNoticePanel.OnConfirmButtonEvent += () =>
            {
                BackendLogin.instance.LogOut();

                BackendChart.instance.InitItemTable();
                _connectionPanelGroupTween.OnRewind(() => connectionPanelGroup.blocksRaycasts = false)
                    .PlayBackwards();
                _loginButtonGroupTween.Restart();
            };
            signUpConfirmPanel.OnConfirmButtonEvent += async () =>
            {
                loginButton.interactable = false;
                CancelTimer();
                await StartEmailSignUp();
                StartEmailLogin();
            };
        }

#endregion

#region EMAIL

        private async UniTaskVoid SendEmail()
        {
            oneTimeCodeField.interactable = true;
            loginButton.interactable = true;
            notifySendEmailObj.SetActive(true);
            if (idField.text == "test@test.com")
            {
                BackendLogin.instance.testLogin = true;
                _oneTimeCode = "123456";
                return;
            }

            PlayerPrefs.SetString(UserEmailID, idField.text);

            if (idField.text.Contains("@") && idField.text.Contains("."))
            {
                SetTimer().Forget();

                _id = idField.text.Trim();
                _oneTimeCode = GenerateRandomCode();

                var form = new WWWForm();
                form.AddField("order", "checkEmail");
                form.AddField("id", _id);
                await Post(form);

                var mail = new MailMessage();
                mail.From = new MailAddress(myEmail);
                mail.To.Add(idField.text);
                mail.Subject = "CODE : " + _oneTimeCode;
                mail.Body = "Code : " + _oneTimeCode;
                var smtpServer = new SmtpClient("smtp.gmail.com");
                smtpServer.Port = 587;
                smtpServer.Credentials = new NetworkCredential(myEmail, myPassword);
                smtpServer.EnableSsl = true;
                ServicePointManager.ServerCertificateValidationCallback = (_, _, _, _) => true;

                await UniTask.RunOnThreadPool(() => { smtpServer.Send(mail); });
            }
        }

        private async UniTaskVoid SetTimer()
        {
            idField.interactable = false;
            timerBackground.enabled = true;
            sendEmailButton.interactable = false;

            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            var timer = 60;
            while (timer > 0)
            {
                timer -= 1;
                timerText.text = timer + "s";
                await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: _cts.Token);
            }

            CancelTimer();
        }

        private void CancelTimer()
        {
            _cts?.Cancel();
            idField.interactable = true;
            oneTimeCodeField.interactable = false;
            timerBackground.enabled = false;
            sendEmailButton.interactable = true;
            notifySendEmailObj.SetActive(false);
            timerText.text = "";
        }

        private async UniTaskVoid Login()
        {
            if (BackendLogin.instance.testLogin && oneTimeCodeField.text == "123456")
            {
                BackendLogin.instance.CustomLogin("test@test.com", "123456");
                ActiveStartPanel();
                return;
            }

            if (oneTimeCodeField.text == _oneTimeCode)
            {
                loadingImage.enabled = true;
                loadingIcon.DOScale(1, 0.25f).From(0).SetEase(Ease.OutBack).SetUpdate(true);
                loadingIcon.DOLocalRotate(new Vector3(0, 0, -360), 1, RotateMode.FastBeyond360).SetLoops(10);

                var jObject = JObject.Parse(_wwwText);

                if (jObject["result"]?.ToString() == "SignUp")
                {
                    signUpConfirmPanel.OpenPopUp();
                    return;
                }

                if (jObject["result"]?.ToString() == "LogIn")
                {
                    loginButton.interactable = false;
                    _cts?.Cancel();
                    var form = new WWWForm();
                    form.AddField("order", "login");
                    form.AddField("row", jObject["value"]?.ToString());
                    await Post(form);
                    _password = jObject["msg"]?.ToString();

                    StartEmailLogin();
                }
            }
        }

        private async UniTask StartEmailSignUp()
        {
            var form = new WWWForm();
            form.AddField("order", "signup");
            form.AddField("id", _id);
            _password = GenerateRandomPassword();
            form.AddField("password", _password);
            await Post(form);
            BackendLogin.instance.CustomSignUp(_id, _password);
        }

        private void StartEmailLogin()
        {
            var backendLogin = BackendLogin.instance;
            backendLogin.CustomLogin(_id, _password);
            backendLogin.loginPlatform = LoginPlatform.Custom;
            backendLogin.customEmail = _id;
            backendLogin.url = url;

            if (!backendLogin.testLogin)
            {
                var countJson = JObject.Parse(_wwwText);
                var curCount = byte.Parse((string)countJson["value"] ?? string.Empty);
                if (curCount >= 7)
                {
                    var oldPassword = _password;
                    _password = GenerateRandomPassword();

                    UniTask.RunOnThreadPool(() => { Backend.BMember.UpdatePassword(oldPassword, _password); });
                    var form = new WWWForm();
                    form.AddField("order", "updatePassword");
                    form.AddField("id", _id);
                    form.AddField("password", _password);
                    Post(form).Forget();
                }
            }

            ActiveStartPanel();
        }

        private void ActiveStartPanel()
        {
            loadingImage.enabled = false;
            loadingIcon.DOScale(0, 0.25f).From(1).SetEase(Ease.OutBack);
            _connectionPanelGroupTween.OnComplete(() => connectionPanelGroup.blocksRaycasts = true).Restart();
            BackendManager.BackendInit().Forget();
            SaveLoginPlatform(LoginPlatform.Custom);

            CancelTimer();

            _oneTimeCode = "";
            oneTimeCodeField.text = "";
            content.DOLocalMoveY(0, 0.5f).SetEase(Ease.OutQuart);
            emailLoginPanelGroupObj.SetActive(false);
            federationPanelGroup.blocksRaycasts = false;
            _federationGroupTween.PlayBackwards();
        }

        private async UniTask Post(WWWForm form)
        {
            using var www = UnityWebRequest.Post(url, form);
            await www.SendWebRequest();
            if (www.isDone)
            {
                _wwwText = www.downloadHandler.text;
            }
            else
            {
                Debug.Log("웹 응답이 없습니다.");
            }
        }

        private void CheckEmailForm()
        {
            if (idField.text.Contains("@") && idField.text.Contains("."))
            {
                sendEmailButton.interactable = true;
                sendEmailBtnText.color = Color.red;
            }
            else
            {
                sendEmailButton.interactable = false;
                sendEmailBtnText.color = Color.gray;
            }
        }

        private string GenerateRandomCode()
        {
            const string code = "0123456789";
            var characters = code.ToCharArray();

            for (var i = characters.Length - 1; i > 0; i--)
            {
                var j = Random.Range(0, i + 1);
                (characters[i], characters[j]) = (characters[j], characters[i]);
            }

            return new string(characters[..6]);
        }

        private string GenerateRandomPassword()
        {
            const string characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new System.Random();

            var randomPassword = new char[12];
            for (var i = 0; i < 12; i++)
            {
                randomPassword[i] = characters[random.Next(characters.Length)];
            }

            return new string(randomPassword);
        }

        private void LoadLoginPlatform()
        {
            var platform = (LoginPlatform)PlayerPrefs.GetInt(LoginPlatformKey);
            BackendLogin.instance.loginPlatform = platform;
            CheckLastPlatform();
        }

        private void SaveLoginPlatform(LoginPlatform loginPlatform)
        {
            PlayerPrefs.SetInt(LoginPlatformKey, (int)loginPlatform);
            BackendLogin.instance.loginPlatform = loginPlatform;
            CheckLastPlatform();
        }

        private void CheckLastPlatform()
        {
            checkLastLoginImage.rectTransform.anchoredPosition = BackendLogin.instance.loginPlatform switch
            {
                LoginPlatform.Apple => appleLoginButton.GetComponent<RectTransform>().anchoredPosition +
                                       new Vector2(300, 0),
                LoginPlatform.Google => googleLoginButton.GetComponent<RectTransform>().anchoredPosition +
                                        new Vector2(300, 0),
                LoginPlatform.Custom => emailLoginButton.GetComponent<RectTransform>().anchoredPosition +
                                        new Vector2(300, 0),
                _ => checkLastLoginImage.rectTransform.anchoredPosition
            };
        }

#endregion

#region Apple

        private void AppleInit()
        {
            if (AppleAuthManager.IsCurrentPlatformSupported)
            {
                var deserializer = new PayloadDeserializer();
                _appleAuthManager = new AppleAuthManager(deserializer);
            }

            if (_appleAuthManager == null)
            {
                CustomLog.Log("#  지원 안함  #");
                return;
            }

            _appleAuthManager.SetCredentialsRevokedCallback(result =>
            {
                CustomLog.Log("#  로그인 세션 삭제  #");
                CustomLog.Log("Received revoked callback " + result);
            });
        }

        private void AppleLogin()
        {
            var loginArgs = new AppleAuthLoginArgs(LoginOptions.None);
            CustomLog.Log("#  로그인 버튼 클릭  #");
            CustomLog.Log($"authmanager : {_appleAuthManager}");
            _appleAuthManager.LoginWithAppleId(loginArgs, credential =>
                {
                    CustomLog.Log("#  로그인 성공  #");
                    CustomLog.Log("# userID: #");
                    CustomLog.Log(credential.User);

                    if (credential is not IAppleIDCredential { IdentityToken: not null } appleIdCredential) return;
                    var identityToken = Encoding.UTF8.GetString(appleIdCredential.IdentityToken, 0,
                        appleIdCredential.IdentityToken.Length);

                    loginButtonsGroup.blocksRaycasts = false;
                    _loginButtonGroupTween.PlayBackwards();

                    var bro = Backend.BMember.AuthorizeFederation(identityToken, FederationType.Apple);
                    if (bro.IsSuccess())
                    {
                        federationPanelGroup.blocksRaycasts = false;
                        _federationGroupTween.PlayBackwards();
                        _connectionPanelGroupTween.OnComplete(() => connectionPanelGroup.blocksRaycasts = true)
                            .Restart();
                        BackendManager.BackendInit().Forget();
                        SaveLoginPlatform(LoginPlatform.Apple);
                        BackendLogin.instance.loginPlatform = LoginPlatform.Apple;
                    }
                    else CustomLog.LogError("Apple 로그인 실패");

                    // if (appleIdCredential is { AuthorizationCode: null }) return;
                    // CustomLog.Log("# authorizationCode:  #");
                    // CustomLog.Log(Encoding.UTF8.GetString(appleIdCredential.AuthorizationCode, 0,
                    //     appleIdCredential.AuthorizationCode.Length));
                },
                error =>
                {
                    CustomLog.Log("#  로그인 실패  #");
                    CustomLog.LogWarning("Sign in with Apple failed " + error.GetAuthorizationErrorCode() + error);
                });
        }

#endregion

#region Google

        private void StartGoogleLogin()
        {
            TheBackend.ToolKit.GoogleLogin.iOS.GoogleLogin(GoogleLoginCallback);
        }

        private void GoogleLoginCallback(bool isSuccess, string errorMessage, string token)
        {
            if (isSuccess == false)
            {
                CustomLog.LogError(errorMessage);
                return;
            }

            loginButtonsGroup.blocksRaycasts = false;
            _loginButtonGroupTween.PlayBackwards();

            CustomLog.Log("구글 토큰 : " + token);
            var bro = Backend.BMember.AuthorizeFederation(token, FederationType.Google);
            CustomLog.Log("페데레이션 로그인 결과 : " + bro);
            if (bro.IsSuccess())
            {
                BackendLogin.instance.loginPlatform = LoginPlatform.Google;

                federationPanelGroup.blocksRaycasts = false;
                _federationGroupTween.PlayBackwards();
                _connectionPanelGroupTween.OnComplete(() => connectionPanelGroup.blocksRaycasts = true)
                    .Restart();
                BackendManager.BackendInit().Forget();
                SaveLoginPlatform(LoginPlatform.Google);
            }
        }

#endregion
    }
}