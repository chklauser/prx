namespace Prx.Tests
{
    public class TestObject
    {
        public int Subject = 43;

        public int Count
        {
            get { return Subject; }
            set { Subject = value; }
        }
    }
}