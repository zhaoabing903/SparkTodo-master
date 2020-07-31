﻿namespace WeihanLi.Common.Models
{
    /// <summary>
    /// ReviewState
    /// 审核状态
    /// </summary>
    public enum ReviewState : sbyte
    {
        /// <summary>
        /// UnReviewed
        /// 待审核
        /// </summary>
        UnReviewed = 0,

        /// <summary>
        /// Reviewed
        /// 审核通过
        /// </summary>
        Reviewed = 1,

        /// <summary>
        /// Rejected
        /// 审核被拒绝
        /// </summary>
        Rejected = 2,
    }

    public class ReviewRequest
    {
        public ReviewState State { get; set; }

        public string Remark { get; set; }
    }
}
