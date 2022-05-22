using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Archbestiary.Util {
    public class HTMLWriter {

        TextWriter writer;

        public HTMLWriter(Stream stream) { writer = new StreamWriter(stream); }

        public void Write(string text) { writer.Write(text); }
        public void WriteLine(string text) { writer.WriteLine(text); }

        public void Close() { writer.Flush(); writer.Close(); }

        public void WriteTable(params object[] rows) {
            writer.WriteLine(HTML.Table(rows));
        }

        public void WriteTableClass(string hClass, params object[] rows) {
            writer.WriteLine(HTML.TableClass(hClass, rows));
        }


        //public void TableStart(string hClass = null) { if (hClass is null) writer.WriteLine("<table>"); else writer.WriteLine($"<table class=\"{hClass}\">"); }
        //public void TableEnd() { writer.WriteLine("</table>"); }

        public void Cell(string value, string? hClass = null) { if (hClass is null) writer.Write("<td>" + value + "/td"); else writer.Write($"<td class=\"{hClass}\">{value}</td>"); }



        public void ListVertical(params object[] values) {
            for (int i = 0; i < values.Length; i++) writer.WriteLine("<tr><td>" + values[i].ToString() + "</td></tr>");
        }
    }

     public static class HTML {
        public static string Table(params object[] rows) {
            StringBuilder b = new StringBuilder();
            b.AppendLine("<table>");
            for (int i = 0; i < rows.Length; i++) b.AppendLine(rows[i].ToString());
            b.AppendLine("</table>");
            return b.ToString();
        }

        public static string TableClass(string hClass, params object[] rows) {
            StringBuilder b = new StringBuilder();
            b.AppendLine($"<table class=\"{hClass}\">");
            for (int i = 0; i < rows.Length; i++) b.AppendLine(rows[i].ToString());
            b.AppendLine("</table>");
            return b.ToString();
        }

        public static string Row(params object[] cells) {
            if (cells is null) return "";
            StringBuilder b = new StringBuilder();
            b.Append("<tr>");
            for (int i = 0; i < cells.Length; i++) if(cells[i] is not null) b.Append("<td>" + cells[i].ToString() + "</td>");
            b.Append("</tr>");
            return b.ToString();
        }

        public static string RowList(params object?[] rows) {
            if (rows is null) return "";
            StringBuilder b = new StringBuilder();
            for (int i = 0; i < rows.Length; i++) 
                if (rows[i] is not null) 
                    b.Append("<tr><td>" + rows[i].ToString() + "</td></tr>");
            return b.ToString();
        }


        public static string RowClass(string? rowClass, string? cellClass, params object[] cells) {
            StringBuilder b = new StringBuilder();
            b.Append(rowClass is null ? "<tr>" : $"<tr class=\"{rowClass}\">");
            for (int i = 0; i < cells.Length; i++) b.Append(cellClass is null ? $"<td>{cells[i]}</td>" : $"<td class=\"{cellClass}\">{cells[i]}</td>");
            b.Append("</tr>");
            return b.ToString();
        }

        public static string Array(params object[] objs) {
            if (objs is null) return "";
            StringBuilder b = new StringBuilder();
            for (int i = 0; i < objs.Length; i++) b.AppendLine(objs[i].ToString());
            return b.ToString();
        }

        public static string Break() { return "<br/>"; }

        public static string[] ToHTMLTable(this List<string[]> list) {
            string[] rows = new string[list.Count];
            for(int i = 0; i < rows.Length; i++) {
                rows[i] = HTML.Row(list[i]);
            }
            return rows;
        }
    }
}
