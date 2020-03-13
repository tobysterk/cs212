using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace BabbleSample {
    /// Babble framework
    /// Starter code for CS212 Babble assignment
    public partial class MainWindow : Window {
        private static string input;               // input file
        private static string[] words;             // input file broken into array of words
        private static int wordCount = 200;        // number of words to babble
        private static Dictionary<string, ArrayList> associations; // hash table
        private static Random ran = new Random(); // random number generator
        private static int N; // order
        private static string currentFile; // name of current file (displayed in analyzeInput()

        public MainWindow() {
            InitializeComponent();
        }


        /* loadButton_Click() manages the response when the loadButton is clicked
         * Goes through the file and fills words[], sets currentFile, and calls analyzeInput()
         */
        private void loadButton_Click(object sender, RoutedEventArgs e) {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.FileName = "Sample"; // Default file name
            ofd.DefaultExt = ".txt"; // Default file extension
            ofd.Filter = "Text documents (.txt)|*.txt"; // Filter files by extension

            // Show open file dialog box
            if ((bool)ofd.ShowDialog()) {
                textBlock1.Text = "Loading file " + ofd.FileName + "\n";
                input = System.IO.File.ReadAllText(ofd.FileName);  // read file
                words = Regex.Split(input, @"\s+");       // split into array of words
            }
            // my additions
            currentFile = ofd.FileName;
            analyzeInput();
        }


        /* analyzeInput() manages the printing of the file analysis
         * Called by loadButton_Click() and orderComboBox_selectionChanged
         * Calls makeHashtable() to create the hashtable, prints the file's word stats, and calls dump() to print the hashtable contents
         */
        private void analyzeInput() {
            associations = makeHashtable();
            if (N == 1)
                textBlock1.Text = ("Number of words: " + words.Length + "(" + associations.Count + " unique)\n\n");
            else
                textBlock1.Text = ("Number of words: " + words.Length + "(" + associations.Count + " unique word combinations)\n\n");
            textBlock1.Text += ("Order " + N + " analysis of " + currentFile + ":\n\n");
            dump(associations);

        }

        /* babbleButton_click() manages the behavior of the program when the babbleButton is clicked
         * Generates a text of wordCount words based on the hashtable contents
         * If the last words in the file are selected, then it will restart from the beginning of the file
         */
        private void babbleButton_Click(object sender, RoutedEventArgs e) {
            // initialize array of first N words
            string[] currentSelection = new string[N];
            for (int j = 0; j < N; j++)
                currentSelection[j] = words[j];
            // compute the current key 
            string currentKey = currentSelection[0];
            for (int j = 1; j < N; j++)
                currentKey += ("-" + currentSelection[j]);
            
            // Print the first N words
            textBlock1.Text = String.Join(" ", currentSelection);

            ArrayList nextWordChoices = new ArrayList();
            for (int i = 0; i < wordCount; i++) {
                try {
                    nextWordChoices = associations[currentKey];
                } catch (KeyNotFoundException problem) {
                    // initialize currentSelection to first N words of doc
                    for (int j = 0; j < N; j++)
                        currentSelection[j] = words[j];

                    // compute the current key 
                    currentKey = currentSelection[0];
                    for (int j = 1; j < N; j++)
                        currentKey += ("-" + currentSelection[j]);

                    nextWordChoices = associations[currentKey];
                }

                // get and print next word
                string nextWord = nextWordChoices[ran.Next(0, nextWordChoices.Count)].ToString();
                textBlock1.Text += (" " + nextWord);

                // increment currentSelection
                for (int j = 0; j < (N - 1); j++) 
                    currentSelection[j] = currentSelection[j + 1];
                currentSelection[N - 1] = nextWord;

                // increment currentKey
                currentKey = currentSelection[0];
                for (int j = 1; j < N; j++) {
                    currentKey += "-" + currentSelection[j];
                }
            }
        }

        /* orderComboBox_SelectionChanged() manages the behavior when the order is changed from the dropdown menu
         * Sets N to the selected order, then either prompts for a file to be loaded or starts the analysis by calling analyzeInput()
         */
        private void orderComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            N = orderComboBox.SelectedIndex + 1; // Default is Order 0, so this will be 1
            if (associations == null) {
                if (N != 1)
                    textBlock1.Text = "Please load a file to begin Order " + N + " analysis";
            }
            else
                analyzeInput();


        }

        /* makeHashtable() creates the hashtable from words[]
         * Called by analyzeInput()
         */
        static Dictionary<string, ArrayList> makeHashtable() {
            // Make variables
            Dictionary<string, ArrayList> hashTable = new Dictionary<string, ArrayList>();
            string key = "";

            // Iterate through words[] and add to hashtable
            for (int i = 0; i < (words.Length - N); i++) {
                key = words[i];
                for (int j = 1; j < N; j++) {
                    key += ("-" + words[i + j]);
                }
                if (!hashTable.ContainsKey(key))
                    hashTable.Add(key, new ArrayList());
                hashTable[key].Add(words[i + N]);
            }
            return hashTable;
        }

        /* dump() prints the contents of the hashtable to textBlock1
         * Called by analyzeInput()
         */
        void dump(Dictionary<string, ArrayList> hashTable) {
            foreach (KeyValuePair<string, ArrayList> entry in hashTable) {
                textBlock1.Text += (entry.Key + " -> ");
                for (int i = 0; i < entry.Value.Count; i++) {
                    if (i == entry.Value.Count - 1)
                        textBlock1.Text += (entry.Value[i] + "\n");
                    else
                        textBlock1.Text += (entry.Value[i] + " ");
                }
            }
        }
    }
}
