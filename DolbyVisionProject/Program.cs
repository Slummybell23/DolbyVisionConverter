﻿using System.Diagnostics;
using System.Text;

namespace DolbyVisionProject;

public abstract class Program
{
    private static void Main(string[] args)
    {
        var consoleLog = new ConsoleLog();
        var converter = new Converter(consoleLog);
        var startHour = 5;
        
        string movieFolder;
        string tvShowFolder;
        
        string checkAll;
        
        if (Debugger.IsAttached)
        {
            checkAll = "y";
            movieFolder = "Z:\\Plex\\Movie";
            movieFolder = "Z:\\Plex\\Movie\\Coraline (2009)";
            tvShowFolder = "Z:\\Plex\\TvShow";
        }
        else
        {
            movieFolder = Environment.GetEnvironmentVariable("MOVIE_FOLDER")!;
            tvShowFolder = Environment.GetEnvironmentVariable("TVSHOW_FOLDER")!;
            checkAll = Environment.GetEnvironmentVariable("CHECK_ALL")!;
        }
        
        while (true)
        {
            var now = DateTime.Now;
            var hoursTill5 = Math.Abs(startHour - now.Hour);

            if (hoursTill5 == 0)
            {
                var nonDolbyVision7 = 0;
                var failedFiles = new List<string>();
                var convertedFiles = new List<string>();

                var directory = converter.BuildFilesList(movieFolder, tvShowFolder, checkAll);
                consoleLog.WriteLine($"Processing {directory.Count} files...");
                foreach (var file in directory)
                {
                    consoleLog.LogText = new StringBuilder();
                    consoleLog.WriteLine($"Processing file: {file}");

                    if (converter.IsProfile7(file))
                    {
                        consoleLog.WriteLine($"Dolby Vision Profile 7 detected in: {file}");

                        var start = DateTime.Now;
                        var converted = converter.ConvertFile(file);

                        if (converted)
                            convertedFiles.Add(file);
                        else
                            failedFiles.Add(file);

                        var end = DateTime.Now;
                        var timeCost = end - start;
                        consoleLog.WriteLine($"Conversion Time: {timeCost.ToString()}");

                        consoleLog.LogFile(file);
                    }
                    else
                    {
                        consoleLog.WriteLine($"Skipping: {file} (not Dolby Vision Profile 7)");
                        nonDolbyVision7++;
                    }
                }

                var endRunOutput = new StringBuilder();
                endRunOutput.AppendLine($"{directory.Count} files processed");
                endRunOutput.AppendLine($"{nonDolbyVision7} files skipped");
                endRunOutput.AppendLine($"{failedFiles.Count} files failed");
                endRunOutput.AppendLine($"{convertedFiles.Count} files converted");

                endRunOutput.AppendLine($"============= Converted Files ============");
                foreach (var converted in convertedFiles)
                {
                    endRunOutput.AppendLine(converted);
                }
                endRunOutput.AppendLine($"==========================================");

                endRunOutput.AppendLine($"============= Failed Files ============");
                foreach (var failed in failedFiles)
                {
                    endRunOutput.AppendLine(failed);
                }
                endRunOutput.AppendLine($"=======================================");

                consoleLog.WriteLine(endRunOutput.ToString());
                
                checkAll = "n";
                consoleLog.WriteLine("Waiting for new files... Setting to recent files");
                
                now = DateTime.Now;
                hoursTill5 = Math.Abs(startHour - now.Hour);
                consoleLog.WriteLine($"Waiting until 5...\n{hoursTill5} hours remaining from time of log.");
                Thread.Sleep(TimeSpan.FromHours(hoursTill5));
            }
            else
            {
                consoleLog.WriteLine($"Waiting until 5...\n{hoursTill5} hours remaining from time of log.");
                Thread.Sleep(TimeSpan.FromHours(hoursTill5));
            }
        }
    }
}