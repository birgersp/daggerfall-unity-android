// Project:         Daggerfall Unity
// Copyright:       Copyright (C) 2009-2023 Daggerfall Workshop
// Web Site:        http://www.dfworkshop.net
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/Interkarma/daggerfall-unity
// Original Author: Birger
// Contributors:    
// 
// Notes:
//

using UnityEngine;
using System;
using System.Collections.Generic;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Items;

namespace DaggerfallWorkshop.Game.UserInterfaceWindows
{
    /// <summary>
    /// Implements an accessibility context menu designed for game controller support.
    /// </summary>
    public class DaggerfallContextMenuWindow : DaggerfallPopupWindow
    {
        enum MenuType
        {
            Root,
            Weapon,
            Character,
            Map,
            Spells,
            SetMode
        }

        Panel mainPanel = new Panel();
        TextLabel titleLabel = new TextLabel();
        ListBox listBox = new ListBox();
        TextLabel promptLabel = new TextLabel();

        Stack<MenuType> menuHistory = new Stack<MenuType>();
        MenuType currentMenu = MenuType.Root;

        bool stickAxisNeutral = true;
        bool dpadYAxisNeutral = true;
        bool dpadXAxisNeutral = true;

        public DaggerfallContextMenuWindow(IUserInterfaceManager uiManager, IUserInterfaceWindow previous = null)
            : base(uiManager, previous)
        {
        }

        public override void OnPush()
        {
            base.OnPush();
            if (GameManager.HasInstance && GameManager.Instance.PlayerMouseLook != null)
            {
                GameManager.Instance.PlayerMouseLook.ForceHideCursor(true);
            }
        }

        public override void OnPop()
        {
            base.OnPop();
            if (GameManager.HasInstance && GameManager.Instance.PlayerMouseLook != null)
            {
                GameManager.Instance.PlayerMouseLook.ForceHideCursor(false);
            }
        }

        protected override void Setup()
        {
            base.Setup();

            // Disable base popup cancellation to handle custom B/Esc logic manually
            AllowCancel = false;

            // Dim background
            ParentPanel.BackgroundColor = ScreenDimColor;

            // Configure main parchment panel (width/height multiples of 22)
            mainPanel.Size = new Vector2(154, 132);
            mainPanel.HorizontalAlignment = HorizontalAlignment.Center;
            mainPanel.VerticalAlignment = VerticalAlignment.Middle;
            DaggerfallUI.Instance.SetDaggerfallPopupStyle(DaggerfallUI.PopupStyle.Parchment, mainPanel);
            NativePanel.Components.Add(mainPanel);

            // Configure Title Label
            titleLabel.Position = new Vector2(10, 10);
            titleLabel.Size = new Vector2(134, 12);
            titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
            titleLabel.TextColor = new Color32(30, 30, 30, 255);
            titleLabel.ShadowColor = Color.clear;
            mainPanel.Components.Add(titleLabel);

            // Configure ListBox
            listBox.Position = new Vector2(10, 26);
            listBox.Size = new Vector2(134, 80);
            listBox.TextColor = new Color32(30, 30, 30, 255);
            listBox.SelectedTextColor = new Color32(162, 36, 12, 255);
            listBox.ShadowColor = Color.clear;
            listBox.SelectedShadowColor = Color.clear;
            listBox.AlwaysAcceptKeyboardInput = false;
            listBox.OnUseSelectedItem += ListBox_OnUseSelectedItem;
            mainPanel.Components.Add(listBox);

            // Configure Prompt/Button Hint Label at the bottom
            promptLabel.Position = new Vector2(10, 110);
            promptLabel.Size = new Vector2(134, 12);
            promptLabel.HorizontalAlignment = HorizontalAlignment.Center;
            promptLabel.TextScale = 0.8f;
            promptLabel.TextColor = new Color32(80, 80, 80, 255);
            promptLabel.ShadowColor = Color.clear;
            promptLabel.Text = "Up/Down: Select  A: Ok  B: Back  Esc: Close";
            mainPanel.Components.Add(promptLabel);

            // Open the root menu initially
            ChangeMenu(MenuType.Root);
        }

        void ChangeMenu(MenuType menu)
        {
            currentMenu = menu;
            listBox.ClearItems();

            switch (menu)
            {
                case MenuType.Root:
                    titleLabel.Text = "Context Menu";
                    listBox.AddItem("Recast spell");
                    listBox.AddItem("Weapon...");
                    listBox.AddItem("Spells...");
                    listBox.AddItem("Set mode...");
                    listBox.AddItem("Map...");
                    listBox.AddItem("Character...");
                    listBox.AddItem("Main menu (Esc)");
                    break;

                case MenuType.Weapon:
                    titleLabel.Text = "Weapon";
                    listBox.AddItem("Ready weapon");
                    listBox.AddItem("Switch hand");
                    break;

                case MenuType.Character:
                    titleLabel.Text = "Character";
                    listBox.AddItem("Status");
                    listBox.AddItem("Character sheet");
                    listBox.AddItem("Inventory");
                    break;

                case MenuType.Map:
                    titleLabel.Text = "Map";
                    listBox.AddItem("Local map");
                    listBox.AddItem("Travel map");
                    listBox.AddItem("Rest");
                    listBox.AddItem("Transport");
                    listBox.AddItem("Logbook");
                    listBox.AddItem("Notebook");
                    break;

                case MenuType.Spells:
                    titleLabel.Text = "Spells";
                    listBox.AddItem("Cast spell");
                    listBox.AddItem("Recast spell");
                    listBox.AddItem("Abort spell");
                    listBox.AddItem("Use magic item");
                    break;

                case MenuType.SetMode:
                    titleLabel.Text = "Set Mode";
                    listBox.AddItem("Steal");
                    listBox.AddItem("Grab");
                    listBox.AddItem("Info");
                    listBox.AddItem("Talk");
                    break;
            }

            listBox.SelectedIndex = 0;
        }

        public override void Update()
        {
            base.Update();

            // 1. Check keyboard / controller D-pad for menu selection wrapping
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                SelectPreviousWithWrap();
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                SelectNextWithWrap();
            }

            // 2. Check Right/Left Stick vertical axis for controller scrolling (using Axis2)
            float stickY = InputManager.GetAxisRawSafe("Axis2");
            if (Mathf.Abs(stickY) < 0.2f)
            {
                stickAxisNeutral = true;
            }
            else if (stickAxisNeutral)
            {
                if (stickY > 0.5f)
                {
                    SelectPreviousWithWrap();
                    stickAxisNeutral = false;
                }
                else if (stickY < -0.5f)
                {
                    SelectNextWithWrap();
                    stickAxisNeutral = false;
                }
            }

            // Check D-pad vertical axis for controller scrolling (using Axis6 or Axis8 for Linux/Logitech F710)
            float dpadY = InputManager.GetAxisRawSafe("Axis6") + InputManager.GetAxisRawSafe("Axis8");
            if (Mathf.Abs(dpadY) < 0.2f)
            {
                dpadYAxisNeutral = true;
            }
            else if (dpadYAxisNeutral)
            {
                // D-pad UP is negative (-1.0) and DOWN is positive (+1.0) on standard gamepads
                if (dpadY < -0.5f)
                {
                    SelectPreviousWithWrap();
                    dpadYAxisNeutral = false;
                }
                else if (dpadY > 0.5f)
                {
                    SelectNextWithWrap();
                    dpadYAxisNeutral = false;
                }
            }

            // Check D-pad horizontal axis for going back or confirming (using Axis5 or Axis7 for Linux/Logitech F710)
            float dpadX = InputManager.GetAxisRawSafe("Axis5") + InputManager.GetAxisRawSafe("Axis7");
            if (Mathf.Abs(dpadX) < 0.2f)
            {
                dpadXAxisNeutral = true;
            }
            else if (dpadXAxisNeutral)
            {
                // D-pad LEFT is negative (-1.0) and RIGHT is positive (+1.0)
                if (dpadX < -0.5f)
                {
                    GoBack();
                    dpadXAxisNeutral = false;
                }
                else if (dpadX > 0.5f)
                {
                    listBox.UseSelectedItem();
                    dpadXAxisNeutral = false;
                }
            }

            // 3. Confirm selection (Enter key, Space key, or controller A button/JoystickButton0)
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0))
            {
                listBox.UseSelectedItem();
            }

            // 4. Back / Cancel selection (Escape key closes directly, Backspace key / controller B button goes back)
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
                CloseWindow();
            }
            else if (Input.GetKeyDown(KeyCode.Backspace) || 
                     (InputManager.Instance.EnableController && InputManager.Instance.GetKeyDown(InputManager.Instance.GetJoystickUIBinding(InputManager.JoystickUIActions.Back), false)))
            {
                GoBack();
            }
        }

        void SelectPreviousWithWrap()
        {
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
            int nextIndex = listBox.SelectedIndex - 1;
            if (nextIndex < 0)
                nextIndex = listBox.Count - 1;
            listBox.SelectedIndex = nextIndex;
            listBox.ScrollToSelected();
        }

        void SelectNextWithWrap()
        {
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
            int nextIndex = listBox.SelectedIndex + 1;
            if (nextIndex >= listBox.Count)
                nextIndex = 0;
            listBox.SelectedIndex = nextIndex;
            listBox.ScrollToSelected();
        }

        void GoBack()
        {
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
            if (menuHistory.Count > 0)
            {
                ChangeMenu(menuHistory.Pop());
            }
            else
            {
                CloseWindow();
            }
        }

        void ListBox_OnUseSelectedItem()
        {
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
            string selected = listBox.SelectedItem;

            // Handle transition to sub-menus
            if (selected.EndsWith("..."))
            {
                menuHistory.Push(currentMenu);
                if (selected == "Weapon...")
                    ChangeMenu(MenuType.Weapon);
                else if (selected == "Spells...")
                    ChangeMenu(MenuType.Spells);
                else if (selected == "Set mode...")
                    ChangeMenu(MenuType.SetMode);
                else if (selected == "Map...")
                    ChangeMenu(MenuType.Map);
                else if (selected == "Character...")
                    ChangeMenu(MenuType.Character);
                return;
            }

            // Handle actions
            CloseWindow();
            ExecuteAction(selected);
        }

        void ExecuteAction(string actionName)
        {
            switch (actionName)
            {
                // Root Menu
                case "Recast spell":
                    RecastLastSpell();
                    break;
                case "Main menu (Esc)":
                    DaggerfallUI.PostMessage(DaggerfallUIMessages.dfuiOpenPauseOptionsDialog);
                    break;

                // Weapon
                case "Ready weapon":
                    GameManager.Instance.WeaponManager.ToggleSheath();
                    break;
                case "Switch hand":
                    ToggleWeaponHand();
                    break;

                // Character
                case "Status":
                    DaggerfallUI.PostMessage(DaggerfallUIMessages.dfuiStatusInfo);
                    break;
                case "Character sheet":
                    DaggerfallUI.PostMessage(DaggerfallUIMessages.dfuiOpenCharacterSheetWindow);
                    break;
                case "Inventory":
                    DaggerfallUI.PostMessage(DaggerfallUIMessages.dfuiOpenInventoryWindow);
                    break;

                // Map
                case "Local map":
                    DaggerfallUI.PostMessage(DaggerfallUIMessages.dfuiOpenAutomap);
                    break;
                case "Travel map":
                    DaggerfallUI.PostMessage(DaggerfallUIMessages.dfuiOpenTravelMapWindow);
                    break;
                case "Rest":
                    DaggerfallUI.PostMessage(DaggerfallUIMessages.dfuiOpenRestWindow);
                    break;
                case "Transport":
                    DaggerfallUI.PostMessage(DaggerfallUIMessages.dfuiOpenTransportWindow);
                    break;
                case "Logbook":
                    DaggerfallUI.PostMessage(DaggerfallUIMessages.dfuiOpenQuestJournalWindow);
                    break;
                case "Notebook":
                    DaggerfallUI.PostMessage(DaggerfallUIMessages.dfuiOpenNotebookWindow);
                    break;

                // Spells
                case "Cast spell":
                    DaggerfallUI.PostMessage(DaggerfallUIMessages.dfuiOpenSpellBookWindow);
                    break;
                case "Abort spell":
                    GameManager.Instance.PlayerEffectManager.AbortReadySpell();
                    break;
                case "Use magic item":
                    DaggerfallUI.PostMessage(DaggerfallUIMessages.dfuiOpenUseMagicItemWindow);
                    break;

                // Set mode
                case "Steal":
                    GameManager.Instance.PlayerActivate.ChangeInteractionMode(PlayerActivateModes.Steal);
                    break;
                case "Grab":
                    GameManager.Instance.PlayerActivate.ChangeInteractionMode(PlayerActivateModes.Grab);
                    break;
                case "Info":
                    GameManager.Instance.PlayerActivate.ChangeInteractionMode(PlayerActivateModes.Info);
                    break;
                case "Talk":
                    GameManager.Instance.PlayerActivate.ChangeInteractionMode(PlayerActivateModes.Talk);
                    break;
            }
        }

        void RecastLastSpell()
        {
            if (GameManager.Instance.PlayerEffectManager.LastSpell != null)
            {
                if (GameManager.Instance.PlayerEntity.Items.Contains(ItemGroups.MiscItems, (int)MiscItems.Spellbook))
                {
                    GameManager.Instance.PlayerEffectManager.SetReadySpell(GameManager.Instance.PlayerEffectManager.LastSpell);
                }
                else
                {
                    DaggerfallUI.AddHUDText(TextManager.Instance.GetLocalizedText("noSpellbook"));
                }
            }
            else
            {
                DaggerfallUI.AddHUDText("No spell to recast.");
            }
        }

        void ToggleWeaponHand()
        {
            bool newRightHand = !GameManager.Instance.WeaponManager.UsingRightHand;
            GameManager.Instance.WeaponManager.UsingRightHand = newRightHand;
            if (newRightHand)
                DaggerfallUI.Instance.PopupMessage(TextManager.Instance.GetLocalizedText("usingRightHand"));
            else
                DaggerfallUI.Instance.PopupMessage(TextManager.Instance.GetLocalizedText("usingLeftHand"));
        }
    }
}
