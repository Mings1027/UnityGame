using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.U2D.Animation;

namespace GameControl
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] private InputActionReference manu;

        [SerializeField] private EventSystem eventSystem;

        //Panel
        [SerializeField] private GameObject readyPanel, selectPlayerPanel, optionPanel;
        //Ready Panel Button

        [SerializeField] private GameObject playButton, selectPlayerButton, optionButton;

        //Select Player Panel Button
        [SerializeField] private GameObject spLeftButton;

        //Option Panel Button
        [SerializeField] private GameObject opButton1;

        private readonly Stack<GameObject> panelStack = new(), buttonStack = new();

        [SerializeField] private GameObject playerSkin;
        [SerializeField] private List<SpriteLibraryAsset> skins = new();
        [SerializeField] private int curSelectedSkin;
        [SerializeField] private SpriteLibrary curPlayerSkin;
        [SerializeField] private SpriteLibrary leftPlayerSkin, rightPlayerSkin;

        private void Awake()
        {
            eventSystem.SetSelectedGameObject(playButton);
            panelStack.Push(readyPanel);
        }

        private void Update()
        {
            if (!manu.action.triggered) return;
            if (panelStack.Count == 1) return;
            panelStack.Pop().SetActive(false);
            panelStack.Peek().SetActive(true);
            eventSystem.SetSelectedGameObject(buttonStack.Pop());
        }

        public void PlayGame()
        {
            PrefabUtility.SaveAsPrefabAsset(playerSkin, "Assets/Prefabs/PlayerSprite.prefab");
            SceneManager.LoadScene("PlayScene");
        }

        public void SelectPlayer()
        {
            PanelManager.PanelControl(panelStack, selectPlayerPanel);
            buttonStack.Push(selectPlayerButton);
            eventSystem.SetSelectedGameObject(spLeftButton);
        }

        public void Option()
        {
            PanelManager.PanelControl(panelStack, optionPanel);
            buttonStack.Push(optionButton);
            eventSystem.SetSelectedGameObject(opButton1);
        }

        public void LeftButton()
        {
            var rightSkin = curSelectedSkin;
            curSelectedSkin -= 1;
            curSelectedSkin = curSelectedSkin < 0 ? skins.Count - 1 : curSelectedSkin;
            curPlayerSkin.spriteLibraryAsset = skins[curSelectedSkin];

            if (curSelectedSkin < 0)
            {
                leftPlayerSkin.spriteLibraryAsset = skins[curSelectedSkin - 1];
                rightPlayerSkin.spriteLibraryAsset = skins[rightSkin];
            }
            else
            {
                leftPlayerSkin.spriteLibraryAsset = curSelectedSkin == 0 ? skins.Last() : skins[curSelectedSkin - 1];
                rightPlayerSkin.spriteLibraryAsset =
                    curSelectedSkin == 0 ? skins[curSelectedSkin + 1] : skins[rightSkin];
            }
        }

        public void RightButton()
        {
            var leftSkin = curSelectedSkin;
            curSelectedSkin += 1;
            curSelectedSkin = curSelectedSkin < skins.Count ? curSelectedSkin : 0;
            curPlayerSkin.spriteLibraryAsset = skins[curSelectedSkin];
            leftPlayerSkin.spriteLibraryAsset = skins[leftSkin];

            if (curSelectedSkin < skins.Count)
            {
                rightPlayerSkin.spriteLibraryAsset =
                    curSelectedSkin == skins.Count - 1 ? skins.First() : skins[curSelectedSkin + 1];
            }
            else
            {
                rightPlayerSkin.spriteLibraryAsset = skins[curSelectedSkin + 1];
            }
        }
    }
}