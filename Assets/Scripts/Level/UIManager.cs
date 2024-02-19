using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace Level
{
    public class UIManager : MonoBehaviour
    {
        private const string PREFIX = "Hero <color=white>Score:</color>";
        private const string HERO_KILL_PREFIX = "Heros <color=white>Killed</color>:";
        private const string PRISONER_SAVED_PREFIX = "Prisoners <color=white>Saved</color>:";

        [FormerlySerializedAs("textTmp")] [SerializeField]
        private TMPro.TMP_Text _textTmp;
        [FormerlySerializedAs("_gameOver")] [SerializeField]
        private GameObject _gameOverLose;
        [SerializeField]
        private GameObject _gameOverWin;
        
        [SerializeField] private TMPro.TMP_Text _timerTextTmp;
        [SerializeField] private TMPro.TMP_Text _herosKilledTextTmp;
        [SerializeField] private TMPro.TMP_Text _prisonerSavedTextTmp;
        
        private static UIManager _instance;
        private int _score=0;
        private int _herosKilled =0;
        private int _prisonerSaved = 0;
        
        private long _seconds=0;
        private Coroutine _timerRoutine;
        public static UIManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<UIManager>();
                
                return _instance;
            }
        }

        private void Start()
        {
            _timerRoutine = StartCoroutine(Timer());
        }

        public void IncrementScore(int score=1)
        {
            _score += score;
            if (_score < 0)
                _score = 0;
            
            _textTmp.text = $"{PREFIX} {_score}";
        }

        public void PrisonerSaved()
        {
            _prisonerSaved += 1;
            _prisonerSavedTextTmp.text = $"{PRISONER_SAVED_PREFIX} {_prisonerSaved}";
        }
        
        public void HeroKilled()
        {
            _herosKilled += 1;
            _herosKilledTextTmp.text = $"{HERO_KILL_PREFIX} {_herosKilled}";
        }

        public void GameOver(bool win)
        {
            _gameOverWin.SetActive(win);
            _gameOverLose.SetActive(!win);
            StopCoroutine(_timerRoutine);
        }

        private IEnumerator Timer()
        {
            while (true)
            {
                _seconds += 1;
                var dt = DateTimeOffset.FromUnixTimeSeconds(_seconds);
                _timerTextTmp.text = $"{dt.Minute:d2}:{dt.Second:D2}";
                yield return new WaitForSeconds(1);
            }
        }
    }
}
