package battleship.battleship;

import javafx.fxml.FXML;
import javafx.scene.control.Button;
import javafx.scene.control.Label;
import javafx.scene.layout.GridPane;

/**
 * This class controls the game interface and handles user interactions
 */
public class BattleShipController {
    @FXML
    private GridPane player1Board;

    @FXML
    private GridPane player2Board;

    @FXML
    private Label gameStatus;

    @FXML
    private String defaultStyle;

    @FXML
    private String hitStyle;

    @FXML
    private String missStyle;

    @FXML
    private String shipStyle;

    private Game game;
    private static final int BOARD_SIZE = 10;
    private Button[][] player1Buttons;
    private Button[][] player2Buttons;
    private int selectedShipLength;
    private boolean isHorizontal;
    private boolean isPlacingShips;

    /**
     * Initializes the game board when the application starts
     */
    @FXML
    public void initialize() {
        isPlacingShips = true;
        isHorizontal = true;
        setupBoards();
        gameStatus.setText("Select a ship to place on your board");
    }

    /**
     * Creates and sets up both player boards
     */
    private void setupBoards() {
        player1Buttons = new Button[BOARD_SIZE][BOARD_SIZE];
        player2Buttons = new Button[BOARD_SIZE][BOARD_SIZE];

        for (int i = 0; i < BOARD_SIZE; i++) {
            for (int j = 0; j < BOARD_SIZE; j++) {
                player1Buttons[i][j] = createButton(i, j, true);
                player1Board.add(player1Buttons[i][j], j, i);

                player2Buttons[i][j] = createButton(i, j, false);
                player2Board.add(player2Buttons[i][j], j, i);
            }
        }
    }

    private Button createButton(int row, int col, boolean isPlayer1) {
        Button button = new Button();
        button.setStyle(defaultStyle);

        if (isPlayer1 == true) {
            button.setOnAction(event -> handleShipPlacement(row, col));
        } else {
            button.setOnAction(event -> makeMove(row, col));
        }

        return button;
    }

    private void handleShipPlacement(int row, int col) {
        if (isPlacingShips == true && selectedShipLength > 0) {
            if (isValidPlacement(row, col) == true) {
                boolean placed = game.placeShip(row, col, selectedShipLength, isHorizontal, game.getCurrentPlayer());

                if (placed == true) {
                    updateShipDisplay(row, col);
                    selectedShipLength = 0;
                    gameStatus.setText("Ship placed successfully!");
                } else {
                    gameStatus.setText("Invalid placement. Try again.");
                }
            } else {
                gameStatus.setText("Ship would be out of bounds. Try again.");
            }
        }
    }

    private boolean isValidPlacement(int row, int col) {
        if (isHorizontal == true) {
            return col + selectedShipLength <= BOARD_SIZE;
        } else {
            return row + selectedShipLength <= BOARD_SIZE;
        }
    }

    private void updateShipDisplay(int row, int col) {
        if (isHorizontal == true) {
            for (int i = 0; i < selectedShipLength; i++) {
                player1Buttons[row][col + i].setStyle(shipStyle);
            }
        } else {
            for (int i = 0; i < selectedShipLength; i++) {
                player1Buttons[row + i][col].setStyle(shipStyle);
            }
        }
    }

    private void makeMove(int row, int col) {
        if (game.isGameInProgress() == true) {
            boolean isHit = game.makeMove(row, col);

            if (isHit == true) {
                player2Buttons[row][col].setStyle(hitStyle);
                gameStatus.setText("Hit!");
            } else {
                player2Buttons[row][col].setStyle(missStyle);
                gameStatus.setText("Miss!");
            }

            if (game.isGameOver() == true) {
                gameStatus.setText("Game Over! " + game.getWinner() + " wins!");
            }
        }
    }

    @FXML
    private void handleNewGame() {
        game = new Game();
        resetBoards();
        isPlacingShips = true;
        isHorizontal = true;
        gameStatus.setText("Select a ship to place on your board");
    }

    private void resetBoards() {
        for (int i = 0; i < BOARD_SIZE; i++) {
            for (int j = 0; j < BOARD_SIZE; j++) {
                player1Buttons[i][j].setStyle(defaultStyle);
                player2Buttons[i][j].setStyle(defaultStyle);
            }
        }
    }

    @FXML
    private void selectCarrier() {
        selectedShipLength = 5;
        gameStatus.setText("Place your Carrier (length: 5)");
    }

    @FXML
    private void selectBattleship() {
        selectedShipLength = 4;
        gameStatus.setText("Place your Battleship (length: 4)");
    }

    @FXML
    private void selectCruiser() {
        selectedShipLength = 3;
        gameStatus.setText("Place your Cruiser (length: 3)");
    }

    @FXML
    private void selectSubmarine() {
        selectedShipLength = 3;
        gameStatus.setText("Place your Submarine (length: 3)");
    }

    @FXML
    private void selectDestroyer() {
        selectedShipLength = 2;
        gameStatus.setText("Place your Destroyer (length: 2)");
    }

    @FXML
    private void rotateShip() {
        isHorizontal = !isHorizontal;
        String orientation = isHorizontal ? "horizontal" : "vertical";
        gameStatus.setText("Ship orientation changed to " + orientation);
    }

    @FXML
    private void startGame() {
        if (game.isPlacingShips() == false) {
            isPlacingShips = false;
            placeComputerShips();
            game.setGameInProgress(true);
            gameStatus.setText("Game started! Make your move.");
        } else {
            gameStatus.setText("Please place all your ships first!");
        }
    }

    private void placeComputerShips() {
        int[] shipLengths = {5, 4, 3, 3, 2};

        for (int length : shipLengths) {
            boolean placed = false;
            while (placed == false) {
                int row = (int)(Math.random() * BOARD_SIZE);
                int col = (int)(Math.random() * BOARD_SIZE);
                boolean horizontal = Math.random() < 0.5;

                placed = game.placeShip(row, col, length, horizontal, game.getPlayer2());
            }
        }
    }

    public void setGame(Game game) {
        this.game = game;
    }
}