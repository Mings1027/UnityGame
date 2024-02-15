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
using UnityEngine.Serialization;
using UnityEngine.UI;
using Utilities;
using Random = UnityEngine.Random;

namespace ManagerControl
{
    public class LoginManager : MonoBehaviour
    {
        private const string UserEmailID = "User Email ID";
        private const string LoginPlatformKey = "LoginPlatform";

        private LoginPlatform _loginPlatform;

        private string _id, _oneTimeCode, _password;
        private string _wwwText;

        private Tween _loginButtonGroupTween;
        private Tween _connectionPanelGroupTween;

        private CancellationTokenSource _cts;

        private BackendManager _backendManager;
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

        [SerializeField] private CanvasGroup connectionPanelGroup;
        [SerializeField] private NoticePanel signUpConfirmPanel;
        [SerializeField] private NoticePanel logOutNoticePanel;

        private void Start()
        {
            Init();
            AppleInit();
            GoogleInit();
            InitInputField();
            InitButtons();
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

        private void Init()
        {
            _loginButtonGroupTween = loginButtonsGroup.DOFade(1, 0.25f).From(0).SetAutoKill(false)
                .Pause();
            _loginButtonGroupTween.Restart();

            _connectionPanelGroupTween = connectionPanelGroup.DOFade(1, 0.25f).From(0).SetAutoKill(false)
                .Pause();
            connectionPanelGroup.blocksRaycasts = false;
            emailLoginPanelGroupObj.SetActive(false);
            notifySendEmailObj.SetActive(false);
            _backendManager = FindAnyObjectByType<BackendManager>();
            timerBackground.enabled = false;
            timerText.text = "";
            oneTimeCodeField.interactable = false;
            loginButton.interactable = false;

            var lastPlatform = PlayerPrefs.GetInt(LoginPlatformKey);
            checkLastLoginImage.enabled = lastPlatform != 0;
            LoadLoginPlatform();

            Input.multiTouchEnabled = false;
            Time.timeScale = 1;
            Application.targetFrameRate = 60;
        }

        private void InitInputField()
        {
            idField.text = PlayerPrefs.GetString(UserEmailID);
            if (idField.text.Contains("@") && idField.text.Contains("."))
            {
                sendEmailButton.interactable = true;
                sendEmailBtnText.color = Color.red;
            }

            idField.OnPointerUpEvent += () =>
            {
                idField.ActivateInputField();
                TouchScreenKeyboard.hideInput = true;
                content.DOLocalMoveY(350, 0.5f).SetEase(Ease.OutQuart);
            };
            oneTimeCodeField.OnPointerUpEvent += () =>
            {
                oneTimeCodeField.ActivateInputField();
                TouchScreenKeyboard.hideInput = true;
                content.DOLocalMoveY(350, 0.5f).SetEase(Ease.OutQuart);
            };

            idField.onSubmit.AddListener(_ => { content.DOLocalMoveY(0, 0.5f).SetEase(Ease.OutQuart); });
            idField.onValueChanged.AddListener(_ => { CheckEmailForm(); });

            oneTimeCodeField.onSubmit.AddListener(_ => { content.DOLocalMoveY(0, 0.5f).SetEase(Ease.OutQuart); });
        }

        private void InitButtons()
        {
            appleLoginButton.onClick.AddListener(AppleLogin);
            emailLoginButton.onClick.AddListener(() =>
            {
                // blockImage.enabled = true;
                content.anchoredPosition = Vector2.zero;
                emailLoginPanelGroupObj.SetActive(true);
                _loginButtonGroupTween.PlayBackwards();
            });
            goBackButton.onClick.AddListener(() =>
            {
                // blockImage.enabled = false;
                emailLoginPanelGroupObj.SetActive(false);
                _loginButtonGroupTween.Restart();
            });
            googleLoginButton.onClick.AddListener(GoogleLogin);

            sendEmailButton.onClick.AddListener(() => SendEmail().Forget());
            loginButton.onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                Login().Forget();
            });

            logOutNoticePanel.OnConfirmButtonEvent += LogOut;
            signUpConfirmPanel.OnConfirmButtonEvent += async () =>
            {
                loginButton.interactable = false;
                _cts?.Cancel();
                await StartEmailSignUp();
                StartEmailLogin().Forget();
            };
        }

#region APPLE

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

            _appleAuthManager.LoginWithAppleId(loginArgs, credential =>
                {
                    CustomLog.Log("#  로그인 성공  #");
                    CustomLog.Log("# userID: #");
                    CustomLog.Log(credential.User);
                    var appleIdCredential = credential as IAppleIDCredential;
                    var passwordCredential = credential as IPasswordCredential;

                    if (appleIdCredential.IdentityToken != null)
                    {
                        var identityToken = Encoding.UTF8.GetString(appleIdCredential.IdentityToken, 0,
                            appleIdCredential.IdentityToken.Length);

                        var bro = Backend.BMember.AuthorizeFederation(identityToken, FederationType.Apple);
                        appleLoginButton.interactable = false;
                        if (bro.IsSuccess())
                        {
                            BackendLogin.instance.FederationLogin();
                            _loginButtonGroupTween.PlayBackwards();
                            _connectionPanelGroupTween.OnComplete(() => connectionPanelGroup.blocksRaycasts = true)
                                .Restart();
                            _backendManager.BackendInit().Forget();
                            SaveLoginPlatform(LoginPlatform.Apple);
                        }
                        else CustomLog.LogError("Apple 로그인 실패");

                        appleLoginButton.interactable = true;
                    }

                    if (appleIdCredential.AuthorizationCode == null) return;
                    CustomLog.Log("# authorizationCode:  #");
                    CustomLog.Log(Encoding.UTF8.GetString(appleIdCredential.AuthorizationCode, 0,
                        appleIdCredential.AuthorizationCode.Length));
                },
                error =>
                {
                    CustomLog.Log("#  로그인 실패  #");
                    CustomLog.LogWarning("Sign in with Apple failed " + error.GetAuthorizationErrorCode() + error);
                });
        }

        private void GameCenterLogin()
        {
            if (Social.localUser.authenticated)
            {
                CustomLog.Log("Success to true");
            }
            else
            {
                Social.localUser.Authenticate(success =>
                {
                    CustomLog.Log(success ? "Success to authenticate" : "Failed to login");
                });
            }
        }

#endregion

#region GOOGLE

        private void GoogleInit()
        {
        }

        private void GoogleLogin()
        {
        }

#endregion

#region EMAIL

        private async UniTaskVoid SendEmail()
        {
            oneTimeCodeField.interactable = true;
            loginButton.interactable = true;
            notifySendEmailObj.SetActive(true);
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

            idField.interactable = true;
            oneTimeCodeField.interactable = false;
            timerBackground.enabled = false;
            sendEmailButton.interactable = true;
            notifySendEmailObj.SetActive(false);
            timerText.text = "";
        }

        private async UniTaskVoid Login()
        {
            if (oneTimeCodeField.text == _oneTimeCode)
            {
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

                    StartEmailLogin().Forget();
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

        private async UniTask StartEmailLogin()
        {
            BackendLogin.instance.CustomLogin(_id, _password);
            var countJson = JObject.Parse(_wwwText);
            var curCount = byte.Parse((string)countJson["value"] ?? string.Empty);
            if (curCount >= 7)
            {
                var oldPassword = _password;
                _password = GenerateRandomPassword();

                Backend.BMember.UpdatePassword(oldPassword, _password, _ => { });
                var form = new WWWForm();
                form.AddField("order", "updatePassword");
                form.AddField("id", _id);
                form.AddField("password", _password);
                await Post(form);
            }

            _connectionPanelGroupTween.OnComplete(() => connectionPanelGroup.blocksRaycasts = true).Restart();
            _backendManager.BackendInit().Forget();
            SaveLoginPlatform(LoginPlatform.Custom);

            idField.interactable = true;
            oneTimeCodeField.interactable = false;
            timerBackground.enabled = false;
            sendEmailButton.interactable = true;
            notifySendEmailObj.SetActive(false);
            timerText.text = "";
            _oneTimeCode = "";
            oneTimeCodeField.text = "";
            content.DOLocalMoveY(0, 0.5f).SetEase(Ease.OutQuart);
            emailLoginPanelGroupObj.SetActive(false);
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

        private void LogOut()
        {
            BackendLogin.instance.LogOut();
            BackendChart.instance.InitItemTable();
            _connectionPanelGroupTween.OnRewind(() => connectionPanelGroup.blocksRaycasts = false).PlayBackwards();
            _loginButtonGroupTween.Restart();
        }

        private void LoadLoginPlatform()
        {
            var platform = (LoginPlatform)PlayerPrefs.GetInt(LoginPlatformKey);
            _loginPlatform = platform;
            CheckLastPlatform();
        }

        private void SaveLoginPlatform(LoginPlatform loginPlatform)
        {
            PlayerPrefs.SetInt(LoginPlatformKey, (int)loginPlatform);
            _loginPlatform = loginPlatform;
            CheckLastPlatform();
        }

        private void CheckLastPlatform()
        {
            switch (_loginPlatform)
            {
                case LoginPlatform.Apple:
                    checkLastLoginImage.rectTransform.anchoredPosition =
                        appleLoginButton.GetComponent<RectTransform>().anchoredPosition + new Vector2(300, 0);
                    break;
                case LoginPlatform.Google:
                    checkLastLoginImage.rectTransform.anchoredPosition =
                        googleLoginButton.GetComponent<RectTransform>().anchoredPosition + new Vector2(300, 0);
                    break;
                case LoginPlatform.Custom:
                    checkLastLoginImage.rectTransform.anchoredPosition =
                        emailLoginButton.GetComponent<RectTransform>().anchoredPosition + new Vector2(300, 0);
                    break;
            }
        }

#endregion
    }
}