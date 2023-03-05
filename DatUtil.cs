using PoeSharp.Filetypes.Dat;
using System.Runtime.Intrinsics.X86;
using System.Text;

namespace Archbestiary.Util {
    public static class DatUtil {
        public static string GetID(this DatRow r) { return r["Id"].GetString(); }
        public static string GetName(this DatRow r) { return r["Name"].GetString(); }
        public static string GetString(this DatRow r, string col) { return r[col].GetString(); }
        public static int GetInt(this DatRow r, string col) { return r[col].GetPrimitive<int>(); }
        public static bool GetBool(this DatRow r, string col) { return r[col].GetPrimitive<bool>(); }

        public static DatReference GetRef(this DatRow r, string col) { return r[col].GetReference(); }
        public static DatReference[] GetRefArray(this DatRow r, string col) { return r[col].GetReferenceArray(); }
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

        public static string Dump(this DatRow r) {
            StringBuilder s = new StringBuilder(r.rowIndex.ToString() + "|");


            foreach (var column in r.Parent.Spec.Columns) {

                if (!column.Array) {
                    if (column.Type == PoeSharp.Filetypes.Dat.Specification.ColumnType.I32) {
                        s.Append(r[column.Name].GetPrimitive<int>());
                    } else if (column.Type == PoeSharp.Filetypes.Dat.Specification.ColumnType.Bool) {
                        s.Append(r[column.Name].GetPrimitive<bool>() ? "True" : "False");
                    } else if (column.Type == PoeSharp.Filetypes.Dat.Specification.ColumnType.ForeignRow) {
                        DatReference datRef = r[column.Name].GetReference();
                        if (datRef is not null) {
                            if(datRef.ReferenceDefinition is null) {
                                s.Append(datRef.RowIndex);
                            } else {
                                DatRow refRow = datRef.GetReferencedRow();
                                if (refRow is not null && refRow.Parent.Spec.Columns[0].Name == "Id" && refRow.Parent.Spec.Columns[0].Type == PoeSharp.Filetypes.Dat.Specification.ColumnType.String)
                                    s.Append(refRow.GetID());
                                else s.Append($"{datRef.ReferenceDefinition.Table}_{datRef.RowIndex}");

                            }
                        } else s.Append("null");
                    } else {
                        s.Append("UNKNOWN " + column.Type.ToString());
                    }

                } else {
                    s.Append('[');
                    if (column.Type == PoeSharp.Filetypes.Dat.Specification.ColumnType.I32) {
                        s.Append(string.Join(',', r[column.Name].GetPrimitiveArray<int>()));
                    } else if (column.Type == PoeSharp.Filetypes.Dat.Specification.ColumnType.ForeignRow) {
                        bool notEmpty = false;
                        foreach(DatReference datRef in r[column.Name].GetReferenceArray()) {
                            if (datRef is not null) {
                                notEmpty = true;

                                if (datRef.ReferenceDefinition is null) {
                                    s.Append(datRef.RowIndex + ",");
                                } else {
                                    DatRow refRow = datRef.GetReferencedRow();
                                    if (refRow is not null && refRow.Parent.Spec.Columns[0].Name == "Id" && refRow.Parent.Spec.Columns[0].Type == PoeSharp.Filetypes.Dat.Specification.ColumnType.String)
                                        s.Append(refRow.GetID() + ",");
                                    else s.Append($"{datRef.ReferenceDefinition.Table}_{datRef.RowIndex}" + ",");
                                }
                            }
                        }
                        if (notEmpty) s.Remove(s.Length - 1, 1);
                    } else {
                        s.Append("UNKNOWN " + column.Type.ToString());
                    }
                    s.Append(']');
                }


                    string reference = column.Reference is not null ? column.Reference.Table : "";
                //Console.WriteLine($"{column.Name} {column.Type}{(column.Array ? "_Array" : "")} {column.Offset64} {reference}");
                s.Append('|');
            }
            return s.ToString();
        }
    }
}
