using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
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
using UnityEditor.Localization.Plugins.XLIFF.V20;
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

        private Tween _loginPanelTween;

        private BackendManager _backendManager;
#if UNITY_IPHONE
        private IAppleAuthManager _appleAuthManager;
#endif
        [SerializeField] private GameObject loginButtons;
        [SerializeField] private Button appleLoginButton;
        [SerializeField] private Button emailLoginButton;
        [SerializeField] private Button googleLoginButton;
        [SerializeField] private Image blockImage;
        [SerializeField] private RectTransform loginPanel;

        [SerializeField] private Button goBackButton;
        [SerializeField] private Button loginButton;
        [SerializeField] private TMP_InputField idField;
        [SerializeField] private TMP_InputField passwordField;
        [SerializeField] private string myEmail;
        [SerializeField] private string myPassword;
        [SerializeField] private Button sendEmailButton;

        private void Awake()
        {
            _loginPanelTween = loginPanel.DOScale(1, 0.25f).From(0).SetEase(Ease.OutBack).SetAutoKill(false).Pause();
            _backendManager = FindAnyObjectByType<BackendManager>();
            blockImage.enabled = false;
        }

        private void Start()
        {
            PlayerPrefs.DeleteAll();
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
            _loginPanelTween?.Kill();
        }

        private void InitInputField()
        {
            idField.text = PlayerPrefs.GetString(UserEmailID) ?? "";

            idField.onSelect.AddListener(_ => { loginPanel.DOLocalMoveY(350, 0.5f).SetEase(Ease.OutQuart); });
            idField.onDeselect.AddListener(_ => { loginPanel.DOLocalMoveY(0, 0.5f).SetEase(Ease.OutQuart); });

            passwordField.onSelect.AddListener(_ => { loginPanel.DOLocalMoveY(350, 0.5f).SetEase(Ease.OutQuart); });
            passwordField.onDeselect.AddListener(_ => { loginPanel.DOLocalMoveY(0, 0.5f).SetEase(Ease.OutQuart); });
        }

        private void InitButtons()
        {
            appleLoginButton.onClick.AddListener(AppleLogin);
            emailLoginButton.onClick.AddListener(() =>
            {
                loginButtons.SetActive(false);
                blockImage.enabled = true;
                _loginPanelTween.Restart();
            });
            goBackButton.onClick.AddListener(() =>
            {
                loginButtons.SetActive(true);
                blockImage.enabled = false;
                _loginPanelTween.PlayBackwards();
            });
            loginButton.onClick.AddListener(() =>
            {
                if (idField.text.Length > 0 && passwordField.text.Length > 0)
                {
                    if (BackendLogin.instance.CustomLogin(idField.text, passwordField.text))
                    {
                        _backendManager.BackendInit().Forget();
                        return;
                    }

                    BackendLogin.instance.CustomSignUp(idField.text, passwordField.text);
                    BackendLogin.instance.CustomLogin(idField.text, passwordField.text);
                    PlayerPrefs.SetString(UserEmailID, idField.text);
                    _backendManager.BackendInit().Forget();
                }
            });
            googleLoginButton.onClick.AddListener(GoogleLogin);

            sendEmailButton.onClick.AddListener(SendEmail);
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
                            CustomLog.Log("Apple 로그인 성공");
                            appleLoginButton.gameObject.SetActive(false);
                            _backendManager.BackendInit().Forget();
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

        private void SendEmail()
        {
            sendEmailButton.interactable = false;

            var newCode = GenerateRandomCode()[..6];
            if (BackendLogin.instance.UpdatePassword(idField.text, newCode))
            {
                var mail = new MailMessage();
                mail.From = new MailAddress(myEmail);
                mail.To.Add(idField.text);
                mail.Subject = "Test Mail";
                mail.Body = "This is for testing smtp mail from gmail";
                var smtpServer = new SmtpClient("smtp.gmail.com");
                smtpServer.Port = 587;
                smtpServer.Credentials = new NetworkCredential(myEmail, myPassword);
                smtpServer.EnableSsl = true;
                ServicePointManager.ServerCertificateValidationCallback =
                    (s, certificate, chain, sslPolicyErrors) => true;

                UniTask.RunOnThreadPool(() => { smtpServer.Send(mail); });
            }
        }

        private string GenerateRandomCode()
        {
            const string code = "0123456789";
            var characters = code.ToCharArray();
            var random = new System.Random();
            for (var i = characters.Length - 1; i >= 0; i--)
            {
                var c = random.Next(0, i + 1);
                (characters[i], characters[c]) = (characters[c], characters[i]);
            }

            return new string(characters);
        }

#endregion
    }
}