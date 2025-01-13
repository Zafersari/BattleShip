package battleship.battleship;

import javafx.fxml.FXML;
import javafx.scene.control.Label;
import javafx.scene.input.MouseButton;
import javafx.scene.layout.BorderPane;
import java.util.Random;

public class BattleShipController {

    @FXML
    private Label gameStatus;

    @FXML
    private BorderPane enemyBoardArea;

    @FXML
    private BorderPane playerBoardArea;

    private boolean running = false;
    private Board enemyBoard, playerBoard;
    private int shipsToPlace = 5;
    private boolean enemyTurn = false;
    private Random random = new Random();

    @FXML
    public void initialize() {
        setupGame();
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

            if (enemyBoard.ships == 0) {
                gameStatus.setText("YOU WIN!");
                running = false;
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

        // Add boards to their containers
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

            if (playerBoard.ships == 0) {
                gameStatus.setText("YOU LOSE!");
                running = false;
                return;
            }
        }
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
        gameStatus.setText("Game Started");
    }
}