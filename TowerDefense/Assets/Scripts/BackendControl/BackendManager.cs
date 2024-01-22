using System;
using System.Threading.Tasks;
using BackEnd;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace BackendControl
{
    public class BackendManager : MonoBehaviour
    {
        // [SerializeField] private InputField idInputField;
        // [SerializeField] private InputField passwordInputField;

        private void Awake()
        {
            var bro = Backend.Initialize(true);

            if (bro.IsSuccess())
            {
                Debug.Log("초기화 성공 : " + bro);
            }
            else
            {
                Debug.LogError("초기화 실패 : " + bro);
            }
        }

        public async void BackendInit()
        {
            await Task.Run(() =>
            {
                var bro = Backend.BMember.GetUserInfo();
                if (bro.IsSuccess())
                {
                    Debug.Log("유저정보를 찾음");
                    var id = bro.GetReturnValuetoJSON()["row"]["gamerId"].ToString();
                    var inDate = bro.GetReturnValuetoJSON()["row"]["inDate"].ToString();
                    BackendGameData.instance.GameDataInsert();
                }

                Debug.Log("테스트를 종료합니다");
            });
        }
    }
}