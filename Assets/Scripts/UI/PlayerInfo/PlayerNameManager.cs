using System;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using TMPro;
using System.Collections.Generic;
using Unity.Services.Multiplayer;
using Unity.Entities;
using Unity.Multiplayer.Widgets;
using Unity.Services.Vivox;

namespace IT4080C
{

    public class PlayerNameManager : MonoBehaviour
    {
        public ISession Session { get; set; }

        public TMP_InputField nameChangeInputField;
        public static PlayerNameManager Instance { get; private set; }

        public event Action<string> OnPlayerNameChanged;

        public TMP_Text playerNameHidden;

        private async void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                await InitializeAndSignInAsync();
            }
            else
            {
                Destroy(gameObject);
            }

        }

        private async Task InitializeAndSignInAsync()
        {
            try
            {
                await UnityServices.InitializeAsync();

                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                    Debug.Log($"Signed in as: {AuthenticationService.Instance.PlayerId}");
                    var displayName = AuthenticationService.Instance.PlayerName;

                    var loginOptions = new LoginOptions
                    {
                        DisplayName = displayName
                    };

                    await VivoxService.Instance.LoginAsync(loginOptions);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to initialize or sign in: {e.Message}");
            }
        }

        public string GetPlayerName()
        {
            return AuthenticationService.Instance.PlayerName;
        }

        public async Task<bool> SetPlayerNameAsync(string newName)
        {
            try
            {
                await AuthenticationService.Instance.UpdatePlayerNameAsync(newName);
                
                OnPlayerNameChanged?.Invoke(newName);
                Debug.Log($"Player name updated to: {newName}");
                GetPlayerName();
                return true;
            }
            catch (AuthenticationException e)
            {
                Debug.LogError($"Authentication error: {e}");
            }
            catch (RequestFailedException e)
            {
                Debug.LogError($"Request failed: {e}");
            }

            

            return false;
        }
        public async void SetPlayerNameButtonHandler()
        {
            await SetPlayerNameAsync(nameChangeInputField.text);
            playerNameHidden.text = nameChangeInputField.text;
            Debug.Log("Changing Player name to: " + nameChangeInputField.text);

/*
            await VivoxService.Instance.LogoutAsync();

            var updatedDisplayName = AuthenticationService.Instance.PlayerName;

            var newLoginOptions = new LoginOptions
            {
                DisplayName = nameChangeInputField.text
            };

            await VivoxService.Instance.LoginAsync(newLoginOptions);*/

        }
    }
}