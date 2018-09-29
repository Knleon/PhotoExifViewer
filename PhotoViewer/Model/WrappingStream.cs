using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoViewer.Model
{
    /// <summary>
    /// MemoryStreamのラッパークラス
    /// </summary>
    /// <remarks>
    /// Dispose時に、内部ストリームの参照を外して、メモリリークを回避
    /// </remarks>
    public class WrappingStream:Stream
    {
        Stream streamBase;
        
        public WrappingStream(Stream _stream)
        {
            if (_stream == null)
            {
                throw new ArgumentNullException("streamBase");
            }
            streamBase = _stream;
        }

        public override bool CanRead
        {
            get { return streamBase == null ? false : streamBase.CanRead; }
        }

        public override bool CanSeek
        {
            get { return streamBase == null ? false : streamBase.CanSeek; }
        }


        public override bool CanWrite
        {
            get { return streamBase == null ? false : streamBase.CanWrite; }
        }

        public override long Length
        {
            get { ThrowIfDisposed(); return streamBase.Length; }
        }

        public override long Position
        {
            get { ThrowIfDisposed(); return streamBase.Position; }
            set { ThrowIfDisposed(); streamBase.Position = value; }
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            ThrowIfDisposed();
            return streamBase.BeginRead(buffer, offset, count, callback, state);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            ThrowIfDisposed();
            return streamBase.BeginWrite(buffer, offset, count, callback, state);
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            ThrowIfDisposed();
            return streamBase.EndRead(asyncResult);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            ThrowIfDisposed();
            streamBase.EndWrite(asyncResult);
        }

        public override void Flush()
        {
            ThrowIfDisposed();
            streamBase.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            ThrowIfDisposed();
            return streamBase.Read(buffer, offset, count);
        }

        public override int ReadByte()
        {
            ThrowIfDisposed();
            return streamBase.ReadByte();
        }


        public override long Seek(long offset, SeekOrigin origin)
        {
            ThrowIfDisposed();
            return streamBase.Seek(offset, origin);
        }


        public override void SetLength(long value)
        {
            ThrowIfDisposed();
            streamBase.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            ThrowIfDisposed();
            streamBase.Write(buffer, offset, count);
        }

        public override void WriteByte(byte value)
        {
            ThrowIfDisposed();
            streamBase.WriteByte(value);
        }

        protected Stream WrappedStream
        {
            get { return streamBase; }
        }

        protected override void Dispose(bool disposing)
        {
            // doesn't close the base stream, but just prevents access to it through this WrappingStream
            if (disposing)
                streamBase = null;
            base.Dispose(disposing);
        }


        private void ThrowIfDisposed()
        {
            // throws an ObjectDisposedException if this object has been disposed
            if (streamBase == null)
                throw new ObjectDisposedException(GetType().Name);
        }
    }
}
