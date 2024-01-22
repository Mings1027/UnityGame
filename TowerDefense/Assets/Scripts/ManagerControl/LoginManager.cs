using System;
using System.Text;
using AppleAuth;
using AppleAuth.Enums;
using AppleAuth.Extensions;
using AppleAuth.Interfaces;
using AppleAuth.Native;
using BackEnd;
using BackendControl;
using LobbyControl;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ManagerControl
{
    public class LoginManager : MonoBehaviour
    {
#if UNITY_IPHONE
        // private const string AppleUserIdKey = "AppleUserId";
        private IAppleAuthManager _appleAuthManager;
        [SerializeField] private Button appleLoginButton;
#endif

        private void Start()
        {
#if UNITY_IPHONE
            appleLoginButton.gameObject.SetActive(true);
            AppleInit();
            appleLoginButton.onClick.AddListener(AppleLogin);
#endif
        }

        private void Update()
        {
#if UNITY_IPHONE
            _appleAuthManager?.Update();
#endif
        }

#if UNITY_IPHONE
        private void AppleInit()
        {
            if (AppleAuthManager.IsCurrentPlatformSupported)
            {
                var deserializer = new PayloadDeserializer();
                _appleAuthManager = new AppleAuthManager(deserializer);
            }

            InitializeAppleLogin();
        }

        private void InitializeAppleLogin()
        {
            if (_appleAuthManager == null)
            {
                Debug.Log("#  지원 안함  #");
                return;
            }

            _appleAuthManager.SetCredentialsRevokedCallback(result =>
            {
                Debug.Log("#  로그인 세션 삭제  #");
                Debug.Log("Received revoked callback " + result);
                // PlayerPrefs.DeleteKey(AppleUserIdKey);
            });
        }

        private void AppleLogin()
        {
            var loginArgs = new AppleAuthLoginArgs(LoginOptions.None);
            Debug.Log("#  로그인 버튼 클릭  #");

            _appleAuthManager.LoginWithAppleId(loginArgs, credential =>
                {
                    Debug.Log("#  로그인 성공  #");
                    Debug.Log("# userID: #");
                    Debug.Log(credential.User);
                    var appleIdCredential = credential as IAppleIDCredential;
                    var passwordCredential = credential as IPasswordCredential;

                    if (appleIdCredential.IdentityToken != null)
                    {
                        var identityToken = Encoding.UTF8.GetString(appleIdCredential.IdentityToken, 0,
                            appleIdCredential.IdentityToken.Length);
                        // Debug.Log($"# identityToken:  {identityToken}#");
                        // Debug.Log(passwordCredential != null
                        //     ? $"password : {passwordCredential.Password}"
                        //     : "passwordcredential is null");
                        var bro = Backend.BMember.AuthorizeFederation(identityToken, FederationType.Apple);

                        if (bro.IsSuccess())
                        {
                            Debug.Log("Apple 로그인 성공");
                            appleLoginButton.gameObject.SetActive(false);
                            FindAnyObjectByType<DownloadManager>().CheckUpdateFiles().Forget();
                            FindAnyObjectByType<BackendManager>().BackendInit();
                        }
                        else Debug.LogError("Apple 로그인 실패");
                    }

                    if (appleIdCredential.AuthorizationCode != null)
                    {
                        var authorizationCode = Encoding.UTF8.GetString(appleIdCredential.AuthorizationCode, 0,
                            appleIdCredential.AuthorizationCode.Length);
                        Debug.Log("# authorizationCode:  #");
                        Debug.Log(authorizationCode);
                    }
                },
                error =>
                {
                    Debug.Log("#  로그인 실패  #");
                    var authorizationErrorCode = error.GetAuthorizationErrorCode();
                    Debug.LogWarning("Sign in with Apple failed " + authorizationErrorCode.ToString() + " " +
                                     error.ToString());
                });
        }

        private void GameCenterLogin()
        {
            if (Social.localUser.authenticated)
            {
                Debug.Log("Success to true");
            }
            else
            {
                Social.localUser.Authenticate(success =>
                {
                    Debug.Log(success ? "Success to authenticate" : "Failed to login");
                });
            }
        }

#endif
    }
}