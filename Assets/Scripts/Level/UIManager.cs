using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Level
{
    public class UIManager : MonoBehaviour
    {
        private const string PREFIX = "Hero <color=black>Score:</color>";

        [FormerlySerializedAs("textTmp")] [SerializeField]
        private TMPro.TMP_Text _textTmp;
        [SerializeField]
        private GameObject _gameOver;
        
        private static UIManager _instance;
        private int _score=0;
        
        public static UIManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<UIManager>();
                
                return _instance;
            }
        }

        public void IncrementScore()
        {
            _score++;
            _textTmp.text = $"{PREFIX} {_score}";
        }

        public void GameOver()
        {
            _gameOver.gameObject.SetActive(true);
        }
        
    }
}
