using System;
namespace DOL.Database2
{
    public interface IDatabaseObject
    {
        bool AutoSave { get; set; }
        void DeleteDB();
        UInt64 ObjectId { get; }
        UInt64 ID { get; }
        void Save();
        bool WriteToDatabase { get; set; }
    }
}
