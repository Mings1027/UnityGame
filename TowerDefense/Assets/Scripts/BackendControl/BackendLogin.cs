using BackEnd;
using GameControl;
using UnityEngine;
using Utilities;

namespace BackendControl
{
    public class BackendLogin
    {
        private static BackendLogin _instance;
        public static BackendLogin instance => _instance ??= new BackendLogin();

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
                Debug.LogError("회원가입에 실패했습니다. : " + bro);
            }
        }

        public void CustomLogin(string id, string pw)
        {
            CustomLog.Log("로그인을 요청합니다.");

            var bro = Backend.BMember.CustomLogin(id, pw);

            if (bro.IsSuccess())
            {
                CustomLog.Log("로그인이 성공했습니다. : " + bro);
            }
            else
            {
                Debug.LogError("로그인이 실패했습니다. : " + bro);
            }
        }

        public void UpdateNickname(string nickname)
        {
            CustomLog.Log("닉네임 변경을 요청합니다.");

            var bro = Backend.BMember.UpdateNickname(nickname);

            if (bro.IsSuccess())
            {
                CustomLog.Log("닉네임 변경에 성공했습니다 : " + bro);
            }
            else
            {
                Debug.LogError("닉네임 변경에 실패했습니다 : " + bro);
            }
        }
    }
}