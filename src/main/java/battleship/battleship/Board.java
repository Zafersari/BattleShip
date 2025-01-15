package battleship.battleship;

/**
 * This class represents a game board in the Battleship game
 * It manages the placement of ships and tracking of shots
 */
public class Board {
    // Board dimensions
    private static final int BOARD_SIZE = 10;

    // Arrays to track ships and shots
    private Ship[][] ships;
    private boolean[][] shots;
    private int totalShipsPlaced;
    private int totalShipsSunk;

    /**
     * Creates a new game board
     */
    public Board() {
        this.ships = new Ship[BOARD_SIZE][BOARD_SIZE];
        this.shots = new boolean[BOARD_SIZE][BOARD_SIZE];
        this.totalShipsPlaced = 0;
        this.totalShipsSunk = 0;
    }

    /**
     * Places a ship on the board
     * Returns: true if ship was placed successfully, false otherwise
     */
    public boolean placeShip(int row, int col, int length, boolean isHorizontal) {
        // Check if placement is within board boundaries
        if (isHorizontal == true) {
            if (col + length > BOARD_SIZE) {
                return false;
            }
        } else {
            if (row + length > BOARD_SIZE) {
                return false;
            }
        }

        // Check if placement overlaps with other ships
        if (isHorizontal == true) {
            for (int i = 0; i < length; i++) {
                if (ships[row][col + i] != null) {
                    return false;
                }
            }
        } else {
            for (int i = 0; i < length; i++) {
                if (ships[row + i][col] != null) {
                    return false;
                }
            }
        }

        // Place the ship
        Ship newShip = new Ship(length);
        if (isHorizontal == true) {
            for (int i = 0; i < length; i++) {
                ships[row][col + i] = newShip;
            }
        } else {
            for (int i = 0; i < length; i++) {
                ships[row + i][col] = newShip;
            }
        }

        totalShipsPlaced = totalShipsPlaced + 1;
        return true;
    }

    /**
     * Processes an attack on the board
     * Returns: true if the attack hit a ship, false otherwise
     */
    public boolean receiveAttack(int row, int col) {
        // Mark this position as shot
        shots[row][col] = true;

        // Check if there is a ship at this position
        Ship ship = ships[row][col];
        if (ship != null) {
            ship.hit();
            if (ship.isSunk() == true) {
                totalShipsSunk = totalShipsSunk + 1;
            }
            return true;
        }

        return false;
    }

    /**
     * Checks if all ships on this board have been sunk
     * Returns: true if all ships are sunk, false otherwise
     */
    public boolean areAllShipsSunk() {
        int sunkCount = 0;
        for (int i = 0; i < BOARD_SIZE; i++) {
            for (int j = 0; j < BOARD_SIZE; j++) {
                if (ships[i][j] != null && ships[i][j].isSunk() == true) {
                    sunkCount++;
                }
            }
        }
        return totalShipsPlaced > 0 && sunkCount == totalShipsPlaced;
    }

    /**
     * Checks if all required ships have been placed
     * Returns: true if all ships are placed, false otherwise
     */
    public boolean areAllShipsPlaced() {
        return totalShipsPlaced == 5;  // Standard Battleship game has 5 ships
    }

    /**
     * Checks if a position has been shot at
     * Returns: true if the position has been shot, false otherwise
     */
    public boolean isShot(int row, int col) {
        return shots[row][col];
    }

    /**
     * Checks if there is a ship at the given position
     * Returns: true if there is a ship, false otherwise
     */
    public boolean hasShip(int row, int col) {
        return ships[row][col] != null;
    }
}