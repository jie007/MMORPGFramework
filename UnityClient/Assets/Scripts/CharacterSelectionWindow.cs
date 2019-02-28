using System.Collections.Generic;
using Assets.Api;
using IO.Swagger.Model;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts
{
    public class CharacterSelectionWindow : MonoBehaviour
    {
        public TMP_InputField CharName;
        public TMP_Dropdown CharSelection;

        private List<CharacterInformation> Chars;

        public void Start()
        {
            Chars = RestApi.GetCharacters();
            Debug.Log("You have " + Chars.Count + " chars.");

            UpdateSelectableCharacters();
        }

        public void CreateCharacter()
        {
            var characterInfo = RestApi.CreateCharacter(CharName.text);
            if (characterInfo == null)
            {
                Debug.Log("Error creating character");
            }
            else
            {
                Chars.Add(characterInfo);
                UpdateSelectableCharacters();
            }
        }

        public void SelectCharacter()
        {
            string charToken = RestApi.SelectCharacter(CharSelection.options[CharSelection.value].text);
            if (string.IsNullOrEmpty(charToken))
            {
                Debug.Log("Error selecting character");
            }
            else
            {
                Context.Charname = CharSelection.options[CharSelection.value].text;
                SceneManager.LoadScene("MapScene");
            }
        }

        private void UpdateSelectableCharacters()
        {
            CharSelection.options.Clear();

            foreach(var c in Chars)
            {
                CharSelection.options.Add(new TMP_Dropdown.OptionData(c.Id));
            }
        }
    }
}
