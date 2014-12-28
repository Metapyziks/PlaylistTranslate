using System;
using System.Collections.Generic;
using System.IO;

namespace PlaylistTranslate
{
    public enum Format
    {
        XSPF
    }

    public class Program
    {
        private static void PrintUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  -i <input-path> (required)");
            Console.WriteLine("  -s <search-path> (at least one required)");
            Console.WriteLine("  -o <output-path> (optional)");
            Console.WriteLine("  -f <format (xspf)> (optional, default=xspf)");
        }

	    public static void Main(String[] args)
        {
            String input = null;
            String output = null;

            var musicPaths = new List<String>();
            var formatStr = "xspf";

            for (var i = 0; i < args.Length - 1; ++i) {
                switch (args[i]) {
                    case "-i":
                        input = args[++i];
                        break;
                    case "-o":
                        output = args[++i];
                        break;
                    case "-s":
                        musicPaths.Add(args[++i]);
                        break;
                    case "-f":
                        formatStr = args[++i];
                        break;
                    default:
                        Console.WriteLine("Unexpected option \"{0}\".", args[i]);
                        return;
                }
            }

            if (input == null) {
                PrintUsage();
                return;
            }

            Format format;
            if (!Enum.TryParse(formatStr, true, out format)) {
                Console.WriteLine("Unexpected format \"{0}\".", formatStr);
                return;
            }

            if (!File.Exists(input)) {
                Console.WriteLine("File \"{0}\" does not exist.", input);
                return;
            }

            if (String.IsNullOrWhiteSpace(output)) {
                var inputDir = Path.GetDirectoryName(input);

                output = Path.Combine(inputDir, Path.GetFileNameWithoutExtension(input))
                    + "." + format.ToString().ToLower();
            }

            var playlist = Playlist.Parse(File.ReadAllText(input));

            foreach (var track in playlist.DiscoverPaths(musicPaths)) {
                Console.WriteLine("The following track could not be found:");
                Console.WriteLine("  Title: {0}", track.Title);
                Console.WriteLine(" Artist: {0}", track.Artist);
                Console.WriteLine("  Album: {0}", track.Album);
                Console.WriteLine("  Guess: {0}", track.Path);
                Console.WriteLine("Please enter its path, or nothing if the guess is right.");

                String path = null;
                do {
                    if (path != null) {
                        Console.WriteLine("Invalid path. Please try again.");
                    }

                    Console.Write("> ");
                    path = Console.ReadLine();
                    
                    if (String.IsNullOrWhiteSpace(path)) path = track.Path;
                } while (!File.Exists(path));

                track.Path = path;
            }

            playlist.Export(output, format);
        }
    }
}