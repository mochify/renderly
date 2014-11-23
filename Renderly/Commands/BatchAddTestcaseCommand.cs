using System;
using System.IO;

using ManyConsole;
using Mochify.Simile.Core.Models.Csv;
using Mochify.Simile.Core.Utils;

using Common.Logging;

namespace Renderly.Commands
{
    /// <summary>
    /// This is the ConsoleCommand that handles batch generation of test cases from a file.
    /// The file should be a CSV command with the following column layout:
    /// - test type,
    /// - url/path to retrieve to make your reference image
    /// - release
    /// - description (optional, leave blank if you want)
    /// </summary>
    public class BatchAddTestCaseCommand : ConsoleCommand
    {
        private static readonly ILog _log = LogManager.GetCurrentClassLogger();

        public string InputFile { get; set; }
        public string AppendTestFile { get; set; }
        public string OutputFile { get; set; }

        public BatchAddTestCaseCommand()
        {
            IsCommand("batchadd", "Add test cases as a batch");
            HasRequiredOption("f|file=", "CSV file with the test cases to generate.", s => InputFile = s);
            HasRequiredOption("o|out=", "CSV file to output with test cases.", s => OutputFile = s);
            HasOption("a|append=", "Test Case file to append to", s => AppendTestFile = s);
        }

        public override int Run(string[] remainingArguments)
        {
            _log.InfoFormat("You are generating tests from {0}", InputFile);
            _log.InfoFormat("You are writing out to {0}", OutputFile);

            if (string.IsNullOrWhiteSpace(AppendTestFile))
            {
                _log.Info("Not appending to anything because no append file specified.");
            }
            else
            {
                _log.InfoFormat("Appending to {0}.", AppendTestFile);
                File.Copy(AppendTestFile, OutputFile);
            }

            var fileManager = new SimileNativeAssetManager();

            using (var csvStream = new FileStream(OutputFile, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            using (var shellFile = new FileStream(InputFile, FileMode.Open, FileAccess.ReadWrite))
            using (var model = new CsvModel(csvStream, fileManager))
            {
                var shellModel = new ShellTestCsvModel(shellFile);
                var generator = new TestCaseGenerator(fileManager);
                model.AddTestCases(generator.GenerateTestCases(shellModel.GetTestCases()));
                model.Save();
            }
            
            return 0;
        }
    }
}
