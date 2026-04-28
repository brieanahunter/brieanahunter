import javax.swing.*;
import java.awt.*;
import java.util.ArrayList;
import java.util.Random;

public class HangmanGame extends JFrame {

	//JFrame Layout
    private CardLayout cardLayout;
    private JPanel mainPanel;

    //Select Diffculty
    private JComboBox<String> modeBox;
    private JComboBox<String> difficultyBox;
    private JTextField customWordField;

    //Labels
    private JLabel wordLabel;
    private JLabel messageLabel;
    private JLabel guessedLettersLabel;
    private JLabel scoreLabel;

    //Guessing fields
    private JTextField letterGuessField;
    private JTextField wordGuessField;

    //Buttons
    private JButton guessLetterButton;
    private JButton guessWordButton;
    private JButton hintButton;
    private JButton newGameButton;

    private HangmanDrawing hangmanDrawing;

    //Declare Variables
    private String secretWord;
    private char[] hiddenWord;
    private ArrayList<Character> guessedLetters;
    private int wrongGuesses;
    private final int MAX_WRONG_GUESSES = 6;
    private int wins = 0;
    private int losses = 0;
    private boolean gameOver = false;
    private final Random random = new Random();

    //Word Banks for each level
    private final String[] easyWords = {
            "cat", "dog", "sun", "happy", "pig", "book", "cake", "java", "moon", "star"
    };

    private final String[] mediumWords = {
            "javascript", "computer", "boolean", "recursion", "keyword", "planet", "galaxies", "library", "monster", "silver"
    };

    private final String[] hardWords = {
            "sanctimoniously", "precocious", "soliloquies", "machiavellian", "encyclopedia", "xylophone", "psychology", "maladies", "circumstances", "masqueraded"
    };

    //
    public HangmanGame() {
        setTitle("Hangman Final Project");
        setSize(850, 650);
        setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
        setLocationRelativeTo(null);
        setResizable(false);

        cardLayout = new CardLayout();
        mainPanel = new JPanel(cardLayout);

        mainPanel.add(createMenuPanel(), "Menu");
        mainPanel.add(createGamePanel(), "Game");

        add(mainPanel);
        cardLayout.show(mainPanel, "Menu");
    }

    private JPanel createMenuPanel() {
        JPanel panel = new JPanel();
        panel.setLayout(null);
        panel.setBackground(new Color(245, 247, 250));

        JLabel titleLabel = new JLabel("Hangman", SwingConstants.CENTER);
        titleLabel.setFont(new Font("Arial", Font.BOLD, 48));
        titleLabel.setBounds(0, 40, 850, 70);
        panel.add(titleLabel);

        JLabel subtitleLabel = new JLabel("Classic word guessing game", SwingConstants.CENTER);
        subtitleLabel.setFont(new Font("Arial", Font.PLAIN, 20));
        subtitleLabel.setBounds(0, 105, 850, 35);
        panel.add(subtitleLabel);

        JLabel modeLabel = new JLabel("Game Mode:");
        modeLabel.setFont(new Font("Arial", Font.BOLD, 18));
        modeLabel.setBounds(280, 180, 130, 30);
        panel.add(modeLabel);

        modeBox = new JComboBox<>(new String[]{"Random Word Bank", "Multiplayer Custom Word"});
        modeBox.setFont(new Font("Arial", Font.PLAIN, 16));
        modeBox.setBounds(410, 180, 220, 35);
        panel.add(modeBox);

        JLabel difficultyLabel = new JLabel("Difficulty:");
        difficultyLabel.setFont(new Font("Arial", Font.BOLD, 18));
        difficultyLabel.setBounds(280, 235, 130, 30);
        panel.add(difficultyLabel);

        difficultyBox = new JComboBox<>(new String[]{"Easy", "Medium", "Hard"});
        difficultyBox.setFont(new Font("Arial", Font.PLAIN, 16));
        difficultyBox.setBounds(410, 235, 220, 35);
        panel.add(difficultyBox);

        JLabel customWordLabel = new JLabel("Custom Word:");
        customWordLabel.setFont(new Font("Arial", Font.BOLD, 18));
        customWordLabel.setBounds(280, 290, 130, 30);
        panel.add(customWordLabel);

        customWordField = new JTextField();
        customWordField.setFont(new Font("Arial", Font.PLAIN, 16));
        customWordField.setBounds(410, 290, 220, 35);
        customWordField.setToolTipText("Only used for Multiplayer Custom Word mode");
        panel.add(customWordField);

        JButton startButton = new JButton("Start Game");
        startButton.setFont(new Font("Arial", Font.BOLD, 20));
        startButton.setBounds(330, 365, 190, 50);
        panel.add(startButton);

        JTextArea instructions = new JTextArea(
                "Instructions:\n" +
                        "1. Choose a mode and difficulty.\n" +
                        "2. Guess one letter at a time.\n" +
                        "3. Correct letters reveal spots in the word.\n" +
                        "4. Wrong guesses draw the hangman.\n" +
                        "5. You can also guess the full word."
        );
        instructions.setFont(new Font("Arial", Font.PLAIN, 16));
        instructions.setEditable(false);
        instructions.setBackground(new Color(245, 247, 250));
        instructions.setBounds(260, 450, 360, 130);
        panel.add(instructions);

        startButton.addActionListener(e -> startNewGame());

        return panel;
    }

    private JPanel createGamePanel() {
        JPanel panel = new JPanel();
        panel.setLayout(new BorderLayout());
        panel.setBackground(Color.WHITE);

        JPanel topPanel = new JPanel(new GridLayout(4, 1));
        topPanel.setBackground(Color.WHITE);

        wordLabel = new JLabel("_ _ _", SwingConstants.CENTER);
        wordLabel.setFont(new Font("Consolas", Font.BOLD, 42));
        topPanel.add(wordLabel);

        messageLabel = new JLabel("Enter a guess.", SwingConstants.CENTER);
        messageLabel.setFont(new Font("Arial", Font.BOLD, 18));
        topPanel.add(messageLabel);

        guessedLettersLabel = new JLabel("Guessed Letters: None", SwingConstants.CENTER);
        guessedLettersLabel.setFont(new Font("Arial", Font.PLAIN, 16));
        topPanel.add(guessedLettersLabel);

        scoreLabel = new JLabel("Wins: 0 | Losses: 0", SwingConstants.CENTER);
        scoreLabel.setFont(new Font("Arial", Font.PLAIN, 16));
        topPanel.add(scoreLabel);

        panel.add(topPanel, BorderLayout.NORTH);

        hangmanDrawing = new HangmanDrawing();
        panel.add(hangmanDrawing, BorderLayout.CENTER);

        JPanel bottomPanel = new JPanel();
        bottomPanel.setLayout(new GridLayout(4, 1, 10, 10));
        bottomPanel.setBorder(BorderFactory.createEmptyBorder(10, 40, 20, 40));
        bottomPanel.setBackground(Color.WHITE);

        JPanel letterPanel = new JPanel();
        letterPanel.setBackground(Color.WHITE);
        letterPanel.add(new JLabel("Guess Letter:"));
        letterGuessField = new JTextField(5);
        letterGuessField.setFont(new Font("Arial", Font.PLAIN, 18));
        letterPanel.add(letterGuessField);
        guessLetterButton = new JButton("Guess Letter");
        letterPanel.add(guessLetterButton);
        bottomPanel.add(letterPanel);

        JPanel wordPanel = new JPanel();
        wordPanel.setBackground(Color.WHITE);
        wordPanel.add(new JLabel("Guess Word:"));
        wordGuessField = new JTextField(12);
        wordGuessField.setFont(new Font("Arial", Font.PLAIN, 18));
        wordPanel.add(wordGuessField);
        guessWordButton = new JButton("Guess Word");
        wordPanel.add(guessWordButton);
        bottomPanel.add(wordPanel);

        JPanel actionPanel = new JPanel();
        actionPanel.setBackground(Color.WHITE);
        hintButton = new JButton("Hint");
        newGameButton = new JButton("New Game");
        JButton menuButton = new JButton("Main Menu");
        actionPanel.add(hintButton);
        actionPanel.add(newGameButton);
        actionPanel.add(menuButton);
        bottomPanel.add(actionPanel);

        JLabel reminderLabel = new JLabel("Duplicate letters are not allowed. You get 6 wrong guesses.", SwingConstants.CENTER);
        reminderLabel.setFont(new Font("Arial", Font.ITALIC, 14));
        bottomPanel.add(reminderLabel);

        panel.add(bottomPanel, BorderLayout.SOUTH);

        guessLetterButton.addActionListener(e -> guessLetter());
        guessWordButton.addActionListener(e -> guessFullWord());
        hintButton.addActionListener(e -> useHint());
        newGameButton.addActionListener(e -> startNewGame());
        menuButton.addActionListener(e -> cardLayout.show(mainPanel, "Menu"));

        letterGuessField.addActionListener(e -> guessLetter());
        wordGuessField.addActionListener(e -> guessFullWord());

        return panel;
    }

    private void startNewGame() {
        String selectedMode = (String) modeBox.getSelectedItem();

        if (selectedMode.equals("Multiplayer Custom Word")) {
            String customWord = customWordField.getText().trim().toLowerCase();

            if (!customWord.matches("[a-zA-Z]+")) {
                JOptionPane.showMessageDialog(this, "Please enter a custom word using letters only.");
                return;
            }

            secretWord = customWord;
        } else {
            secretWord = getRandomWord();
        }

        hiddenWord = new char[secretWord.length()];
        for (int i = 0; i < hiddenWord.length; i++) {
            hiddenWord[i] = '_';
        }

        guessedLetters = new ArrayList<>();
        wrongGuesses = 0;
        gameOver = false;

        letterGuessField.setText("");
        wordGuessField.setText("");

        updateDisplay();
        messageLabel.setText("New game started! Guess a letter.");
        hangmanDrawing.setWrongGuesses(wrongGuesses);
        cardLayout.show(mainPanel, "Game");
    }

    private String getRandomWord() {
        String difficulty = (String) difficultyBox.getSelectedItem();
        String[] wordBank;

        if (difficulty.equals("Easy")) {
            wordBank = easyWords;
        } else if (difficulty.equals("Medium")) {
            wordBank = mediumWords;
        } else {
            wordBank = hardWords;
        }

        return wordBank[random.nextInt(wordBank.length)].toLowerCase();
    }

    private void guessLetter() {
        if (gameOver) {
            messageLabel.setText("The game is over. Start a new game.");
            return;
        }

        String input = letterGuessField.getText().trim().toLowerCase();
        letterGuessField.setText("");

        if (!input.matches("[a-zA-Z]")) {
            messageLabel.setText("Please enter exactly one letter.");
            return;
        }

        char guessedLetter = input.charAt(0);

        if (guessedLetters.contains(guessedLetter)) {
            messageLabel.setText("You already guessed the letter '" + guessedLetter + "'. Try another one.");
            return;
        }

        guessedLetters.add(guessedLetter);

        boolean correctGuess = false;

        for (int i = 0; i < secretWord.length(); i++) {
            if (secretWord.charAt(i) == guessedLetter) {
                hiddenWord[i] = guessedLetter;
                correctGuess = true;
            }
        }

        if (correctGuess) {
            messageLabel.setText("Correct! The letter '" + guessedLetter + "' is in the word.");
        } else {
            wrongGuesses++;
            messageLabel.setText("Incorrect! The letter '" + guessedLetter + "' is not in the word.");
        }

        updateDisplay();
        checkGameStatus();
    }

    private void guessFullWord() {
        if (gameOver) {
            messageLabel.setText("The game is over. Start a new game.");
            return;
        }

        String guessedWord = wordGuessField.getText().trim().toLowerCase();
        wordGuessField.setText("");

        if (!guessedWord.matches("[a-zA-Z]+")) {
            messageLabel.setText("Please enter a full word using letters only.");
            return;
        }

        if (guessedWord.equals(secretWord)) {
            for (int i = 0; i < secretWord.length(); i++) {
                hiddenWord[i] = secretWord.charAt(i);
            }
            updateDisplay();
            winGame();
        } else {
            wrongGuesses++;
            messageLabel.setText("That word is incorrect. One hangman part was added.");
            updateDisplay();
            checkGameStatus();
        }
    }

    private void useHint() {
        if (gameOver) {
            messageLabel.setText("The game is over. Start a new game.");
            return;
        }

        ArrayList<Character> hiddenLetters = new ArrayList<>();

        for (int i = 0; i < secretWord.length(); i++) {
            char currentLetter = secretWord.charAt(i);
            if (hiddenWord[i] == '_' && !hiddenLetters.contains(currentLetter)) {
                hiddenLetters.add(currentLetter);
            }
        }

        if (hiddenLetters.isEmpty()) {
            messageLabel.setText("No hints available because all letters are revealed.");
            return;
        }

        char hintLetter = hiddenLetters.get(random.nextInt(hiddenLetters.size()));

        if (!guessedLetters.contains(hintLetter)) {
            guessedLetters.add(hintLetter);
        }

        for (int i = 0; i < secretWord.length(); i++) {
            if (secretWord.charAt(i) == hintLetter) {
                hiddenWord[i] = hintLetter;
            }
        }

        messageLabel.setText("Hint used! The letter '" + hintLetter + "' was revealed.");
        updateDisplay();
        checkGameStatus();
    }

    private void updateDisplay() {
        StringBuilder displayedWord = new StringBuilder();
        for (char letter : hiddenWord) {
            displayedWord.append(letter).append(" ");
        }
        wordLabel.setText(displayedWord.toString());

        if (guessedLetters.isEmpty()) {
            guessedLettersLabel.setText("Guessed Letters: None");
        } else {
            guessedLettersLabel.setText("Guessed Letters: " + guessedLetters.toString());
        }

        scoreLabel.setText("Wins: " + wins + " | Losses: " + losses + " | Wrong Guesses: " + wrongGuesses + "/" + MAX_WRONG_GUESSES);
        hangmanDrawing.setWrongGuesses(wrongGuesses);
    }

    private void checkGameStatus() {
        if (String.valueOf(hiddenWord).equals(secretWord)) {
            winGame();
        } else if (wrongGuesses >= MAX_WRONG_GUESSES) {
            loseGame();
        }
    }

    private void winGame() {
        gameOver = true;
        wins++;
        messageLabel.setText("You won! The word was: " + secretWord);
        updateDisplay();
    }

    private void loseGame() {
        gameOver = true;
        losses++;
        messageLabel.setText("You lost! The word was: " + secretWord);
        updateDisplay();
    }

    public static void main(String[] args) {
        SwingUtilities.invokeLater(() -> {
            HangmanGame game = new HangmanGame();
            game.setVisible(true);
        });
    }
}
