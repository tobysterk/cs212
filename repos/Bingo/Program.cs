using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace Bingo {
    class Program {
        private static RelationshipGraph rg;

        // Read RelationshipGraph whose filename is passed in as a parameter.
        // Build a RelationshipGraph in RelationshipGraph rg
        private static void ReadRelationshipGraph(string filename) {
            rg = new RelationshipGraph();                           // create a new RelationshipGraph object

            string name = "";                                       // name of person currently being read
            int numPeople = 0;
            string[] values;
            Console.Write("Reading file " + filename + "\n");
            try {
                string input = System.IO.File.ReadAllText(filename);// read file
                input = input.Replace("\r", ";");                   // get rid of nasty carriage returns 
                input = input.Replace("\n", ";");                   // get rid of nasty new lines
                string[] inputItems = Regex.Split(input, @";\s*");  // parse out the relationships (separated by ;)
                foreach (string item in inputItems) {
                    if (item.Length > 2)                            // don't bother with empty relationships
                    {
                        values = Regex.Split(item, @"\s*:\s*");     // parse out relationship:name
                        if (values[0] == "name")                    // name:[personname] indicates start of new person
                        {
                            name = values[1];                       // remember name for future relationships
                            rg.AddNode(name);                       // create the node
                            numPeople++;
                        }
                        else {
                            rg.AddEdge(name, values[1], values[0]); // add relationship (name1, name2, relationship)

                            // handle symmetric relationships -- add the other way
                            if (values[0] == "hasSpouse" || values[0] == "hasFriend")
                                rg.AddEdge(values[1], name, values[0]);

                            // for parent relationships add child as well
                            else if (values[0] == "hasParent")
                                rg.AddEdge(values[1], name, "hasChild");
                            else if (values[0] == "hasChild")
                                rg.AddEdge(values[1], name, "hasParent");
                        }
                    }
                }
            }
            catch (Exception e) {
                Console.Write("Unable to read file {0}: {1}\n", filename, e.ToString());
            }
            Console.WriteLine(numPeople + " people read");
        }

        // Show the relationships a person is involved in
        private static void ShowPerson(string name) {
            GraphNode n = rg.GetNode(name);
            if (n != null)
                Console.Write(n.ToString());
            else
                Console.WriteLine("{0} not found", name);
        }

        // Show a person's friends
        private static void ShowFriends(string name) {
            GraphNode n = rg.GetNode(name);
            if (n != null) {
                Console.Write("{0}'s friends: ", name);
                List<GraphEdge> friendEdges = n.GetEdges("hasFriend");
                foreach (GraphEdge e in friendEdges) {
                    Console.Write("{0} ", e.To());
                }
                Console.WriteLine();
            }
            else
                Console.WriteLine("{0} not found", name);
        }

        // ShowOrphans writes the names of the GraphNodes that have no parents
        private static void ShowOrphans() {
            foreach (GraphNode n in rg.nodes) {
                List<GraphEdge> parentEdges = n.GetEdges("hasParent");
                if (parentEdges.Count == 0)
                    Console.Write("{0} ", n.Name);
                else {
                    foreach (GraphEdge possibleParentEdge in parentEdges) {
                        if (possibleParentEdge.From() != n.Name)
                            parentEdges.Remove(possibleParentEdge);
                    }
                    if (parentEdges.Count == 0)
                        Console.Write("{0} ", n.Name);
                }
            }
            Console.WriteLine();
        }

        /* ShowSiblings prints the siblings of the GraphNode called name
         * @Param: name, the string of whose siblings are to be printed
         */
        private static void ShowSiblings(string name) {
            // find named node
            GraphNode beginning = rg.GetNode(name);
            if (beginning == null) {
                Console.WriteLine("{0} not found", name);
                return;

            } else {
                // find named node's parents
                List<GraphEdge> parentEdges = beginning.GetEdges("hasParent");
                if (parentEdges.Count == 0) {
                    Console.WriteLine("No parents of {0} found", name);
                    return;
                } else {
                    List<GraphNode> parents = new List<GraphNode>(parentEdges.Count);
                    foreach (GraphEdge parentEdge in parentEdges) {
                        parents.Add(rg.GetNode(parentEdge.To()));
                    }
                    // find named node's parent's other children
                    List<string> siblingStrings = new List<string>();
                    foreach (GraphNode parent in parents) {
                        List<GraphEdge> siblingEdges = parent.GetEdges("hasChild");
                        foreach (GraphEdge siblingEdge in siblingEdges) {
                            string possibleSibling = siblingEdge.To();
                            if ((!siblingStrings.Contains(possibleSibling)) && (possibleSibling != name))
                                siblingStrings.Add(possibleSibling);
                        }
                    }
                    // print the siblings
                    foreach (string sibling in siblingStrings) {
                        Console.Write("{0} ", sibling);
                    }
                }
                Console.WriteLine();
            }
        }
        
        /* PrintDescendant prints the title and name of the descendant whose GraphNode is d
         * @Param: d, the GraphNode representing the descendant
         */
        private static void PrintDescendant(GraphNode d) {
            int generation = int.Parse(d.Label);
            for (int i = 3; i <= generation; i++) {
                Console.Write("great-");
            }
            if (generation >= 2)
                Console.Write("grand");
            if (int.Parse(d.Label) >= 1) {
                Console.Write("child: {0}; ", d.Name);
            }
        }

        /* ShowDescendants prints the descendants of the GraphNode called name
         * @Param: name, the string of whose descendants are to be printed
         */
        private static void ShowDescendants(string name) {
            // find named node
            GraphNode beginning = rg.GetNode(name);
            if (beginning == null) {
                Console.WriteLine("{0} not found", name);
                return;
            }
            else {
                // find and print children, then print child's children
                List<GraphEdge> childEdges = beginning.GetEdges("hasChild");
                if (childEdges.Count == 0)
                    Console.WriteLine("{0} has no descendants", name);
                else {
                    Queue<GraphNode> descendants = new Queue<GraphNode>(childEdges.Count);
                    foreach (GraphEdge e in childEdges) {
                        GraphNode initialChild = rg.GetNode(e.To());
                        initialChild.Label = 1.ToString();
                        descendants.Enqueue(initialChild);
                    }

                    while (descendants.Count != 0) {
                        foreach (GraphNode descendant in descendants.ToList()) {
                            PrintDescendant(descendant);
                            // enqueue next edges and label them for printing
                            foreach (GraphEdge edge in descendant.GetEdges("hasChild")) {
                                GraphNode nextChild = rg.GetNode(edge.To());
                                nextChild.Label = (int.Parse(rg.GetNode(edge.From()).Label) + 1).ToString();
                                descendants.Enqueue(rg.GetNode(edge.To()));
                            }
                            // reset label and dequeue
                            descendant.Label = "Unvisited";
                            descendants.Dequeue();
                        }
                    }
                    Console.WriteLine();
                }
            }
        }


        private static void FindBingo(string name1, string name2) {
            GraphNode beginning = rg.GetNode(name1);
            GraphNode end = rg.GetNode(name2);
            List<GraphNode> connections = new List<GraphNode>();
            foreach (GraphEdge e in beginning.GetEdges()) {
                if (e.To() == name2) {
                    // done
                    Console.WriteLine("Done!");
                }
                connections.Add(rg.GetNode(e.To()));
            }
        }

        /* ShowCousins prints the  Nth cousins, Kth removed of the GraphNode called name
         * @Param: name, the string of whose cousins are to be found
         * @Param: N, which order of cousin should be printed
         * @Param: K, how many times removed the cousin is
         */
        private static void ShowCousins(string name, int N, int K) {
            if (N <= 0) {
                Console.WriteLine("N is invalid");
                return;
            } else if (K >= (N+1)) {
                Console.WriteLine("K is invalid");
                return;
            }
            // find named node
            GraphNode beginning = rg.GetNode(name);
            if (beginning == null) {
                Console.WriteLine("{0} not found", name);
                return;
            } else {

                Queue<GraphNode> findRelativeQueue = new Queue<GraphNode>();
                findRelativeQueue.Enqueue(beginning);
                List<GraphNode> nthLevelUp = new List<GraphNode>();

                // go up N+1 generations
                for (int upNum = 1; upNum <= (N + 1); upNum++) {
                    foreach (GraphNode person in findRelativeQueue.ToList()) { // in each generation,
                        // enqueue parents
                        foreach (GraphEdge parentEdge in person.GetEdges("hasParent")) {
                            findRelativeQueue.Enqueue(rg.GetNode(parentEdge.To()));
                        }
                        // dequeue 
                        if (upNum != (N + 1)) {
                            findRelativeQueue.Dequeue();
                        }
                        else {
                            nthLevelUp.Add(findRelativeQueue.Dequeue());
                        }
                    }
                }

                // Go down N-K+1 generations
                for (int downNum = 1; downNum <= (N - K + 1); downNum++) {
                    foreach (GraphNode person in findRelativeQueue.ToList()) {
                        // queue children
                        foreach (GraphEdge parentEdge in person.GetEdges("hasChild")) {
                            GraphNode child = rg.GetNode(parentEdge.To());
                            if ((!findRelativeQueue.Contains(child)) && (!nthLevelUp.Contains(child)))
                                findRelativeQueue.Enqueue(rg.GetNode(parentEdge.To()));
                        }
                        // dequeue self
                        findRelativeQueue.Dequeue();
                    }
                }

                // print first set of cousins
                if (findRelativeQueue.Count != 0) {
                    foreach (GraphNode cousin in findRelativeQueue.ToList()) {
                            Console.Write("{0} ", cousin.Name);
                        }
                }

                if (K != 0) {
                    // Go down 2K generations
                    for (int downNum = 1; downNum <= (2 * K); downNum++) {
                        foreach (GraphNode person in findRelativeQueue.ToList()) { // for each generation,
                                                                                   // queue children
                            foreach (GraphEdge parentEdge in person.GetEdges("hasChild")) {
                                GraphNode child = rg.GetNode(parentEdge.To());
                                if ((!findRelativeQueue.Contains(child)) && (!nthLevelUp.Contains(child)))
                                    findRelativeQueue.Enqueue(rg.GetNode(parentEdge.To()));
                            }
                            // dequeue self
                            findRelativeQueue.Dequeue();
                        }
                    }

                    // print second set of cousins
                    if (findRelativeQueue.Count != 0) {
                        foreach (GraphNode cousin in findRelativeQueue.ToList()) {
                            Console.Write("{0} ", cousin.Name);
                        }
                    }
                }
                Console.WriteLine();
            }
        }

        // accept, parse, and execute user commands
        private static void CommandLoop() {
            string command = "";
            string[] commandWords;
            Console.Write("Welcome to Harry's Dutch Bingo Parlor!\n");

            while (command != "exit") {
                Console.Write("\nEnter a command: ");
                command = Console.ReadLine();
                commandWords = Regex.Split(command, @"\s+");        // split input into array of words
                command = commandWords[0];

                if (command == "exit")
                    ;                                               // do nothing

                // read a relationship graph from a file
                else if (command == "read" && commandWords.Length > 1)
                    ReadRelationshipGraph(commandWords[1]);

                // show information for the whole graph
                else if (command == "orphans") {
                    ShowOrphans();
                }

                // show information for one person
                else if (command == "show" && commandWords.Length > 1)
                    ShowPerson(commandWords[1]);

                else if (command == "friends" && commandWords.Length > 1)
                    ShowFriends(commandWords[1]);

                else if (command == "siblings" && commandWords.Length > 1) {
                    ShowSiblings(commandWords[1]);
                }

                else if (command == "descendants" && commandWords.Length > 1) {
                    ShowDescendants(commandWords[1]);
                }

                else if (command == "cousins" && commandWords.Length > 3) {
                    ShowCousins(commandWords[1], int.Parse(commandWords[2]), int.Parse(commandWords[3]));
                }

                // show shortest chain between people
                else if (command == "bingo" && commandWords.Length > 2) {
                    FindBingo(commandWords[1], commandWords[2]);
                }
           
                // dump command prints out the graph
                else if (command == "dump")
                    rg.Dump();

                // illegal command
                else
                    Console.Write("\nLegal commands: read [filename], " +
                                                     "dump, " +
                                                     "show [personname], " +
                                                     "friends [personname], " +
                                                     "siblings [personname], " +
                                                     "descendents [personname], " +
                                                     "cousins [personname] [numCousin] [numTimesRemoved], " +
                                                     "exit\n");
            }
        }

        static void Main(string[] args) {
            CommandLoop();
        }
    }
}
