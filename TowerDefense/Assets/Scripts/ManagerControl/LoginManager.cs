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

        private string _id, _oneTimeCode, _password;
        private string _wwwText;

        private bool _isFederationLogin;

        private Tween _loginPanelTween;
        private CancellationTokenSource _cts;

        private BackendManager _backendManager;
#if UNITY_IPHONE
        private IAppleAuthManager _appleAuthManager;
#endif
        [SerializeField] private Button appleLoginButton;
        [SerializeField] private Button emailLoginButton;
        [SerializeField] private Button googleLoginButton;

        [SerializeField] private Image blockImage;

        [SerializeField] private CanvasGroup loginButtonsGroup;
        [SerializeField] private CanvasGroup emailLoginPanelGroup;
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

        [SerializeField] private Button startButton;
        [SerializeField] private Button logOutButton;
        [SerializeField] private NoticePanel logOutNotifyPanel;

        private void Awake()
        {
            emailLoginPanelGroup.gameObject.SetActive(false);
            _backendManager = FindAnyObjectByType<BackendManager>();
            blockImage.enabled = false;
            timerBackground.enabled = false;
            timerText.text = "";
            oneTimeCodeField.interactable = false;
            Input.multiTouchEnabled = false;
            Time.timeScale = 1;
            Application.targetFrameRate = 60;
        }

        private void Start()
        {
            startButton.gameObject.SetActive(false);
            logOutButton.gameObject.SetActive(false);
            notifySendEmailObj.SetActive(false);
            AppleInit();
            GoogleInit();
            InitInputField();
            InitButtons();

            if (idField.text.Contains("@") && idField.text.Contains("."))
            {
                sendEmailButton.interactable = true;
                sendEmailBtnText.color = Color.red;
            }
        }

        private void Update()
        {
#if UNITY_IPHONE
            _appleAuthManager?.Update();
#endif
        }

        private void InitInputField()
        {
            idField.text = PlayerPrefs.GetString(UserEmailID);

            idField.OnPointerUpEvent += () =>
            {
                idField.ActivateInputField();
                TouchScreenKeyboard.Open("", TouchScreenKeyboardType.EmailAddress, false, false, false);
                content.DOLocalMoveY(350, 0.5f).SetEase(Ease.OutQuart);
            };
            oneTimeCodeField.OnPointerUpEvent += () =>
            {
                oneTimeCodeField.ActivateInputField();
                TouchScreenKeyboard.Open("", TouchScreenKeyboardType.NumberPad, false, false, false);
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
                loginButtonsGroup.gameObject.SetActive(false);
                blockImage.enabled = true;
                content.anchoredPosition = Vector2.zero;
                emailLoginPanelGroup.gameObject.SetActive(true);
            });
            goBackButton.onClick.AddListener(() =>
            {
                loginButtonsGroup.gameObject.SetActive(true);
                blockImage.enabled = false;
                emailLoginPanelGroup.gameObject.SetActive(false);
                loginButtonsGroup.DOFade(1, 0.5f).From(0);
            });
            googleLoginButton.onClick.AddListener(GoogleLogin);

            sendEmailButton.onClick.AddListener(() => SendEmail().Forget());
            loginButton.onClick.AddListener(() => EmailLogin());

            logOutNotifyPanel.OnOkButtonEvent += LogOut;
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
                            _isFederationLogin = true;
                            CustomLog.Log("Apple 로그인 성공");
                            loginButtonsGroup.gameObject.SetActive(false);
                            CustomLog.Log("========애플로그인");
                            _backendManager.BackendInit().Forget();
                            startButton.gameObject.SetActive(true);
                            logOutButton.gameObject.SetActive(true);
                            gameObject.SetActive(false);
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
            notifySendEmailObj.SetActive(true);
            PlayerPrefs.SetString(UserEmailID, idField.text);

            if (idField.text.Contains("@") && idField.text.Contains("."))
            {
                SetTimer().Forget();

                _id = idField.text.Trim();
                _oneTimeCode = GenerateRandomCode();
                _password = GenerateRandomPassword();

                var form = new WWWForm();
                form.AddField("order", "register");
                form.AddField("id", _id);
                form.AddField("password", _password);
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

                UniTask.RunOnThreadPool(() => { smtpServer.Send(mail); });
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

        private async UniTaskVoid EmailLogin()
        {
            if (oneTimeCodeField.text == _oneTimeCode)
            {
                _cts?.Cancel();

                var jObject = JObject.Parse(_wwwText);

                if (jObject["result"]?.ToString() == "SignUp")
                {
                    BackendLogin.instance.CustomSignUp(_id, _password);
                }
                else
                {
                    _password = jObject["msg"]?.ToString();
                }

                BackendLogin.instance.CustomLogin(_id, _password);

                if (byte.Parse((string)jObject["value"] ?? string.Empty) >= 7)
                {
                    Debug.Log("비밀변호 업데이트");
                    var oldPassword = _password;
                    _password = GenerateRandomPassword();

                    Backend.BMember.UpdatePassword(oldPassword, _password);

                    var form = new WWWForm();
                    form.AddField("order", "updatePassword");
                    form.AddField("id", _id);
                    form.AddField("password", _password);
                    form.AddField("count", 0);
                    await Post(form);
                    Debug.Log("업뎃 카운트 +1");
                }

                _backendManager.BackendInit().Forget();
                startButton.gameObject.SetActive(true);
                logOutButton.gameObject.SetActive(true);

                idField.interactable = true;
                oneTimeCodeField.interactable = false;
                timerBackground.enabled = false;
                sendEmailButton.interactable = true;
                notifySendEmailObj.SetActive(false);
                timerText.text = "";
                _oneTimeCode = "";
                oneTimeCodeField.text = "";
                content.DOLocalMoveY(0, 0.5f).SetEase(Ease.OutQuart);
                emailLoginPanelGroup.gameObject.SetActive(false);
                loginButtonsGroup.DOFade(1, 0.5f).From(0);
                gameObject.SetActive(false);
            }
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

            Debug.Log(_wwwText);
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
            if (_isFederationLogin)
            {
            }
            else
            {
                Backend.BMember.Logout();
                BackendChart.instance.InitItemTable();
                startButton.gameObject.SetActive(false);
                logOutButton.gameObject.SetActive(false);
                gameObject.SetActive(true);
                loginButtonsGroup.gameObject.SetActive(true);
            }
        }

#endregion
    }
}