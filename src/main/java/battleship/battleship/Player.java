package battleship.battleship;

/**
 * This class represents a player in the Battleship game
 */
public class Player {
    // Player's name
    private String name;

    /**
     * Creates a new player with the given name
     */
    public Player(String name) {
        this.name = name;
    }

    /**
     * Gets the player's name
     * Returns: The name of the player
     */
    public String getName() {
        return this.name;
    }

    /**
     * Returns a string representation of the player
     * Returns: The player's name
     */
    @Override
    public String toString() {
        return this.name;
    }
}