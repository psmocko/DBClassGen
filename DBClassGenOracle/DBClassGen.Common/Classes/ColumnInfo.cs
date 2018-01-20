using System;

namespace DBClassGen.Common.Classes {
    public class ColumnInfo {

        public String ColumnName { get; set; }
        public Int32 Ordinal { get; set; }
        public Int32 Size { get; set; }
        public Int32? Precision { get; set; }
        public Int32? Scale { get; set; }
        public bool IsUnique { get; set; }
        public bool IsKey { get; set; }
        public bool AllowDBNull { get; set; }
        public Type DataType { get; set; }
        public bool IsRowId { get; set; }
        public bool IsForeignKey { get; set; }
        public bool IsIdentity { get; set; }
        public String DbDataType { get; set; }        
    }
}
