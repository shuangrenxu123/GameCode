using System;
using System.Text;
using UnityEngine;

public class ByteBuffer
{
    private readonly byte[] buffer;
    private int readerIndex = 0;
    private int writerIndex = 0;
    //private int markedReaderIndex = 0;
    //private int markedWriterIndex = 0;
    /// <summary>
    /// 字节缓冲区
    /// </summary>
    public ByteBuffer(int size)
    {
        buffer = new byte[size];
        Capacity = size;
    }
    public ByteBuffer(byte[] bytes)
    {
        buffer = bytes;
    }

    /// <summary>
    /// 缓冲区总容量
    /// </summary>
    public int Capacity;
    /// <summary>
    /// 获得缓冲数组
    /// </summary>
    /// <returns></returns>
    public byte[] GetBuffer()
    {
        return buffer;
    }
    /// <summary>
    /// 获得缓冲区容量
    /// </summary>
    /// <returns></returns>
    public int GetCapacity()
    {
        return buffer.Length;
    }
    /// <summary>
    /// 清空缓冲区
    /// </summary>
    public void Clear()
    {
        readerIndex = 0;
        writerIndex = 0;
        //markedReaderIndex = 0;
        //markedWriterIndex = 0;
    }
    /// <summary>
    /// 删除已读的部分，重新初始化数组
    /// writeIndex指向了数组中写入的数据量，而readIndex则是读取的数据量
    /// </summary>
    public void DiscardReadBytes()
    {
        if (readerIndex == 0)
            return;
        if (readerIndex == writerIndex)//如果两者相同，即已经将缓冲区所有的数组都读取完毕了，所以可以认为重置缓冲区
        {
            readerIndex = writerIndex = 0;
        }
        else
        {
            for (int i = 0, j = readerIndex, length = writerIndex - readerIndex; i < length; i++, j++)
            {
                buffer[i] = buffer[j];
            }
            writerIndex -= readerIndex;
            readerIndex = 0;
        }
    }
    #region 读取相关
    /// <summary>
    /// 已经读取的了数量zui
    /// </summary>
    /// <returns></returns>
    public int ReaderIndex()
    {
        return readerIndex;
    }
    /// <summary>
    /// 剩余的读取量
    /// </summary>
    /// <returns></returns>
    public int ReadableBytes()
    {
        return writerIndex - readerIndex;
    }
    /// <summary>
    /// 检测是否可以查询这么多数据
    /// </summary>
    /// <param name="size">要查询的数据长度</param>
    /// <returns></returns>
    public bool IsReadable(int size = 1)
    {
        return (writerIndex - readerIndex) > size;
    }

    #endregion
    #region 写入相关
    /// <summary>
    /// 当前写入了多少数据
    /// </summary>
    /// <returns></returns>
    public int GetWriteIndex()
    {
        return writerIndex;
    }
    /// <summary>
    /// 还可以写入多少数据量
    /// </summary>
    /// <returns></returns>
    public int WriteBytes()
    {
        return Capacity - writerIndex;
    }
    /// <summary>
    /// 是否可以写入数据
    /// </summary>
    /// <param name="size">要写入的数据量</param>
    /// <returns></returns>
    public bool CanWriteable(int size = 1)
    {
        return (Capacity - writerIndex) >= size;
    }


    #endregion
    #region 读取操作
    private void CheckReaderIndex(int length)
    {
        if (readerIndex + length > writerIndex)
        {
            throw new IndexOutOfRangeException();
        }
    }
    public byte[] ReadBytes(int count)
    {
        CheckReaderIndex(count);
        byte[] data = new byte[count];
        Buffer.BlockCopy(buffer, readerIndex, data, 0, count);
        readerIndex += count;
        return data;
    }
    public bool ReadBool()
    {
        CheckReaderIndex(1);
        return buffer[readerIndex++] == 1;
    }
    public byte ReadByte()
    {
        CheckReaderIndex(1);
        return buffer[readerIndex++];
    }
    public int ReadInt()
    {
        CheckReaderIndex(4);
        int result = BitConverter.ToInt32(buffer, readerIndex);
        readerIndex += 4;
        return result;
    }
    public ushort ReadUshort()
    {
        CheckReaderIndex(2);
        ushort result = BitConverter.ToUInt16(buffer, readerIndex);
        readerIndex += 2;
        return result;
    }
    public float ReadFloat()
    {
        CheckReaderIndex(4);
        float result = BitConverter.ToSingle(buffer, readerIndex);
        readerIndex += 4;
        return result;
    }

    public string ReadString()
    {
        ushort length = ReadUshort();
        CheckReaderIndex(length);
        string result = Encoding.UTF8.GetString(buffer, readerIndex, length - 1);
        readerIndex += length;
        return result;
    }
    public Vector2 ReadVector2()
    {
        CheckReaderIndex(8);
        float x = ReadFloat();
        float y = ReadFloat();
        return new Vector2(x, y);
    }
    public Vector3 ReadVector3()
    {
        CheckReaderIndex(12);
        float x = ReadFloat();
        float y = ReadFloat();
        float z = ReadFloat();
        return new Vector3(x, y, z);
    }
    #endregion
    #region 写入操作
    private void CheckWriterIndex(int length)
    {
        if (writerIndex + length > Capacity)
        {
            throw new IndexOutOfRangeException();
        }
    }
    public void WriteBytes(byte[] data)
    {
        WriteBytes(data, 0, data.Length);
    }
    /// <summary>
    /// 写入数据
    /// </summary>
    /// <param name="data">要写入的数组</param>
    /// <param name="offset">要开始写入的下标</param>
    /// <param name="count">写入的数量</param>
    public void WriteBytes(byte[] data, int offset, int count)
    {
        CheckWriterIndex(count);
        Buffer.BlockCopy(data, offset, buffer, writerIndex, count);
        writerIndex += count;
    }

    public void WriteByte(byte value)
    {
        CheckWriterIndex(1);
        buffer[writerIndex++] = value;
    }

    public void WriteFloat(float value)
    {
        byte[] data = BitConverter.GetBytes(value);
        WriteBytes(data);
    }

    public void WriteBool(bool value)
    {
        WriteByte((byte)(value ? 1 : 0));
    }

    public void WriteInt(int value)
    {
        CheckWriterIndex(4);
        byte[] bytes = BitConverter.GetBytes(value);
        WriteBytes(bytes);
    }

    public void WriteUshort(ushort value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        WriteBytes(bytes);
    }

    public void WriteString(string value)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(value);
        int length = bytes.Length + 1;
        if (length > ushort.MaxValue)
            throw new FormatException("字符的数量太多");
        WriteUshort(Convert.ToUInt16(length));
        WriteBytes(bytes);
        WriteByte((byte)'\0');

    }

    public void WriteVector2(Vector2 value)
    {
        CheckWriterIndex(8);
        WriteFloat(value.x);
        WriteFloat(value.y);
    }
    public void WriteVector3(Vector3 value)
    {
        CheckWriterIndex(12);
        WriteFloat(value.x);
        WriteFloat(value.y);
        WriteFloat(value.z);
    }
    #endregion
}
