using System;
using UnityEngine;

namespace Board
{
    public class BoardController : MonoBehaviour
    {
        private Dot _selectedDot;
        
        private Dot[,] _dots;
        
        private Square[,] _board;
        
        private void Start()
        {
            _board = new Square[4,4]; 
            var squares = GetComponentsInChildren<Square>();
            for (int i = 0; i < _board.GetLength(0); i++)
            {
                for (int j = 0; j < _board.GetLength(1); j++)
                {
                    _board[i, j] = squares[i * _board.GetLength(1) + j];
                }
            }
          
            _selectedDot = FindObjectOfType<Dot>();
        }


        private void Update()
        {
            
        }
        
        private void CalculateAllClosestSquares()
        {
           
        }
    }
}