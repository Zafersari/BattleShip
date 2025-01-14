package battleship.battleship;

/**
 * This class represents the main game logic for Battleship game
 * It manages the game boards, players and game status
 */
public class Game {
    // Game boards for both players
    private Board player1Board;
    private Board player2Board;

    // Player information
    private Player currentPlayer;
    private Player player1;
    private Player player2;

    // Ship placement tracking
    private int[] shipLengths = {5, 4, 3, 3, 2};  // Carrier, Battleship, Cruiser, Submarine, Destroyer
    private int[] player1ShipsPlaced = {0, 0, 0, 0, 0};
    private int[] player2ShipsPlaced = {0, 0, 0, 0, 0};

    // Game status flags
    private boolean isGameInProgress;
    private boolean isPlacingShips;

    /**
     * Creates a new game and initializes the boards
     */
    public Game() {
        initializeGame();
    }

    /**
     * Initializes the game by creating new boards and setting initial states
     */
    public void initializeGame() {
        player1Board = new Board();
        player2Board = new Board();
        isGameInProgress = false;
        isPlacingShips = true;
    }

    /**
     * Sets the players for the game
     */
    public void setPlayers(Player player1, Player player2) {
        this.player1 = player1;
        this.player2 = player2;
        this.currentPlayer = player1;
    }

    /**
     * Makes a move in the game
     * Returns: true if the move hits a ship, false if it misses
     */
    public boolean makeMove(int row, int col) {
        if (isGameInProgress == false || isPlacingShips == true) {
            return false;
        }

        Board targetBoard = (currentPlayer == player1) ? player2Board : player1Board;

        if (targetBoard.isShot(row, col) == true) {
            return false;  // Position already shot at
        }

        boolean isHit = targetBoard.receiveAttack(row, col);

        if (targetBoard.areAllShipsSunk() == true) {
            isGameInProgress = false;
        } else {
            switchPlayer();
        }

        return isHit;
    }

    /**
     * Places a ship on the board
     * Returns: true if the ship was placed successfully
     */
    public boolean placeShip(int row, int col, int length, boolean isHorizontal, Player player) {
        if (isPlacingShips == false) {
            return false;
        }

        Board board = (player == player1) ? player1Board : player2Board;
        boolean placed = board.placeShip(row, col, length, isHorizontal);

        if (placed == true) {
            updateShipPlacementCount(length, player);
            checkIfAllShipsPlaced();
        }

        return placed;
    }

    /**
     * Updates the count of placed ships for each player
     */
    private void updateShipPlacementCount(int length, Player player) {
        int[] shipCount = (player == player1) ? player1ShipsPlaced : player2ShipsPlaced;
        for (int i = 0; i < shipLengths.length; i++) {
            if (shipLengths[i] == length && shipCount[i] == 0) {
                shipCount[i] = 1;
                break;
            }
        }
    }

    /**
     * Checks if all ships have been placed and starts the game if they have
     */
    private void checkIfAllShipsPlaced() {
        boolean player1Ready = true;
        boolean player2Ready = true;

        for (int count : player1ShipsPlaced) {
            if (count == 0) {
                player1Ready = false;
                break;
            }
        }

        for (int count : player2ShipsPlaced) {
            if (count == 0) {
                player2Ready = false;
                break;
            }
        }

        if (player1Ready == true && player2Ready == true) {
            isPlacingShips = false;
            isGameInProgress = true;
        }
    }

    /**
     * Switches the current player
     */
    private void switchPlayer() {
        currentPlayer = (currentPlayer == player1) ? player2 : player1;
    }

    /**
     * Gets the current player
     */
    public Player getCurrentPlayer() {
        return currentPlayer;
    }

    /**
     * Gets player 1
     */
    public Player getPlayer1() {
        return player1;
    }

    /**
     * Gets player 2
     */
    public Player getPlayer2() {
        return player2;
    }

    /**
     * Gets the winner of the game
     */
    public Player getWinner() {
        if (isGameInProgress == true) {
            return null;
        }

        if (player1Board.areAllShipsSunk() == true) {
            return player2;
        } else {
            return player1;
        }
    }

    /**
     * Checks if the game is in progress
     */
    public boolean isGameInProgress() {
        return isGameInProgress;
    }

    /**
     * Checks if ships are still being placed
     */
    public boolean isPlacingShips() {
        return isPlacingShips;
    }

    /**
     * Checks if the game is over
     */
    public boolean isGameOver() {
        return isGameInProgress == false && isPlacingShips == false;
    }

    /**
     * Sets the game in progress status
     */
    public void setGameInProgress(boolean inProgress) {
        this.isGameInProgress = inProgress;
    }
}