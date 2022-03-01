/*
 * Author: Martin Brown, 2022
 * Copyright (c) Marty's List (martys-list.com)
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Movie_Ranker
{
    /// <summary>
    /// Movie Ranking Program
    /// </summary>
    /// <remarks>This software collates a list of movies and prompts a user to assess each one against the list as a whole to order it.</remarks>
    class MovieRanker
    {
        public const string BETTER_COMMAND = "better";
        public const string WORSE_COMMAND = "worse";
        public const string SKIP_COMMAND = "skip";
        public const string YES_COMMAND = "y";
        public const string NO_COMMAND = "n";
        public const int MINIMUM_BOUNDARY = 1;


        static void Main(string[] args)
        {
            // Initiate program variables
            string[] commands = { BETTER_COMMAND, WORSE_COMMAND, SKIP_COMMAND };

            string movieDirectory = @"H:\Movies";
            string extraMoviesFilePath = @"C:\<your>\<path>\<here>\NetflixMovies.txt"; // Text file of netflix, cinema, etc
            string outputFileDirectory = @"C:\<your>\<path>\<here>\";
            string outputFileName = "MovieListRanked.txt";

            string outputFilePath = outputFileDirectory + outputFileName;



            // Construct the grand movies list
            var subDirectories = Directory.GetDirectories(movieDirectory).Select(dir => Path.GetFileName(dir)).ToList();
            var subFiles = Directory.GetFiles(movieDirectory).Select(file => Path.GetFileName(file)).ToList();
            var extraMovies = extraMoviesFilePath?.Equals(string.Empty) ?? true ? null : File.ReadAllLines(extraMoviesFilePath);

            var movieList = new List<string>(); // Input
            var orderedList = new List<string>(); // Output

            movieList.AddRange(subDirectories);
            movieList.AddRange(subFiles);

            if (extraMovies != null)
            {
                movieList.AddRange(extraMovies);
            }
           
            

            // Address each movie
            foreach (string movie in movieList)
            {
                Console.WriteLine($"\n\n----Assessing: {movie}----\n");
                
                // Decide on the first movie of the list
                if (!orderedList.Any())
                {
                    string messagePrompt = $"Would you like to add this movie to the list? ({YES_COMMAND.ToUpper()}/{NO_COMMAND.ToUpper()})";

                    Console.WriteLine(messagePrompt);
                    string action = Console.ReadLine().Trim().ToLower();

                    ValidateChoice(new string[] { YES_COMMAND, NO_COMMAND }, ref action, messagePrompt);

                    if (action.Equals(YES_COMMAND))
                    {
                        orderedList.Add(movie);
                    }
                }

                // Recursively binary insert the rest
                else
                {
                    BinaryInsert(commands, movie, orderedList, -1, orderedList.Count);
                }                
            }



            // Number 'em
            for (int index = 0; index < orderedList.Count; index++)
            {
                orderedList[index] = $"{index + 1}. {orderedList.ElementAt(index)}";
            }



            // Write the data
            File.Create(outputFilePath).Close(); // Necessary so we can write without sharing the same process

            File.WriteAllLines(outputFilePath, orderedList);
        }


        private static void BinaryInsert(string[] commands, string movie, List<string> movies, int minBoundary, int maxBoundary)
        {           
            int middleIndex = (maxBoundary + minBoundary) / 2;

            // Prompt
            Console.WriteLine($"\nIs {movie} better or worse than {movies.ElementAt(middleIndex)}?");

            string commandText = string.Join(", ", commands);

            Console.WriteLine($"Reminder of the commands: {commandText}");
            string action = Console.ReadLine().Trim().ToLower();

            ValidateChoice(commands, ref action, $"commands: {commandText}");


            // Interpret
            if (!action.Equals(SKIP_COMMAND))
            {
                // These essentially just say if we're about to move onto a boundary, that's likely our last stop for placement if the error margin is 1
                var lowerBoundaryError = middleIndex - minBoundary;
                var upperBoundaryError = maxBoundary - middleIndex;

                // If we haven't hit our goal (boundary), recurse. If we have, insert.
                if (action.Equals(BETTER_COMMAND))
                {
                    if (lowerBoundaryError == MINIMUM_BOUNDARY)
                    {
                        movies.Insert(middleIndex, movie);
                    }
                    else
                    {
                        BinaryInsert(commands, movie, movies, minBoundary, middleIndex);
                    }                            
                }
                else
                {
                    if (upperBoundaryError == MINIMUM_BOUNDARY)
                    {
                        movies.Insert(middleIndex + 1, movie);
                    }
                    else
                    {
                        BinaryInsert(commands, movie, movies, middleIndex, maxBoundary);
                    }
                }                                              
            }
        }


        private static void ValidateChoice(string[] commands, ref string input, string customMessage)
        {
            // Idiot proof that shit
            while (!commands.Contains(input))
            {
                Console.WriteLine($"\n**\"{input}\" is not recognised as a valid command.**\n");
                Console.WriteLine(customMessage);
                input = Console.ReadLine().Trim().ToLower();
            }
        }
    }
}
