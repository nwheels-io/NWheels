namespace NWheels.UI.Model
{
    public sealed class Empty
    {
        public static readonly Empty Value = new Empty();
        
        public class Props
        {
            public static implicit operator Props(Empty e)
            {
                return new Props();
            }
        }
        
        public class Data
        {            
            public static implicit operator Data(Empty e)
            {
                return new Data();
            }
        }

        public class State
        {            
            public static implicit operator State(Empty e)
            {
                return new State();
            }
        }

        public class Actions
        {            
            public static implicit operator Actions(Empty e)
            {
                return new Actions();
            }
        }
    }
}