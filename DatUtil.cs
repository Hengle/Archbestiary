using PoeSharp.Filetypes.Dat;

namespace Archbestiary.Util {
    public static class DatUtil {
        public static string GetID(this DatRow r) { return r["Id"].GetString(); }
        public static string GetName(this DatRow r) { return r["Name"].GetString(); }

        public static string GetStringNoNull(this DatValue v) {
            string ret = v.GetString();
            if (ret is null) ret = "";
            return ret;
        }
    }
}
