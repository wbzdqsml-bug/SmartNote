using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartNote.Domain.Entities
{
    /// <summary>
    /// 笔记与标签的多对多关系
    /// </summary>
    public class NoteTag
    {
        public int NoteId { get; set; }
        public int TagId { get; set; }

        public Note Note { get; set; } = null!;
        public Tag Tag { get; set; } = null!;
    }
}
