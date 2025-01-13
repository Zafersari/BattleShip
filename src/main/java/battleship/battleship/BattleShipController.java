package battleship.battleship;

import javafx.fxml.FXML;
import javafx.scene.control.Label;
import javafx.scene.control.Alert;
import javafx.scene.control.Alert.AlertType;
import javafx.scene.control.ButtonType;
import javafx.scene.input.MouseButton;
import javafx.scene.layout.BorderPane;
import javafx.application.Platform;
import java.util.Optional;
import java.util.Random;

public class BattleShipController {

    @FXML
    private Label statusLabel;

    @FXML
    private Label scoreLabel; // Yeni eklendi

    @FXML
    private BorderPane enemyBoardArea;

    @FXML
    private BorderPane playerBoardArea;

    private boolean running = false;
    private Board enemyBoard, playerBoard;
    private int shipsToPlace = 5;
    private boolean enemyTurn = false;
    private Random random = new Random();

    // Skor değişkenleri
    private int playerScore = 0;
    private int enemyScore = 0;
    private static final int HIT_POINTS = 10;  // Her isabetli vuruş için 10 puan
    private static final int MISS_PENALTY = -2; // Her ıskalama için -2 puan

    @FXML
    public void initialize() {
        showWelcomeMessage();
        updateScore(); // Başlangıç skorunu göster
    }

    private void updateScore() {
        scoreLabel.setText(String.format("Score - You: %d | Enemy: %d", playerScore, enemyScore));
    }

    private void setupGame() {
        // Create enemy board
        enemyBoard = new Board(true, event -> {
            if (!running)
                return;

            Board.Cell cell = (Board.Cell) event.getSource();
            if (cell.wasShot)
                return;

            enemyTurn = !cell.shoot();

            // Oyuncu vuruş sonucu
            if (!enemyTurn) {
                playerScore += HIT_POINTS;  // İsabet
            } else {
                playerScore += MISS_PENALTY; // Karavana
            }
            updateScore();

            if (enemyBoard.ships == 0) {
                playerScore += 50; // Oyunu kazanma bonusu
                updateScore();
                showGameEndAlert(true);
                return;
            }

            if (enemyTurn)
                enemyMove();
        });

        // Create player board
        playerBoard = new Board(false, event -> {
            if (running)
                return;

            Board.Cell cell = (Board.Cell) event.getSource();
            if (playerBoard.placeShip(new Ship(shipsToPlace, event.getButton() == MouseButton.PRIMARY),
                    cell.x, cell.y)) {
                if (--shipsToPlace == 0) {
                    startGame();
                }
            }
        });

        enemyBoardArea.setCenter(enemyBoard);
        playerBoardArea.setCenter(playerBoard);
    }

    private void enemyMove() {
        while (enemyTurn) {
            int x = random.nextInt(10);
            int y = random.nextInt(10);

            Board.Cell cell = playerBoard.getCell(x, y);
            if (cell.wasShot)
                continue;

            enemyTurn = cell.shoot();

            // Düşman vuruş sonucu
            if (enemyTurn) {
                enemyScore += HIT_POINTS;  // İsabet
            } else {
                enemyScore += MISS_PENALTY; // Karavana
            }
            updateScore();

            if (playerBoard.ships == 0) {
                enemyScore += 50; // Oyunu kazanma bonusu
                updateScore();
                showGameEndAlert(false);
                return;
            }
        }
    }

    private void resetGame() {
        // Oyun değişkenlerini sıfırla
        running = false;
        shipsToPlace = 5;
        enemyTurn = false;
        playerScore = 0;  // Skorları sıfırla
        enemyScore = 0;

        // Tahtaları temizle
        enemyBoardArea.getChildren().clear();
        playerBoardArea.getChildren().clear();

        updateScore();  // Skor göstergesini güncelle
        showWelcomeMessage();
    }

    private void showWelcomeMessage() {
        Alert welcome = new Alert(AlertType.INFORMATION);
        welcome.setTitle("Welcome");
        welcome.setHeaderText("Welcome to Battleship!");
        welcome.setContentText("Place your 5 ships on the board to start the game.\nCurrent High Score: " + playerScore);
        welcome.showAndWait();
        setupGame();
        statusLabel.setText("Place your ships");
    }

    private void startGame() {
        // Place enemy ships
        int type = 5;

        while (type > 0) {
            int x = random.nextInt(10);
            int y = random.nextInt(10);

            if (enemyBoard.placeShip(new Ship(type, Math.random() < 0.5), x, y)) {
                type--;
            }
        }

        running = true;
        statusLabel.setText("Game Started - Your turn");

        Alert gameStart = new Alert(AlertType.INFORMATION);
        gameStart.setTitle("Game Started");
        gameStart.setHeaderText("All ships are placed!");
        gameStart.setContentText("The game has started. Good luck!\nTry to beat the high score: " + playerScore);
        gameStart.show();
    }

    private void showGameEndAlert(boolean playerWon) {
        running = false;
        Alert alert = new Alert(AlertType.CONFIRMATION);
        alert.setTitle("Game Over");
        String scoreMessage = String.format("\nFinal Score:\nYou: %d\nEnemy: %d", playerScore, enemyScore);
        alert.setHeaderText((playerWon ? "Congratulations! You Won!" : "Game Over! You Lost!") + scoreMessage);
        alert.setContentText("Would you like to play again?");

        Optional<ButtonType> result = alert.showAndWait();
        if (result.isPresent() && result.get() == ButtonType.OK) {
            resetGame();
        } else {
            Platform.exit();
        }
    }
}