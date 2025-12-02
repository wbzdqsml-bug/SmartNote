namespace SmartNote.Domain.Entities.Enums
{
    /// <summary>
    /// 笔记行为类型（用于学习热力图 / 趋势分析）
    /// </summary>
    public enum NoteActivityType
    {
        /// <summary>
        /// 创建笔记
        /// </summary>
        Created = 0,

        /// <summary>
        /// 更新笔记内容 / 标题 / 分类
        /// </summary>
        Updated = 1,

        /// <summary>
        /// 标签更新（添加 / 删除标签）
        /// </summary>
        TagUpdated = 2,

        /// <summary>
        /// 软删除笔记（移动到回收站）
        /// </summary>
        SoftDeleted = 3
    }
}
