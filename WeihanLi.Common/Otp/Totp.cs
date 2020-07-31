﻿using System;
using System.Security.Cryptography;
using System.Text;

namespace WeihanLi.Common.Otp
{
    public class Totp
    {
        private readonly OtpHashAlgorithm _hashAlgorithm;
        private readonly int _codeSize;

        public Totp() : this(OtpHashAlgorithm.SHA1, 6)
        {
        }

        public Totp(int codeSize) : this(OtpHashAlgorithm.SHA1, codeSize)
        {
        }

        public Totp(OtpHashAlgorithm hashAlgorithm) : this(hashAlgorithm, 6)
        {
        }

        public Totp(OtpHashAlgorithm otpHashAlgorithm, int codeSize)
        {
            _hashAlgorithm = otpHashAlgorithm;

            // valid input parameter
            if (codeSize <= 0 || codeSize >= 10)
            {
                throw new ArgumentOutOfRangeException(nameof(codeSize), codeSize, "length must between 1 and 9");
            }
            _codeSize = codeSize;
        }

        private static readonly Encoding _encoding = new UTF8Encoding(false, true);

        public virtual string Compute(string securityToken) => Compute(_encoding.GetBytes(securityToken));

        public virtual string Compute(byte[] securityToken) => Compute(securityToken, GetCurrentTimeStepNumber());

        private string Compute(byte[] securityToken, long counter)
        {
            HMAC hmac;
            switch (_hashAlgorithm)
            {
                case OtpHashAlgorithm.SHA1:
                    hmac = new HMACSHA1(securityToken);
                    break;

                case OtpHashAlgorithm.SHA256:
                    hmac = new HMACSHA256(securityToken);
                    break;

                case OtpHashAlgorithm.SHA512:
                    hmac = new HMACSHA512(securityToken);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(_hashAlgorithm), _hashAlgorithm, null);
            }

            using (hmac)
            {
                var stepBytes = BitConverter.GetBytes(counter);
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(stepBytes); // need BigEndian
                }
                // See https://tools.ietf.org/html/rfc4226
                var hashResult = hmac.ComputeHash(stepBytes);

                var offset = hashResult[hashResult.Length - 1] & 0xf;
                var p = "";
                for (var i = 0; i < 4; i++)
                {
                    p += hashResult[offset + i].ToString("X2");
                }
                var num = Convert.ToInt64(p, 16) & 0x7FFFFFFF;

                //var binaryCode = (hashResult[offset] & 0x7f) << 24
                //                 | (hashResult[offset + 1] & 0xff) << 16
                //                 | (hashResult[offset + 2] & 0xff) << 8
                //                 | (hashResult[offset + 3] & 0xff);

                var code = (num % (int)Math.Pow(10, _codeSize)).ToString("");
                return code.PadLeft(_codeSize, '0');
            }
        }

        public virtual bool Verify(string securityToken, string code) => Verify(_encoding.GetBytes(securityToken), code);

        public virtual bool Verify(string securityToken, string code, TimeSpan timeToleration) => Verify(_encoding.GetBytes(securityToken), code, timeToleration);

        public virtual bool Verify(byte[] securityToken, string code) => Verify(securityToken, code, TimeSpan.Zero);

        public virtual bool Verify(byte[] securityToken, string code, TimeSpan timeToleration)
        {
            if (string.IsNullOrWhiteSpace(code))
                return false;

            if (code.Length != _codeSize)
                return false;

            var step = GetCurrentTimeStepNumber();

            var futureStep = Math.Min((int)(timeToleration.TotalSeconds * TimeSpan.TicksPerSecond / TimeStepTicks), step);
            for (var i = 0; i <= futureStep; i++)
            {
                var totp = Compute(securityToken, step - i);
                if (totp == code)
                {
                    return true;
                }
            }
            return false;
        }

        public int RemainingSeconds()
        {
            return (int)(TimeStepTicks - ((DateTime.UtcNow.Ticks - _unixEpochTicks) / TimeSpan.TicksPerSecond) % TimeStepTicks);
        }

        private static readonly long _unixEpochTicks = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;

        /// <summary>
        /// time step
        /// 30s(Recommend)
        /// </summary>
        private const long TimeStepTicks = TimeSpan.TicksPerSecond * 30;

        // More info: https://tools.ietf.org/html/rfc6238#section-4
        private static long GetCurrentTimeStepNumber() => (DateTime.UtcNow.Ticks - _unixEpochTicks) / TimeStepTicks;
    }
}
