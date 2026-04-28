import javax.swing.*;
import java.awt.*;

public class HangmanDrawing extends JPanel {

    private int wrongGuesses = 0;

    public HangmanDrawing() {
        setPreferredSize(new Dimension(850, 300));
        setBackground(Color.WHITE);
    }

    public void setWrongGuesses(int wrongGuesses) {
        this.wrongGuesses = wrongGuesses;
        repaint();
    }

    @Override
    protected void paintComponent(Graphics g) {
        super.paintComponent(g);

        Graphics2D g2 = (Graphics2D) g;
        g2.setStroke(new BasicStroke(4));
        g2.setColor(Color.BLACK);

        // Gallows/base
        g2.drawLine(250, 260, 500, 260);
        g2.drawLine(310, 260, 310, 40);
        g2.drawLine(310, 40, 450, 40);
        g2.drawLine(450, 40, 450, 75);

        // Hangman body parts based on wrong guesses
        if (wrongGuesses >= 1) {
            // Head
            g2.drawOval(425, 75, 50, 50);
        }

        if (wrongGuesses >= 2) {
            // Body
            g2.drawLine(450, 125, 450, 190);
        }

        if (wrongGuesses >= 3) {
            // Left arm
            g2.drawLine(450, 145, 415, 165);
        }

        if (wrongGuesses >= 4) {
            // Right arm
            g2.drawLine(450, 145, 485, 165);
        }

        if (wrongGuesses >= 5) {
            // Left leg
            g2.drawLine(450, 190, 420, 230);
        }

        if (wrongGuesses >= 6) {
            // Right leg
            g2.drawLine(450, 190, 480, 230);
        }
    }
}

