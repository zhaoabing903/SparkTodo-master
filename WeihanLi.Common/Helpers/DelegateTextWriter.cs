﻿using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace WeihanLi.Common.Helpers
{
    public class DelegateTextWriter : TextWriter
    {
        public override Encoding Encoding => Encoding.UTF8;

        private readonly Action<string> _onLineWritten;
        private readonly StringBuilder _builder;

        public DelegateTextWriter(Action<string> onLineWritten)
        {
            _onLineWritten = onLineWritten ?? throw new ArgumentNullException(nameof(onLineWritten));

            _builder = new StringBuilder();
        }

        public override void Flush()
        {
            if (_builder.Length > 0)
            {
                FlushInternal();
            }
        }

        public override Task FlushAsync()
        {
            if (_builder.Length > 0)
            {
                FlushInternal();
            }

            return TaskHelper.CompletedTask;
        }

        public override void Write(char value)
        {
            if (value == '\n')
            {
                FlushInternal();
            }
            else
            {
                _builder.Append(value);
            }
        }

        public override Task WriteAsync(char value)
        {
            if (value == '\n')
            {
                FlushInternal();
            }
            else
            {
                _builder.Append(value);
            }
            return TaskHelper.CompletedTask;
        }

        private void FlushInternal()
        {
            _onLineWritten(_builder.ToString());
            _builder.Clear();
        }
    }
}
