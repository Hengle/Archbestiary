using PoeSharp.Filetypes.Dat;
using System.Text;

namespace Archbestiary.Util {
    public static class DatUtil {
        public static string GetID(this DatRow r) { return r["Id"].GetString(); }
        public static string GetName(this DatRow r) { return r["Name"].GetString(); }
        public static string GetString(this DatRow r, string col) { return r[col].GetString(); }
        public static int GetInt(this DatRow r, string col) { return r[col].GetPrimitive<int>(); }
        public static DatRow GetRow(this DatRow r, string col) { return r[col].GetReference().GetReferencedRow(); }

        public static string GetStringNoNull(this DatValue v) {
            string ret = v.GetString();
            if (ret is null) ret = "";
            return ret;
        }


        public static string GetReferenceArrayIDsFormatted(this DatRow r, string col) {
            var refs = r[col].GetReferenceArray();
            StringBuilder s  = new StringBuilder();
            for (int i = 0; i < refs.Length; i++) s.Append(refs[i].GetReferencedRow().GetID() + ", ");
            if(s.Length > 0) s.Remove(s.Length - 2, 2);
            return s.ToString();
        }
    }
}
