package battleship.battleship;

import javafx.application.Application;
import javafx.fxml.FXMLLoader;
import javafx.scene.Scene;
import javafx.stage.Stage;

/**
 * Main application class that starts the Battleship game
 */
public class BattleShipApplication extends Application {
    @Override
    public void start(Stage stage) throws Exception {
        // Load the FXML file
        FXMLLoader fxmlLoader = new FXMLLoader(getClass().getResource("/battleship/battleship/battleship.fxml"));
        Scene scene = new Scene(fxmlLoader.load());

        // Initialize the game and controller
        BattleShipController controller = fxmlLoader.getController();
        Game game = new Game();

        // Create players and initialize the game
        Player player1 = new Player("Player 1");
        Player player2 = new Player("Player 2");
        game.initializeGame();
        game.setPlayers(player1, player2);

        // Set the game instance in controller
        controller.setGame(game);

        // Configure and show the stage
        stage.setTitle("Battleship");
        stage.setScene(scene);
        stage.setResizable(false);
        stage.show();
    }

    public static void main(String[] args) {
        launch();
    }
}