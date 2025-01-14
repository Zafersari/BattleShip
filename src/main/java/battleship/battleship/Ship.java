package battleship.battleship;

/**
 * This class represents a ship in the Battleship game
 * It tracks the ship's length and damage status
 */
public class Ship {
    // Ship properties
    private int length;
    private int hitCount;

    /**
     * Creates a new ship with the specified length
     */
    public Ship(int length) {
        this.length = length;
        this.hitCount = 0;
    }

    /**
     * Records a hit on this ship
     */
    public void hit() {
        if (this.hitCount < this.length) {
            this.hitCount = this.hitCount + 1;
        }
    }

    /**
     * Checks if the ship has been sunk
     * Returns: true if hit count equals ship length, false otherwise
     */
    public boolean isSunk() {
        return this.hitCount == this.length;
    }

    /**
     * Gets the length of the ship
     * Returns: the ship's length
     */
    public int getLength() {
        return this.length;
    }

    /**
     * Gets the number of hits on this ship
     * Returns: the number of hits taken
     */
    public int getHitCount() {
        return this.hitCount;
    }
}