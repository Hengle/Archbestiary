using PoeSharp.Filetypes.Dat;

namespace Archbestiary.Util {
    public static class DatUtil {
        public static string GetID(this DatRow r) { return r["Id"].GetString(); }
        public static string GetName(this DatRow r) { return r["Name"].GetString(); }
    }
}
