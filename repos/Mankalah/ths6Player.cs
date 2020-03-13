using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace Mankalah {

    public class ths6Player : Player {

        private bool timeUp { get; set; } // boolean for the timer
        private Position myPos { get; set; } // easy access to which spot the player is in
        private Timer timer = new System.Timers.Timer(4000); // default timer value because the timeLimit isn't static


        public ths6Player(Position pos, int timeLimit) : base(pos, "Toby", timeLimit) {
            myPos = pos;
        }

        public override string gloat() {
            return "Welp good game";
        }

        /* evaluate() evaluates a given board based on how many stones are on my side, how many go-again opportunities I have, and how many stones I have
         * @param: b, a Board to be evaluated
         * @return: an integer representing how favorable/unfavorable the board is
         */
        public override int evaluate(Board b) { // if top, want high; if bottom, want low
            int result = 0;

            // calculate position-based variables
            int myMultiplier = -1; // if on bottom, make beneficial contributions more negative
            int myFirstPosition = 0;
            if (myPos == Position.Top) {
                myMultiplier = 1; // if on top, make beneficial contributions more positive
                myFirstPosition = 7;
            }
            int myLastPosition = myFirstPosition + 5;
            int myKalahPosition = myLastPosition + 1;
            int theirFirstPosition = myFirstPosition - (7 * myMultiplier); // First positions are 0 and 7
            int theirLastPosition = myLastPosition - (7 * myMultiplier); // Last positions are 5 and 12
            int theirKalahPosition = theirLastPosition + 1;


            // maximize number of stones on my side
            for (int position = myFirstPosition; position <= myLastPosition; position++) {
                result += (b.stonesAt(position) * myMultiplier * 5); // adjustable weight
            }

            // simple choose go-again opportunities
            for (int position = myFirstPosition; position <= myLastPosition; position++) {
                if (b.stonesAt(position) == (myKalahPosition - position))
                    result += (b.stonesAt(position) * myMultiplier * 5); // adjustable weight
            }

            // choose captures (point increases)
            result += (b.stonesAt(myKalahPosition) * myMultiplier * 7); // adjustable weight


/*            // avoid capture opportunities for them
            for (int position = theirLastPosition; position <= theirFirstPosition; position--) {
                if (b.stonesAt(position) == 0) {
                    for (int smallerPos = theirFirstPosition; smallerPos < position; smallerPos++) {
                        if (b.stonesAt(smallerPos) == (position - smallerPos))
                            result -= (b.stonesAt(12 - position) * myMultiplier * 5); // adjustable weight
                    }
                }
            }*/
            return result;
        }

        /* miniMaxVal implements the minimax search of a board for a given depth
         * @param: board, the Board whose child boards will be evaluated
         * @param: depth, how many generations will be analyzed
         * @return: a Result that is the optimal move and its value
         */
        private Result miniMaxVal (Board board, int depth) {
            int bestMove = -1; // dummy value
            int bestVal = 0; // dummy value
            if (board.gameOver() || (depth == 0))
                return new Result(0, evaluate(board));

            if (board.whoseMove() == Position.Top) { // max
                bestVal = -9999999; // impossible to get less than this
                for (int move = 7; move <= 12; move++) {
                    if (board.legalMove(move) && !timeUp) { // stop if the timer has finished
                        Board boardCopy = new Board(board);
                        boardCopy.makeMove(move, false);
                        Result moveResult = miniMaxVal(boardCopy, depth - 1);
                        if (moveResult.value > bestVal) {
                            bestVal = moveResult.value;
                            bestMove = move;
                        }
                    }
                }
            }
            else { // BOTTOM'S MOVE (min)
                bestVal = 9999999; // impossible to get more than this
                for (int move = 0; move <= 5; move++) {
                    if (board.legalMove(move) && !timeUp) { // stop if the timer has finished
                        Board boardCopy = new Board(board);
                        boardCopy.makeMove(move, false);
                        Result moveResult = miniMaxVal(boardCopy, depth - 1);
                        if (moveResult.value < bestVal) {
                            bestVal = moveResult.value;
                            bestMove = move;
                        }

                    }
                }
            }
            return new Result(bestMove, bestVal);
        }

        /* chooseMove chooses the move the player will select given the current board
         * @param: b, the current board
         * @return: the move choice of the player
         * Starts a move timer of the time limit specified upon player creation
         * Starts an iterative DFS of the minimax search of b and selects the best completed one
         */
        public override int chooseMove(Board b) {
            timeUp = false;
            timer.Interval = getTimePerMove();
            timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);

            int idx = 1;
            int bestVal;
            int bestMove = -1;

            timer.Enabled = true;

            if (myPos == Position.Top) { // MAX
                bestVal = -999999999; // impossible to get less than this
                while (!timeUp) {
                    Result minimaxResult = miniMaxVal(b, idx);
                    if (minimaxResult.value >= bestVal) {
                        bestVal = minimaxResult.value; // because the while loop checks timeUp, picks the best completed search result
                        bestMove = minimaxResult.move;
                    }
                    idx++;
                }
            }
            else { // MIN
                bestVal = 999999999; // impossible to get more than this
                while (!timeUp) {
                    Result minimaxResult = miniMaxVal(b, idx);
                    if (minimaxResult.value <= bestVal) {
                        bestVal = minimaxResult.value; // because the while loop checks timeUp, picks the best completed search result
                        bestMove = minimaxResult.move;
                    }
                    idx++;
                }
            }
            return bestMove;
        }		
        
        // Event handler for the interrupt caused by the timer finishing
        private void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e) {
            timer.Stop();
            timeUp = true;
        }

        // Uses the specified picture
        public override String getImage() { return "ths6.png"; }

    }

    // Result class that allows for returning of moves and values
    public class Result {
        public int value { get; set; }
        public int move { get; set; }

        public Result(int selectedMove, int selectedValue) {
            value = selectedValue;
            move = selectedMove;
        }

        
    }
}
