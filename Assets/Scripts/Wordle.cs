using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Wordle : MonoBehaviour
{
    private class WordsData
    {
        public string[] words;
    }

    [Header("Grid stuff")]
    public Transform grid;
    public GameObject squarePrefab;
    public float squareWidth;
    
    private GameObject[][] _squares;
    
    [Header("Word stuff")]
    public TextAsset wordsFile;
    public string answerWord;
    private List<string> _userWord = new List<string>();
    
    private List<Image> _clickedImages = new List<Image>();
    private HashSet<Image> _grayImages = new HashSet<Image>();
    private HashSet<Image> _correctImages = new HashSet<Image>();
    private HashSet<Image> _wrongPlaceImages = new HashSet<Image>();

    [Header("Other")]
    public GameObject keyboard;
    public GameObject startBtn;
    public TextMeshProUGUI infoText;

    private int _currentYIndex;
    private HashSet<int> _closedAnswerIndexes = new HashSet<int>();
    private HashSet<int> _correctIndexes = new HashSet<int>();
    private HashSet<int> _wrongPlaceIndexes = new HashSet<int>();



    public void GameSetup()
    {
        ResetImages();
        _correctImages.Clear();
        _wrongPlaceImages.Clear();
        _grayImages.Clear();

        _currentYIndex = 0;
        
        _userWord.Clear();
        _clickedImages.Clear();
        
        DestroyBoard();
        CreateBoard();
        GetRandomWord();

        keyboard.SetActive(true);
        startBtn.SetActive(false);
        infoText.text = "";
    }
    


    private void DestroyBoard()
    {
        if (_squares == null) return;
        
        for (int i = 0; i < _squares.Length; i++)
        {
            for (int j = 0; j < _squares[i].Length; j++)
            {
                Destroy(_squares[i][j]);
            }
        }
    }



    private void CreateBoard()
    {
        float x = -squareWidth * 2f;
        float y = squareWidth * 2f;

        _squares = new GameObject[5][];
        
        for (int i = 0; i < _squares.Length; i++)
        {
            _squares[i] = new GameObject[5];
            
            for (int j = 0; j < _squares[i].Length; j++)
            {
                GameObject tempSquare = Instantiate(squarePrefab, new Vector2(0, 0), Quaternion.identity, grid);
                tempSquare.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
                _squares[i][j] = tempSquare;

                x += squareWidth;
            }

            x = -squareWidth * 2f;
            y -= squareWidth;
        }
    }

    

    private void GetRandomWord()
    {
        WordsData wordsClass = JsonUtility.FromJson<WordsData>(wordsFile.text);
        answerWord = wordsClass.words[Random.Range(0, wordsClass.words.Length)];
    }



    public void OnKeyboardClick(TextMeshProUGUI selfText)
    {
        if (_userWord.Count >= 5) return;
        
        _userWord.Add(selfText.text);
        _clickedImages.Add(EventSystem.current.currentSelectedGameObject.GetComponent<Image>());
        UpdateGridWord();
    }


    
    public void OnEnterClick()
    {
        if (_userWord.Count < 5) return;
        
        CompareWords();
        
        if (_correctIndexes.Count == 5)
        {
            EndGame(true);
            return;
        }
        
        _currentYIndex++;
        if (_currentYIndex > 4)
        {
            EndGame(false);
            return;
        }

        _userWord.Clear();
        _clickedImages.Clear();
    }



    public void OnBackspaceClick()
    {
        if (_userWord.Count == 0) return;

        _userWord.RemoveAt(_userWord.Count - 1);
        _clickedImages.RemoveAt(_clickedImages.Count - 1);
        UpdateGridWord();
    }
    


    private void CompareWords()
    {
        _closedAnswerIndexes.Clear();
        _correctIndexes.Clear();
        _wrongPlaceIndexes.Clear();

        GetCorrectIndexes();
        GetWrongPlaceIndexes();
        SetSquareColors();
    }



    private void GetCorrectIndexes()
    {
        for (int i = 0; i < _userWord.Count; i++)
        {
            if (string.Equals(answerWord[i].ToString().ToUpper(), _userWord[i].ToUpper()))
            {
                _closedAnswerIndexes.Add(i);
                _correctIndexes.Add(i);
            }
        }
    }



    private void GetWrongPlaceIndexes()
    {
        for (int i = 0; i < _userWord.Count; i++)
        {
            if(_correctIndexes.Contains(i)) continue;

            for (int j = 0; j < answerWord.Length; j++)
            {
                if(_closedAnswerIndexes.Contains(j)) continue;
                
                if (string.Equals(answerWord[j].ToString().ToUpper(), _userWord[i].ToUpper()))
                {
                    _closedAnswerIndexes.Add(j);
                    _wrongPlaceIndexes.Add(i);
                    break;
                }
            }
        }
    }



    private void SetSquareColors()
    {
        foreach (var correctIndex in _correctIndexes)
        {
            _squares[_currentYIndex][correctIndex].GetComponent<Image>().color = Color.green;

            if (_correctImages.Contains(_clickedImages[correctIndex])) continue;
            _clickedImages[correctIndex].color = Color.green;
            _correctImages.Add(_clickedImages[correctIndex]);
        }
        
        foreach (var wrongPlaceIndex in _wrongPlaceIndexes)
        {
            _squares[_currentYIndex][wrongPlaceIndex].GetComponent<Image>().color = Color.yellow;
            
            if (_correctImages.Contains(_clickedImages[wrongPlaceIndex])) continue;
            if (_wrongPlaceImages.Contains(_clickedImages[wrongPlaceIndex])) continue;
            _clickedImages[wrongPlaceIndex].color = Color.yellow;
            _wrongPlaceImages.Add(_clickedImages[wrongPlaceIndex]);
        }

        for (int i = 0; i < answerWord.Length; i++)
        {
            if (_correctIndexes.Contains(i)) continue;
            if (_wrongPlaceIndexes.Contains(i)) continue;
            
            _squares[_currentYIndex][i].GetComponent<Image>().color = Color.gray;
            
            if (_correctImages.Contains(_clickedImages[i])) continue;
            if (_wrongPlaceImages.Contains(_clickedImages[i])) continue;
            if (_grayImages.Contains(_clickedImages[i])) continue;
            _clickedImages[i].color = Color.gray;
            _grayImages.Add(_clickedImages[i]);
        }
    }



    private void ResetImages()
    {
        foreach (Image correctImage in _correctImages)
        {
            correctImage.color = Color.white;
        }
        
        foreach (Image wrongPlaceImage in _wrongPlaceImages)
        {
            wrongPlaceImage.color = Color.white;
        }
        
        foreach (Image grayImage in _grayImages)
        {
            grayImage.color = Color.white;
        }
    }



    private void UpdateGridWord()
    {
        for (int i = 0; i < _squares[_currentYIndex].Length; i++)
        {
            _squares[_currentYIndex][i].GetComponentInChildren<TextMeshProUGUI>().text = "";
        }
        
        for (int i = 0; i < _squares[_currentYIndex].Length; i++)
        {
            if (_userWord.Count > i)
            {
                _squares[_currentYIndex][i].GetComponentInChildren<TextMeshProUGUI>().text = _userWord[i];
            }
            else
            {
                return;
            }
        }
    }



    private void EndGame(bool won)
    {
        keyboard.SetActive(false);
        startBtn.SetActive(true);

        if (won)
        {
            infoText.text = "You won! ";
        }
        else
        {
            infoText.text = "You lost! ";
        }

        infoText.text += "The correct word was " + answerWord;
    }



}
