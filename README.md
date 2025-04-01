# ChameleonGame

Simple board game implemented as an assignment for the Event-driven 
applications class at ELTE in my 3rd semester of university. 

## Task description

Let's write a program to play the following two-player boardgame. Given
a board of *n* × *n* squares, where the squares can take on two colors in a
spiral (traditionally red and green), and the middle square is gray. On
each square, except the middle, there is a chameleon of the same color
as the square, so each player has (*n*<sup>2</sup> − 1)/2 chameleons.

Players take turns moving. A chameleon can move to an adjacent empty
square (horizontally or vertically), or can jump over the opponent's
chameleon (horizontally or vertically), if the next square is empty. 
The jumped chameleon is removed from the board. Each player's goal is
to make their opponent lose all of their pieces.

The unique aspect of the game is that chameleons change color to match
their surroundings. If a chameleon has jumped or stepped onto a square
of a different color, then after 1 more turn it will change to the other
color (so it will belong to the other player). The exception is the
middle square.

The program should provide the ability to start a new game by specifying
the board size (3×3, 5×5, 7×7), as well as to save and load the game.
Recognize when the game is over and display which player won.
