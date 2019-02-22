using System;
using Assets.Api;
using Common.RestApi;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts
{
    public class LoginWindow : MonoBehaviour
    {
        public TMP_InputField Email;
        public TMP_InputField Password;

        public TMP_Text ErrorMessage;

        public void Login()
        {
            try
            {
                string token = RestApi.Login(Email.text, Password.text);

                if (string.IsNullOrEmpty(token))
                {
                    ErrorMessage.text = "Your password or E-Mail is wrong";
                }

                SceneManager.LoadScene("CharacterSelection");
            }
            catch (Exception e)
            {
                ErrorMessage.text = e.ToString();
                throw;
            }
        }

        public void Register()
        {
            var result = RestApi.Register(Email.text, Password.text);

            if (result != RegisterResult.Ok)
            {
                ErrorMessage.text = "Register Error: " + result;
            }
        }
    }
}
