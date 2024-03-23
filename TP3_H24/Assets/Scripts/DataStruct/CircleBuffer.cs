namespace DataStruct
{
    public class CircleBuffer<T>
    {
        private T[] m_Buffer;
        private int m_Size;

        public CircleBuffer(int size)
        {
            m_Size = size;
            m_Buffer = new T[m_Size];
        }

        public void Put(T data, int index)
        {
            m_Buffer[index % m_Size] = data;
        }

        public T Get(int index)
        {
            return m_Buffer[index % m_Size];
        }

        public void Clear()
        {
            m_Buffer = new T[m_Size];
        }
    }
    
}