using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delegates.Reports
{
    public interface IReport
    {
        Func<string, string> MakeCaption { get; }
        Func<string, string, string> MakeItem { get; }
        Func<string> BeginList { get; }
        Func<string> EndList { get; }
    }

    public interface IStatistics
    {
        string Caption { get; }
        object MakeStatistics(IEnumerable<double> data);
    }

    public class ReportMakerHtml : IReport
    {
        public Func<string, string> MakeCaption => caption => $"<h1>{caption}</h1>";
        public Func<string, string, string> MakeItem => (valueType, entry) => $"<li><b>{valueType}</b>: {entry}";
        public Func<string> BeginList => () => "<ul>";
        public Func<string> EndList => () => "</ul>";
    }

    public class ReportMakerMarkdown : IReport
    {
        public Func<string, string> MakeCaption => caption => $"## {caption}\n\n";
        public Func<string, string, string> MakeItem => (valueType, entry) => $" * **{valueType}**: {entry}\n\n";
        public Func<string> BeginList => () => "";
        public Func<string> EndList => () => "";
    }

    public class MeanAndStdStatistics : IStatistics
    {
        public string Caption => "Mean and Std";

        public object MakeStatistics(IEnumerable<double> data)
        {
            var dataList = data.ToList();
            var mean = dataList.Average();
            var std = Math.Sqrt(dataList.Select(z => Math.Pow(z - mean, 2)).Sum() / (dataList.Count - 1));

            return new MeanAndStd
            {
                Mean = mean,
                Std = std
            };
        }
    }

    public class MedianStatistics : IStatistics
    {
        public string Caption => "Median";

        public object MakeStatistics(IEnumerable<double> data)
        {
            var list = data.OrderBy(z => z).ToList();
            if (list.Count % 2 == 0)
                return (list[list.Count / 2] + list[list.Count / 2 - 1]) / 2;

            return list[list.Count / 2];
        }
    }

    public static class ReportMakerHelper
    {
        private static ReportMakerHtml ReportMakerHtml { get; } = new ReportMakerHtml();
        private static ReportMakerMarkdown ReportMakerMarkdown { get; } = new ReportMakerMarkdown();
        private static MeanAndStdStatistics MeanAndStdStatistics { get; } = new MeanAndStdStatistics();
        private static MedianStatistics MedianStatistics { get; } = new MedianStatistics();

        private static string MakeReport(IEnumerable<Measurement> measurements, IReport report, IStatistics statistics)
        {
            var data = measurements.ToList();
            var result = new StringBuilder();
            result.Append(report.MakeCaption(statistics.Caption));
            result.Append(report.BeginList());
            result.Append(report.MakeItem("Temperature",
                statistics.MakeStatistics(data.Select(z => z.Temperature)).ToString()));
            result.Append(report.MakeItem("Humidity",
                statistics.MakeStatistics(data.Select(z => z.Humidity)).ToString()));
            result.Append(report.EndList());
            return result.ToString();
        }

        public static string MeanAndStdHtmlReport(IEnumerable<Measurement> data)
        {
            return MakeReport(data, ReportMakerHtml, MeanAndStdStatistics);
        }

        public static string MedianMarkdownReport(IEnumerable<Measurement> data)
        {
            return MakeReport(data, ReportMakerMarkdown, MedianStatistics);
        }

        public static string MeanAndStdMarkdownReport(IEnumerable<Measurement> data)
        {
            return MakeReport(data, ReportMakerMarkdown, MeanAndStdStatistics);
        }

        public static string MedianHtmlReport(IEnumerable<Measurement> data)
        {
            return MakeReport(data, ReportMakerHtml, MedianStatistics);
        }
    }
}