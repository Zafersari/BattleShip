package battleship.battleship;

import javafx.fxml.FXML;
import javafx.scene.control.Button;
import javafx.scene.control.Label;
import javafx.scene.layout.GridPane;

/**
 * Controls the game interface and handles user interactions
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

    @FXML
    private Button carrierButton;

    @FXML
    private Button battleshipButton;

    @FXML
    private Button cruiserButton;

    @FXML
    private Button submarineButton;

    @FXML
    private Button destroyerButton;

    private Game game;
    private static final int BOARD_SIZE = 10;
    private Button[][] player1Buttons;
    private Button[][] player2Buttons;
    private int selectedShipLength;
    private boolean isHorizontal;
    private boolean isPlacingShips;
    private boolean[] shipsUsed = new boolean[5];
    private int shipsPlaced = 0;

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

    /**
     * Creates a button for the game board
     */
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

    /**
     * Handles ship placement when a board position is clicked
     */
    private void handleShipPlacement(int row, int col) {
        if (isPlacingShips && selectedShipLength > 0) {
            if (isValidPlacement(row, col)) {
                if (isOverlappingShips(row, col)) {
                    gameStatus.setText("Ships cannot overlap. Try another position.");
                    enableButtonForCurrentShip();
                    return;
                }
                boolean placed = game.placeShip(row, col, selectedShipLength, isHorizontal, game.getCurrentPlayer());

                if (placed) {
                    updateShipDisplay(row, col);
                    selectedShipLength = 0;
                    shipsPlaced++;

                    if (shipsPlaced == 5) {
                        isPlacingShips = false;
                        placeComputerShips();
                        game.setGameInProgress(true);
                        gameStatus.setText("All ships placed! Game started - Make your move.");
                    } else {
                        gameStatus.setText("Ship placed successfully! Select next ship.");
                    }
                } else {
                    gameStatus.setText("Invalid placement. Try again.");
                    enableButtonForCurrentShip();
                }
            } else {
                gameStatus.setText("Ship would be out of bounds. Try again.");
                enableButtonForCurrentShip();
            }
        }
    }

    /**
     * Re-enables button for current ship when placement fails
     */
    private void enableButtonForCurrentShip() {
        switch (selectedShipLength) {
            case 5:
                carrierButton.setDisable(false);
                shipsUsed[0] = false;
                break;
            case 4:
                battleshipButton.setDisable(false);
                shipsUsed[1] = false;
                break;
            case 3:
                if (shipsUsed[2] && !shipsUsed[3]) {
                    cruiserButton.setDisable(false);
                    shipsUsed[2] = false;
                } else {
                    submarineButton.setDisable(false);
                    shipsUsed[3] = false;
                }
                break;
            case 2:
                destroyerButton.setDisable(false);
                shipsUsed[4] = false;
                break;
        }
        selectedShipLength = 0;
    }

    /**
     * Checks if ship placement is within board boundaries
     */
    private boolean isValidPlacement(int row, int col) {
        if (isHorizontal == true) {
            return col + selectedShipLength <= BOARD_SIZE;
        } else {
            return row + selectedShipLength <= BOARD_SIZE;
        }
    }

    /**
     * Checks if ship placement would overlap with existing ships
     */
    private boolean isOverlappingShips(int row, int col) {
        if (isHorizontal) {
            for (int i = 0; i < selectedShipLength; i++) {
                if (col + i < BOARD_SIZE &&
                        player1Buttons[row][col + i].getStyle().equals(shipStyle)) {
                    return true;
                }
            }
        } else {
            for (int i = 0; i < selectedShipLength; i++) {
                if (row + i < BOARD_SIZE &&
                        player1Buttons[row + i][col].getStyle().equals(shipStyle)) {
                    return true;
                }
            }
        }
        return false;
    }

    /**
     * Updates the visual display of ships on the board
     */
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

    /**
     * Handles attack moves on opponent's board
     */
    private void makeMove(int row, int col) {
        if (game.isGameInProgress() && !player2Buttons[row][col].getStyle().equals(hitStyle) &&
                !player2Buttons[row][col].getStyle().equals(missStyle)) {

            boolean isHit = game.makeMove(row, col);

            if (isHit) {
                player2Buttons[row][col].setStyle(hitStyle);
                player2Buttons[row][col].setDisable(true);
                gameStatus.setText("Hit!");

                if (game.getPlayer2Board().areAllShipsSunk()) {
                    gameStatus.setText("Game Over! " + game.getWinner() + " wins!");
                    game.setGameInProgress(false);
                }
            } else {
                player2Buttons[row][col].setStyle(missStyle);
                player2Buttons[row][col].setDisable(true);
                gameStatus.setText("Miss!");
            }
        }
    }

    /**
     * Handles starting a new game
     */
    @FXML
    private void handleNewGame() {
        game = new Game();
        resetBoards();
        isPlacingShips = true;
        isHorizontal = true;
        gameStatus.setText("Select a ship to place on your board");
        enableAllShipButtons();
    }

    /**
     * Resets the game boards to initial state
     */
    private void resetBoards() {
        shipsPlaced = 0;
        shipsUsed = new boolean[5];
        for (int i = 0; i < BOARD_SIZE; i++) {
            for (int j = 0; j < BOARD_SIZE; j++) {
                player1Buttons[i][j].setStyle(defaultStyle);
                player2Buttons[i][j].setStyle(defaultStyle);
                player2Buttons[i][j].setDisable(false);
            }
        }
    }

    /**
     * Enables all ship selection buttons
     */
    private void enableAllShipButtons() {
        carrierButton.setDisable(false);
        battleshipButton.setDisable(false);
        cruiserButton.setDisable(false);
        submarineButton.setDisable(false);
        destroyerButton.setDisable(false);
    }

    /**
     * Ship selection methods
     */
    @FXML
    private void selectCarrier() {
        if (!shipsUsed[0]) {
            selectedShipLength = 5;
            shipsUsed[0] = true;
            carrierButton.setDisable(true);
            gameStatus.setText("Place your Carrier (length: 5)");
        }
    }

    @FXML
    private void selectBattleship() {
        if (!shipsUsed[1]) {
            selectedShipLength = 4;
            shipsUsed[1] = true;
            battleshipButton.setDisable(true);
            gameStatus.setText("Place your Battleship (length: 4)");
        }
    }

    @FXML
    private void selectCruiser() {
        if (!shipsUsed[2]) {
            selectedShipLength = 3;
            shipsUsed[2] = true;
            cruiserButton.setDisable(true);
            gameStatus.setText("Place your Cruiser (length: 3)");
        }
    }

    @FXML
    private void selectSubmarine() {
        if (!shipsUsed[3]) {
            selectedShipLength = 3;
            shipsUsed[3] = true;
            submarineButton.setDisable(true);
            gameStatus.setText("Place your Submarine (length: 3)");
        }
    }

    @FXML
    private void selectDestroyer() {
        if (!shipsUsed[4]) {
            selectedShipLength = 2;
            shipsUsed[4] = true;
            destroyerButton.setDisable(true);
            gameStatus.setText("Place your Destroyer (length: 2)");
        }
    }

    /**
     * Handles ship rotation
     */
    @FXML
    private void rotateShip() {
        isHorizontal = !isHorizontal;
        String orientation = isHorizontal ? "horizontal" : "vertical";
        gameStatus.setText("Ship orientation changed to " + orientation);
    }

    /**
     * Starts the game when all ships are placed
     */
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

    /**
     * Places ships for computer player
     */
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

    /**
     * Sets the game instance
     */
    public void setGame(Game game) {
        this.game = game;
    }

    /**
     * Gets player 2's board
     */
    private Board getPlayer2Board() {
        return game.getPlayer2Board();
    }
}