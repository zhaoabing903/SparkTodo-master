﻿using WeihanLi.Extensions;

namespace WeihanLi.Common.Otp
{
    public class TotpOptions
    {
        /// <summary>
        /// 计算 code 的算法
        /// </summary>
        public OtpHashAlgorithm Algorithm { get; set; } = OtpHashAlgorithm.SHA1;

        /// <summary>
        /// 生成的 code 长度
        /// </summary>
        public int Size { get; set; } = 6;

        /// <summary>
        /// 过期时间，单位是秒
        /// </summary>
        public int ExpiresIn { get; set; } = 300;

        private string _salt;

        /// <summary>
        /// Salt
        /// </summary>
        public string Salt
        {
            get => _salt;
            set
            {
                _salt = value;
                SaltBytes = string.IsNullOrEmpty(value) ? null : value.GetBytes();
            }
        }

        internal byte[] SaltBytes { get; private set; }
    }
}
