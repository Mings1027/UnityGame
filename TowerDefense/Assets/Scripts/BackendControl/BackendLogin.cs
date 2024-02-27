using BackEnd;
using UnityEngine;
using Utilities;

namespace BackendControl
{
    public class BackendLogin
    {
        private static BackendLogin _instance;
        public static BackendLogin instance => _instance ??= new BackendLogin();
        private bool _isFederationLogin;

        public void CustomSignUp(string id, string pw)
        {
            CustomLog.Log("회원가입을 요청합니다.");

            var bro = Backend.BMember.CustomSignUp(id, pw);

            if (bro.IsSuccess())
            {
                CustomLog.Log("회원가입에 성공했습니다. : " + bro);
            }
            else
            {
                CustomLog.LogError("회원가입에 실패했습니다. : " + bro);
            }
        }

        public void CustomLogin(string id, string pw)
        {
            CustomLog.Log("로그인을 요청합니다.");

            var bro = Backend.BMember.CustomLogin(id, pw);

            if (bro.IsSuccess())
            {
                CustomLog.Log("로그인이 성공했습니다. : " + bro);
                return;
            }

            CustomLog.LogError("로그인이 실패했습니다. : " + bro);
        }

        public void FederationLogin()
        {
            _isFederationLogin = true;
        }

        public void LogOut()
        {
            if (_isFederationLogin)
            {
                _isFederationLogin = false;
            }
            else Backend.BMember.Logout();
        }

        public (bool, string) UpdateNickname(string nickname)
        {
            CustomLog.Log("닉네임 변경을 요청합니다.");

            var bro = Backend.BMember.UpdateNickname(nickname);

            if (bro.IsSuccess())
            {
                CustomLog.Log("닉네임 변경에 성공했습니다 : " + bro);
                return (true, "204");
            }

            CustomLog.LogError("닉네임 변경에 실패했습니다 : " + bro);
            return (false, bro.GetStatusCode());
        }

        public string GetUserNickName()
        {
            var bro = Backend.BMember.GetUserInfo();
            string nickName;
            if (bro.IsSuccess())
            {
                CustomLog.Log("닉네임 찾음");
                nickName = bro.GetReturnValuetoJSON()["row"]["nickname"].ToString();
            }
            else
            {
                CustomLog.Log("닉네임 못찾음");
                nickName = bro.GetReturnValuetoJSON()["row"]["gamerId"].ToString()[..7];
            }

            return nickName;
        }
    }
}