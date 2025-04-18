using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Linq.Expressions;
using UnityEngine.UIElements;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Diagnostics.Tracing;
using System;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;
using JetBrains.Annotations;
using UnityEngine.Scripting.APIUpdating;
using System.Linq;
using UnityEngine.SocialPlatforms.Impl;
using System.Data;
using UnityEditor.PackageManager.UI;

/*
Minimax supporting methods winning_move, evaluate_window, isTerminal,
and the scoring of the minimax methods from: 
https://roboticsproject.readthedocs.io/en/latest/ConnectFourAlgorithm.html

Non-minimax code for free from https://assetstore.unity.com/packages/templates/connect-four-starter-kit-19722 
by Unity user Eikester
*/

namespace ConnectFour
{
	public class GameController : MonoBehaviour
	{
		enum Piece
		{
			Empty = 0,
			Yellow = 1,
			Red = 2
		}

		[Range(3, 8)]
		public int numRows = 6;
		[Range(3, 8)]
		public int numColumns = 7;

		[Tooltip("How many pieces have to be connected to win.")]
		public int numPiecesToWin = 4;

		[Tooltip("Allow diagonally connected Pieces?")]
		public bool allowDiagonally = true;

		public float dropTime = 4f;

		// Gameobjects 
		public GameObject pieceRed;
		public GameObject pieceYellow;
		public GameObject pieceField;

		public GameObject winningText;
		public string playerWonText = "You Won!";
		public string playerLoseText = "You Lose!";
		public string drawText = "Draw!";

		public GameObject btnPlayAgain;
		public GameObject btnExit;
		public GameObject difficultyText;
		public GameObject easyBtn;
		public GameObject medBtn;
		public GameObject hardBtn;

		bool btnPlayAgainTouching = false;
		Color btnPlayAgainOrigColor;
		Color btnPlayAgainHoverColor = new Color(255, 143, 4);

		GameObject gameObjectField;

		// temporary gameobject, holds the piece at mouse position until the mouse has clicked
		GameObject gameObjectTurn;

		/// <summary>
		/// The Game field.
		/// 0 = Empty
		/// 1 = Yellow
		/// 2 = Red
		/// </summary>
		int[,] field;

		bool isPlayersTurn = true;
		bool isLoading = true;
		bool isDropping = false;
		bool mouseButtonPressed = false;

		bool gameOver = false;
		bool isCheckingForWinner = false;
		int difficulty = 0;

		// Use this for initialization
		void Start()
		{
			int max = Mathf.Max(numRows, numColumns);

			if (numPiecesToWin > max)
				numPiecesToWin = max;

			CreateField();

			isPlayersTurn = System.Convert.ToBoolean(Random.Range(0, 1));

			btnPlayAgainOrigColor = btnPlayAgain.GetComponent<Renderer>().material.color;
		}

		void SelectDifficulty()
		{
			difficultyText.SetActive(true);
			easyBtn.SetActive(true);
			medBtn.SetActive(true);
			hardBtn.SetActive(true);

			RaycastHit hit;
			//ray shooting out of the camera from where the mouse is
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			if (Physics.Raycast(ray, out hit) && hit.collider.name == easyBtn.name)
			{
				easyBtn.GetComponent<Renderer>().material.color = btnPlayAgainHoverColor;
				//check if the left mouse has been pressed down this frame
				if (Input.GetMouseButtonDown(0) || Input.touchCount > 0 && btnPlayAgainTouching == false)
				{
					difficulty = 3;
				}
			}
			else
			{
				easyBtn.GetComponent<Renderer>().material.color = btnPlayAgainOrigColor;
			}

			if (Physics.Raycast(ray, out hit) && hit.collider.name == medBtn.name)
			{
				medBtn.GetComponent<Renderer>().material.color = btnPlayAgainHoverColor;
				//check if the left mouse has been pressed down this frame
				if (Input.GetMouseButtonDown(0) || Input.touchCount > 0 && btnPlayAgainTouching == false)
				{
					difficulty = 4;
				}
			}
			else
			{
				medBtn.GetComponent<Renderer>().material.color = btnPlayAgainOrigColor;
			}

			if (Physics.Raycast(ray, out hit) && hit.collider.name == hardBtn.name)
			{
				hardBtn.GetComponent<Renderer>().material.color = btnPlayAgainHoverColor;
				//check if the left mouse has been pressed down this frame
				if (Input.GetMouseButtonDown(0) || Input.touchCount > 0 && btnPlayAgainTouching == false)
				{
					difficulty = 6;
				}
			}
			else
			{
				hardBtn.GetComponent<Renderer>().material.color = btnPlayAgainOrigColor;
			}

			if (difficulty != 0)
			{
				Debug.Log("Difficulty chosen: " + difficulty);
				difficultyText.SetActive(false);
				easyBtn.SetActive(false);
				medBtn.SetActive(false);
				hardBtn.SetActive(false);
			}
		}

		/// <summary>
		/// Creates the field.
		/// </summary>
		void CreateField()
		{
			winningText.SetActive(false);
			btnPlayAgain.SetActive(false);
			btnExit.SetActive(false);

			isLoading = true;

			SelectDifficulty();

			gameObjectField = GameObject.Find("Field");
			if (gameObjectField != null)
			{
				DestroyImmediate(gameObjectField);
			}
			gameObjectField = new GameObject("Field");

			// create an empty field and instantiate the cells
			field = new int[numColumns, numRows];
			for (int x = 0; x < numColumns; x++)
			{
				for (int y = 0; y < numRows; y++)
				{
					field[x, y] = (int)Piece.Empty;
					GameObject g = Instantiate(pieceField, new Vector3(x, y * -1, -1), Quaternion.identity) as GameObject;
					g.transform.parent = gameObjectField.transform;
				}
			}

			isLoading = false;
			gameOver = false;

			// center camera
			Camera.main.transform.position = new Vector3(
				(numColumns - 1) / 2.0f, -((numRows - 1) / 2.0f), Camera.main.transform.position.z);

			winningText.transform.position = new Vector3(
				(numColumns - 1) / 2.0f, -((numRows - 1) / 2.0f) + 1, winningText.transform.position.z);

			btnPlayAgain.transform.position = new Vector3(
				(numColumns - 1) / 2.0f, -((numRows - 1) / 2.0f) - 1, btnPlayAgain.transform.position.z);
		}

		/// <summary>
		/// Spawns a piece at mouse position above the first row
		/// </summary>
		/// <returns>The piece.</returns>
		GameObject SpawnPiece()
		{
			Vector3 spawnPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

			if (!isPlayersTurn)
			{
				List<int> moves = GetPossibleMoves(field);

				if (moves.Count > 0)
				{
					//minimax goes here
					(int score, int column) = Minimax(field, int.MinValue, int.MaxValue, 0, false);//moves[Random.Range (0, moves.Count)];
					Debug.Log("Minimax best score: " + score);
					Debug.Log("Minimax best column: " + column);
					spawnPos = new Vector3(column, 0, 0);
				}
			}

			GameObject g = Instantiate(
					isPlayersTurn ? pieceYellow : pieceRed, // is players turn = spawn Yellow, else spawn red
					new Vector3(
					Mathf.Clamp(spawnPos.x, 0, numColumns - 1),
					gameObjectField.transform.position.y + 1, 0), // spawn it above the first row
					Quaternion.identity) as GameObject;

			return g;
		}

		void UpdatePlayAgainButton()
		{
			RaycastHit hit;
			//ray shooting out of the camera from where the mouse is
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			if (Physics.Raycast(ray, out hit) && hit.collider.name == btnPlayAgain.name)
			{
				btnPlayAgain.GetComponent<Renderer>().material.color = btnPlayAgainHoverColor;
				//check if the left mouse has been pressed down this frame
				if (Input.GetMouseButtonDown(0) || Input.touchCount > 0 && btnPlayAgainTouching == false)
				{
					btnPlayAgainTouching = true;

					//CreateField();
					SceneManager.LoadScene("Connect4");
				}
			}
			else
			{
				btnPlayAgain.GetComponent<Renderer>().material.color = btnPlayAgainOrigColor;
			}

			if (Physics.Raycast(ray, out hit) && hit.collider.name == btnExit.name)
			{
				btnExit.GetComponent<Renderer>().material.color = btnPlayAgainHoverColor;
				//check if the left mouse has been pressed down this frame
				if (Input.GetMouseButtonDown(0) || Input.touchCount > 0 && btnPlayAgainTouching == false)
				{
					SceneManager.LoadScene("MazeScene");
				}
			}
			else
			{
				btnExit.GetComponent<Renderer>().material.color = btnPlayAgainOrigColor;
			}

			if (Input.touchCount == 0)
			{
				btnPlayAgainTouching = false;
			}
		}

		// Update is called once per frame
		void Update()
		{
			if (difficulty == 0)
			{
				SelectDifficulty();
			}
			else
			{

				if (isLoading)
					return;

				if (isCheckingForWinner)
					return;

				if (gameOver)
				{
					winningText.SetActive(true);
					btnPlayAgain.SetActive(true);
					btnExit.SetActive(true);

					UpdatePlayAgainButton();

					return;
				}

				if (isPlayersTurn)
				{
					if (gameObjectTurn == null)
					{
						gameObjectTurn = SpawnPiece();
					}
					else
					{
						// update the objects position
						Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
						gameObjectTurn.transform.position = new Vector3(
							Mathf.Clamp(pos.x, 0, numColumns - 1),
							gameObjectField.transform.position.y + 1, 0);

						// click the left mouse button to drop the piece into the selected column
						if (Input.GetMouseButtonDown(0) && !mouseButtonPressed && !isDropping)
						{
							mouseButtonPressed = true;

							StartCoroutine(dropPiece(gameObjectTurn));
						}
						else
						{
							mouseButtonPressed = false;
						}
					}
				}
				else
				{
					if (gameObjectTurn == null)
					{
						gameObjectTurn = SpawnPiece();
					}
					else
					{
						if (!isDropping)
							StartCoroutine(dropPiece(gameObjectTurn));
					}
				}
			}
		}

		/// <summary>
		/// Gets all the possible moves.
		/// </summary>
		/// <returns>The possible moves.</returns>
		public List<int> GetPossibleMoves(int[,] board)
		{
			List<int> possibleMoves = new List<int>();
			for (int c = 0; c < numColumns; c++)
			{
				for (int r = numRows - 1; r >= 0; r--)
				{
					if (board[c, r] == (int)Piece.Empty)
					{
						possibleMoves.Add(c);
						break;
					}
				}
			}
			return possibleMoves;
		}

		/// <summary>
		/// This method searches for a empty cell and lets 
		/// the object fall down into this cell
		/// </summary>
		/// <param name="gObject">Game Object.</param>
		IEnumerator dropPiece(GameObject gObject)
		{
			isDropping = true;

			Vector3 startPosition = gObject.transform.position;
			Vector3 endPosition = new Vector3();

			// round to a grid cell
			int x = Mathf.RoundToInt(startPosition.x);
			startPosition = new Vector3(x, startPosition.y, startPosition.z);

			// is there a free cell in the selected column?
			bool foundFreeCell = false;
			for (int i = numRows - 1; i >= 0; i--)
			{
				if (field[x, i] == 0)
				{
					foundFreeCell = true;
					field[x, i] = isPlayersTurn ? (int)Piece.Yellow : (int)Piece.Red;
					endPosition = new Vector3(x, i * -1, startPosition.z);

					break;
				}
			}

			if (foundFreeCell)
			{
				// Instantiate a new Piece, disable the temporary
				GameObject g = Instantiate(gObject) as GameObject;
				gameObjectTurn.GetComponent<Renderer>().enabled = false;

				float distance = Vector3.Distance(startPosition, endPosition);

				float t = 0;
				while (t < 1)
				{
					t += Time.deltaTime * dropTime * ((numRows - distance) + 1);

					g.transform.position = Vector3.Lerp(startPosition, endPosition, t);
					yield return null;
				}

				g.transform.parent = gameObjectField.transform;

				// remove the temporary gameobject
				DestroyImmediate(gameObjectTurn);

				// run coroutine to check if someone has won
				StartCoroutine(Won());

				// wait until winning check is done
				while (isCheckingForWinner)
					yield return null;

				isPlayersTurn = !isPlayersTurn;
			}

			isDropping = false;

			yield return 0;
		}

		/// <summary>
		/// Check for Winner
		/// </summary>
		IEnumerator Won()
		{
			isCheckingForWinner = true;

			for (int x = 0; x < numColumns; x++)
			{
				for (int y = 0; y < numRows; y++)
				{
					// Get the Laymask to Raycast against, if its Players turn only include
					// Layermask Yellow otherwise Layermask Red
					int layermask = isPlayersTurn ? (1 << 8) : (1 << 9);

					// If its Players turn ignore red as Starting piece and wise versa
					if (field[x, y] != (isPlayersTurn ? (int)Piece.Yellow : (int)Piece.Red))
					{
						continue;
					}

					// shoot a ray of length 'numPiecesToWin - 1' to the right to test horizontally
					RaycastHit[] hitsHorz = Physics.RaycastAll(
						new Vector3(x, y * -1, 0),
						Vector3.right,
						numPiecesToWin - 1,
						layermask);

					// return true (won) if enough hits
					if (hitsHorz.Length == numPiecesToWin - 1)
					{
						gameOver = true;
						break;
					}

					// shoot a ray up to test vertically
					RaycastHit[] hitsVert = Physics.RaycastAll(
						new Vector3(x, y * -1, 0),
						Vector3.up,
						numPiecesToWin - 1,
						layermask);

					if (hitsVert.Length == numPiecesToWin - 1)
					{
						gameOver = true;
						break;
					}

					// test diagonally
					if (allowDiagonally)
					{
						// calculate the length of the ray to shoot diagonally
						float length = Vector2.Distance(new Vector2(0, 0), new Vector2(numPiecesToWin - 1, numPiecesToWin - 1));

						RaycastHit[] hitsDiaLeft = Physics.RaycastAll(
							new Vector3(x, y * -1, 0),
							new Vector3(-1, 1),
							length,
							layermask);

						if (hitsDiaLeft.Length == numPiecesToWin - 1)
						{
							gameOver = true;
							break;
						}

						RaycastHit[] hitsDiaRight = Physics.RaycastAll(
							new Vector3(x, y * -1, 0),
							new Vector3(1, 1),
							length,
							layermask);

						if (hitsDiaRight.Length == numPiecesToWin - 1)
						{
							gameOver = true;
							break;
						}
					}

					yield return null;
				}

				yield return null;
			}

			// if Game Over update the winning text to show who has won
			if (gameOver == true)
			{
				winningText.GetComponent<TextMesh>().text = isPlayersTurn ? playerWonText : playerLoseText;
			}
			else
			{
				// check if there are any empty cells left, if not set game over and update text to show a draw
				if (!FieldContainsEmptyCell())
				{
					gameOver = true;
					winningText.GetComponent<TextMesh>().text = drawText;
				}
			}

			isCheckingForWinner = false;

			yield return 0;
		}

		/// <summary>
		/// check if the field contains an empty cell
		/// </summary>
		/// <returns><c>true</c>, if it contains empty cell, <c>false</c> otherwise.</returns>
		bool FieldContainsEmptyCell()
		{
			for (int x = 0; x < numColumns; x++)
			{
				for (int y = 0; y < numRows; y++)
				{
					if (field[x, y] == (int)Piece.Empty)
						return true;
				}
			}
			return false;
		}

		//returns true if the given piece type has won the given board
		bool winning_move(int[,] board, int pieceType)
		{
			//check vertical positions for four in a row
			for (int c = 0; c < numColumns; c++)
			{
				for (int r = 0; r < numRows - 3; r++)
				{
					if (board[c, r] == pieceType && board[c, r + 1] == pieceType && board[c, r + 2] == pieceType && board[c, r + 3] == pieceType)
					{
						return true;
					}
				}
			}
			//check horizontal positions for four in a row
			for (int c = 0; c < numColumns - 3; c++)
			{
				for (int r = 0; r < numRows; r++)
				{
					if (board[c, r] == pieceType && board[c + 1, r] == pieceType && board[c + 2, r] == pieceType && board[c + 3, r] == pieceType)
					{
						return true;
					}
				}
			}
			//check \ diagonals
			for (int c = 0; c < numColumns - 3; c++)
			{
				for (int r = 0; r < numRows - 3; r++)
				{
					if (board[c, r] == pieceType && board[c + 1, r + 1] == pieceType && board[c + 2, r + 2] == pieceType && board[c + 3, r + 3] == pieceType)
					{
						return true;
					}
				}
			}
			//check / diagonals
			for (int c = 0; c < numColumns - 3; c++)
			{
				for (int r = 3; r < numRows; r++)
				{
					if (board[c, r] == pieceType && board[c + 1, r - 1] == pieceType && board[c + 2, r - 2] == pieceType && board[c + 3, r - 3] == pieceType)
					{
						return true;
					}
				}
			}
			return false;
		}

		//Returns true if either player has won or if there are no moves to make
		bool isTerminal(int[,] board)
		{
			if (!FieldContainsEmptyCell() || winning_move(board, 1) || winning_move(board, 2))
			{
				return true;
			}

			return false;
		}

		//returns a score for a slice of 4
		int evaluate_window(List<int> slice, int pieceType) {
			int score = 0;

			int opponentPiece = (pieceType == 1) ? 2 : 1;

			if (slice.FindAll(x => x == pieceType).Count == 4) {
				score += 200;
			}
			else if (slice.FindAll(x => x == pieceType).Count == 3 && slice.FindAll(x => x == 0).Count == 1) {
				score += 10;
			}
			else if (slice.FindAll(x => x == pieceType).Count == 2 && slice.FindAll(x => x == 0).Count == 2) {
				score += 2;
			}

			if (slice.FindAll(x => x == opponentPiece).Count == 3 && slice.FindAll(x => x == 0).Count == 1) {
				score -= 5;
			}

			return score;
		}

		//iterate though all slices of 4 and add up scores
		int scorePosition(int[,] board, int pieceType) {
			int score = 0;

			//weight center column heavier
			int centerColIndex = board.GetLength(1) / 2;
			int centerCount = 0;
			for (int row = 0; row < numRows; row++) {
				if (board[row, centerColIndex] == pieceType) {
					centerCount++;
				}
			}
			score += centerCount * 3;

			//score horizontal positions
			for (int r = 0; r < numRows; r++) {
				for (int sliceStartCol = 0; sliceStartCol < numColumns - 3; sliceStartCol++) {
					List<int> row_slice = new List<int>
                    {
                        board[sliceStartCol, r],
                        board[sliceStartCol + 1, r],
                        board[sliceStartCol + 2, r],
                        board[sliceStartCol + 3, r]
                    };
					score += evaluate_window(row_slice, pieceType);
				}
			}

			//score vertical positions
			for (int c = 0; c < numColumns; c++) {
				for (int sliceStartRow = 0; sliceStartRow < numRows - 3; sliceStartRow++) {
					List<int> col_slice = new List<int>
                    {
                        board[c, sliceStartRow],
                        board[c, sliceStartRow + 1],
                        board[c, sliceStartRow + 2],
                        board[c, sliceStartRow + 3]
                    };
					score += evaluate_window(col_slice, pieceType);
					
				}
			}

			//score \ diagonals
			for (int r = 0; r < numRows - 3; r++) {
				for (int c = 0; c < numColumns - 3; c++) {
					List<int> diagonal_slice = new List<int>
                    {
                        board[c, r],
                        board[c + 1, r + 1],
                        board[c + 2, r + 2],
                        board[c + 3, r + 3]
                    };
					score += evaluate_window(diagonal_slice, pieceType);
				}
			}

			//score / diagonals
			for (int r = 3; r < numRows; r++) {
				for (int c = 0; c < numColumns - 3; c++) {
					List<int> diagonal_slice = new List<int>
					{
						board[c,r],
						board[c + 1, r - 1],
						board[c + 2, r - 2],
						board[c + 3, r - 3]
					};
					score += evaluate_window(diagonal_slice, pieceType);
				}
			}

			return score;
		}

		//modify a board with the given move in the given column
		void makeMove(int [,] board, int col, int pieceType) {
			bool moveMade = false;
			for (int row = numRows - 1; row >= 0; row--) {
				if (board[col,row] == 0 && !moveMade) {
					board[col,row] = pieceType;
					moveMade = true;
				}
			}
		}

		(int bestScore, int bestCol) Minimax(int[,] board, int alpha, int beta, int currentDepth, bool player)
		{
			int bestCol = -1;
			int bestScore;
			Debug.Log("Current Depth: " + currentDepth);

			if (isTerminal(board) || currentDepth == difficulty)
			{
				if (isTerminal(board))
				{
					if (winning_move(board, 2))
					{
						return (int.MaxValue, -1);
					}
					else if (winning_move(board, 1))
					{
						return (int.MinValue, -1);
					}
					else
					{
						return (0, -1);
					}
				} 
				else
				{
					return (scorePosition(board, 2), -1);
				}
			}

			if (!player)
			{
				bestScore = int.MinValue;
			}
			else
			{
				bestScore = int.MaxValue;
			}

			foreach (int col in GetPossibleMoves(board))
			{
				int[,] newBoard = (int[,])board.Clone();

				//make move in new board
				makeMove(newBoard, col, player ? 1 : 2);

				(int currScore, int currMove) = Minimax(newBoard, alpha, beta, currentDepth + 1, !player);
				if (!player)
				{
					if (currScore > bestScore)
					{
						bestScore = currScore;
						bestCol = col;
					}
					if (bestScore >= beta)
					{
						break;
					}
					alpha = (alpha > bestScore) ? alpha : bestScore;
				}
				else
				{
					if (currScore <= bestScore)
					{
						bestScore = currScore;
						bestCol = col;
					}
					if (bestScore <= alpha)
					{
						break;
					}
					beta = (beta < bestScore) ? beta : bestScore;
				}
			}

			return (bestScore, bestCol);
		}
	}
}
