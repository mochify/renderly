using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using ManyConsole;
using Common.Logging;
using Mochify.Simile.Core;
using Mochify.Simile.Core.Models.Csv;
using Mochify.Simile.Core.Utils;

namespace Renderly.Commands
{
    public class DeleteTestCaseCommand : ConsoleCommand
    {
        private static readonly ILog _log = LogManager.GetCurrentClassLogger();
        private string ModelFile { get; set; }
        private string OutputFile { get; set; }
        private IEnumerable<DateTime> Dates { get; set; }
        private IEnumerable<string> Releases { get; set; }
        private IEnumerable<int> TestIds { get; set; }

        public DeleteTestCaseCommand()
        {
            Dates = Enumerable.Empty<DateTime>();
            Releases = Enumerable.Empty<string>();
            TestIds = Enumerable.Empty<int>();

            IsCommand("deletetest", "Delete test cases (and their reference images) from a model");
            HasRequiredOption("f|file=", "Model file to delete test cases from.", s => ModelFile = s);
            HasOption("d|dates=", "Comma-separated list of dates to delete test cases for. MM-DD-YYYY format.",
                x => { Dates = x.Split(',').Select(DateTime.Parse); });
            HasOption("r|releases=", "Comma-separated list of releases to delete test cases for",
                x => { Releases = x.Replace(" ", "").Split(','); });
            HasOption("t|testids=", "Comma-separated list of test IDs to delete",
                x => { TestIds = x.Split(',').Select(Int32.Parse); });
            HasOption("o|outfile=", "File to save test cases to. If no file is specified, the input file is overwritten.",
                x => OutputFile = x);
        }


        public override int Run(string[] remainingArguments)
        {
            var file = ModelFile;
            if (!string.IsNullOrWhiteSpace(OutputFile))
            {
                File.Copy(ModelFile, OutputFile);
                file = OutputFile;
            }

            var csvStream = new FileStream(file, FileMode.Open, FileAccess.ReadWrite);
            var fileManager = new SimileNativeAssetManager();

            using (var model = new CsvModel(csvStream, fileManager))
            {
                if (!Dates.Any() && !Releases.Any() && !TestIds.Any())
                {
                    _log.Info("Nothing to do, please specify at least one of dates|releases|test ids");
                }
                else
                {
                    var predicate = PredicateBuilder.False<TestCase>();

                    foreach (var d in Dates)
                    {
                        predicate = predicate.Or(x => x.DateAdded.Date == d.Date);
                    }

                    foreach (var s in Releases)
                    {
                        predicate = predicate.Or(x => x.Release == s);
                    }

                    foreach (var t in TestIds)
                    {

                        predicate = predicate.Or(x => x.TestId == t);
                    }

                    var deleted = model.Delete(predicate.Compile());
                    _log.InfoFormat("Deleted {0} test cases from {1}", deleted, ModelFile);
                    model.Save();
                }
            }

            return 0;
        }
    }
}