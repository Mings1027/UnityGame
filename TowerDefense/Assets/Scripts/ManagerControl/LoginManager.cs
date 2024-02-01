using System.Text;
using AppleAuth;
using AppleAuth.Enums;
using AppleAuth.Extensions;
using AppleAuth.Interfaces;
using AppleAuth.Native;
using BackEnd;
using BackendControl;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace ManagerControl
{
    public class LoginManager : MonoBehaviour
    {
#if UNITY_IPHONE
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
                            FindAnyObjectByType<BackendManager>().BackendInit().Forget();
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
                    CustomLog.LogWarning("Sign in with Apple failed " + error.GetAuthorizationErrorCode().ToString() +
                                         error.ToString());
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

#endif
    }
}