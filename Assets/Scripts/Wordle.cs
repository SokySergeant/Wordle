using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class Wordle : MonoBehaviour
{
    public class WordsData
    {
        public List<string> words = new List<string>();
    }

    [Header("Grid stuff")]
    public Canvas canvas;
    public GameObject squarePrefab;
    public float squareWidth;
    
    private GameObject[][] _squares;
    
    [Header("Word stuff")]
    public TextAsset wordsFile;
    public string answerWord;
    

    private void Start()
    {
        BoardSetup();
        GetRandomWord();
    }
    

    
    private void BoardSetup()
    {
        float x = (Screen.width / 2f) - (squareWidth * 2f);
        float y = (Screen.height / 2f) + (squareWidth * 2f);

        _squares = new GameObject[5][];
        
        for (int i = 0; i < _squares.Length; i++)
        {
            _squares[i] = new GameObject[5];
            
            for (int j = 0; j < _squares[i].Length; j++)
            {
                GameObject tempSquare = Instantiate(squarePrefab, new Vector2(x, y), Quaternion.identity, canvas.transform);
                _squares[i][j] = tempSquare;

                x += squareWidth;
            }

            x = (Screen.width / 2f) - (squareWidth * 2f);
            y -= squareWidth;
        }
    }

    

    private void GetRandomWord()
    {
        WordsData wordsClass = JsonUtility.FromJson<WordsData>(wordsFile.text);
        answerWord = wordsClass.words[Random.Range(0, wordsClass.words.Count)];
    }



    private void UpdateGridWord(int yCoord, string word)
    {
        for (int i = 0; i < _squares[yCoord].Length; i++)
        {
            if (word.Length > i)
            {
                _squares[yCoord][i].GetComponentInChildren<TextMeshProUGUI>().text = word[i].ToString();
            }
            else
            {
                return;
            }
        }
    }
    
}
