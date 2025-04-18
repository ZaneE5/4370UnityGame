# 4370UnityGame

Credit to:

https://assetstore.unity.com/packages/templates/connect-four-starter-kit-19722 for the Connect 4 asset that I used as a base for the minigame

https://assetstore.unity.com/packages/3d/environments/low-poly-fps-map-lite-258453 for the map asset that I used to make the maze

https://roboticsproject.readthedocs.io/en/latest/ConnectFourAlgorithm.html for connect-4 specialized minimax helper functions minimax winning_move, evaluate_window, isTerminal,
and the scoring of the minimax methods in GameController.cs


Even with the connect 4 asset and the helpful minimax methods, getting both sets of code to work with each other was a pain and it might have even been easier to write it myself. Currently, the higher difficulties of the connect 4 AI run pretty slowly, because of how it has to look through every possible slice of the board for possible sequences.