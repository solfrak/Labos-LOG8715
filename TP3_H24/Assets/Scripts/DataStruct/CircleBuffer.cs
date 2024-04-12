namespace DataStruct
{
    public class CircleBuffer<T>
    {

        public CircleBuffer(int size)
        {
            m_Buffer = new T[size];

            m_Size = size;
        }
        
        public T this[int index]
        {
            get
            {
                return m_Buffer[index % m_Size];
            }
            
            set
            {
                m_Buffer[index % m_Size] = value;
            }
        }

        private T[] m_Buffer;
        private int m_Size;
    }
}