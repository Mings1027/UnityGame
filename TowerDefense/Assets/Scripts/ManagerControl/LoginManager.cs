using System;
using System.Net;
using System.Net.Mail;
using System.Text;
using AppleAuth;
using AppleAuth.Enums;
using AppleAuth.Extensions;
using AppleAuth.Interfaces;
using AppleAuth.Native;
using BackEnd;
using BackendControl;
using Cysharp.Threading.Tasks;
using DG.Tweening;
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

        private string _id, _oneTimeCode;
        private string _wwwText;

        private bool _isFederationLogin;

        private Tween _loginPanelTween;
        private Tween _logOutPanelTween;

        private BackendManager _backendManager;
#if UNITY_IPHONE
        private IAppleAuthManager _appleAuthManager;
#endif
        [SerializeField] private GameObject loginButtonParent;
        [SerializeField] private Button appleLoginButton;
        [SerializeField] private Button emailLoginButton;
        [SerializeField] private Button googleLoginButton;
        [SerializeField] private Image blockImage;
        [SerializeField] private RectTransform loginPanel;

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
        [SerializeField] private NotificationPanel logOutNotifyPanel;

        private void Awake()
        {
            _loginPanelTween = loginPanel.DOScale(1, 0.25f).From(0).SetEase(Ease.OutBack).SetAutoKill(false).Pause();
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

        private void OnDisable()
        {
            _loginPanelTween?.Kill();
        }

        private void InitInputField()
        {
            idField.text = PlayerPrefs.GetString(UserEmailID);

            idField.OnPointerUpEvent += () =>
            {
                idField.ActivateInputField();
                TouchScreenKeyboard.Open("", TouchScreenKeyboardType.EmailAddress, false, false, false);
                loginPanel.DOLocalMoveY(350, 0.5f).SetEase(Ease.OutQuart);
            };
            oneTimeCodeField.OnPointerUpEvent += () =>
            {
                oneTimeCodeField.ActivateInputField();
                TouchScreenKeyboard.Open("", TouchScreenKeyboardType.NumberPad, false, false, false);
                loginPanel.DOLocalMoveY(350, 0.5f).SetEase(Ease.OutQuart);
            };

            idField.onSubmit.AddListener(_ => { loginPanel.DOLocalMoveY(0, 0.5f).SetEase(Ease.OutQuart); });
            idField.onValueChanged.AddListener(_ => { CheckEmailForm(); });

            oneTimeCodeField.onSubmit.AddListener(_ => { loginPanel.DOLocalMoveY(0, 0.5f).SetEase(Ease.OutQuart); });
        }

        private void InitButtons()
        {
            appleLoginButton.onClick.AddListener(AppleLogin);
            emailLoginButton.onClick.AddListener(() =>
            {
                loginButtonParent.SetActive(false);
                blockImage.enabled = true;
                _loginPanelTween.Restart();
            });
            goBackButton.onClick.AddListener(() =>
            {
                loginButtonParent.SetActive(true);
                blockImage.enabled = false;
                _loginPanelTween.PlayBackwards();
            });
            googleLoginButton.onClick.AddListener(GoogleLogin);

            sendEmailButton.onClick.AddListener(() => SendEmail().Forget());
            loginButton.onClick.AddListener(EmailLogin);

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
                            loginButtonParent.SetActive(false);
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
            PlayerPrefs.SetString(UserEmailID, idField.text);

            if (idField.text.Contains("@") && idField.text.Contains("."))
            {
                SetTimer().Forget();

                _id = idField.text.Trim();
                _oneTimeCode = GenerateRandomCode();

                var form = new WWWForm();
                form.AddField("order", "register");
                form.AddField("id", _id);
                form.AddField("oneTimeCode", _oneTimeCode);
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

            var timer = 60;
            while (timer > 0)
            {
                timer -= 1;
                timerText.text = timer + "s";
                await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: this.GetCancellationTokenOnDestroy());
            }

            idField.interactable = true;
            oneTimeCodeField.interactable = false;
            timerBackground.enabled = false;
            sendEmailButton.interactable = true;
            timerText.text = "";
            _oneTimeCode = "";
        }

        private void EmailLogin()
        {
            if (oneTimeCodeField.text == _oneTimeCode)
            {
                _wwwText = _wwwText[11..];
                var endIndex = _wwwText.Length - 2;
                _wwwText = _wwwText[..endIndex];

                if (_wwwText == "SignUp")
                {
                    BackendLogin.instance.CustomSignUp(_id, "f5e6F6B755Da");
                }

                BackendLogin.instance.CustomLogin(_id, "f5e6F6B755Da");

                _backendManager.BackendInit().Forget();
                startButton.gameObject.SetActive(true);
                logOutButton.gameObject.SetActive(true);

                oneTimeCodeField.text = "";
                loginPanel.DOLocalMoveY(0, 0.5f).SetEase(Ease.OutQuart);
                _loginPanelTween.PlayBackwards();
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
                startButton.gameObject.SetActive(false);
                logOutButton.gameObject.SetActive(false);
                gameObject.SetActive(true);
            }
        }

#endregion
    }
}